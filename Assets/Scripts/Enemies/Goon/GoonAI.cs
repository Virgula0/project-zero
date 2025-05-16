using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class AI : MonoBehaviour, IEnemy, IPoints
{
    [SerializeField] private float patrolSpeed = 3f;
    [SerializeField] private float chaseSpeed;
    [SerializeField] private float runAwaySpeed;
    [SerializeField] private float findAWaponSpeed;
    [SerializeField] private float stoppingDistance = 5f; // set to a lower distance when it can equip melee too
    [SerializeField] private Vector2[] patrolWaypoints;
    // exitWaypoints is a vector containing the coordinates of doors or obstacles (manually defined in the editor) 
    // in order to surpass them when chasing the player
    // if the player is on the other side of one of doorPoints within the vector we use that waypoint which indicates the exit 
    // from the room
    // We have the playerObject reference so we know its position easly.
    // They're used as well for getting back in the patrolling room.
    [SerializeField] private Vector2[] exitWaypoints;
    [SerializeField] private AudioClip deathSfx;
    private GameObject player; // we need the position and other ottributes of the player
    private IMovement currentMovement;
    private IMovement patrolMovement;
    private IMovement chaseMovement;
    private IMovement findForAWeapon;
    private IMovement cowardMovement;
    private List<IMovement> listOfMovements = new List<IMovement>();
    private EnemyWeaponManager weaponManager;
    private Detector playerDetector;
    private Rigidbody2D body;
    private KdTree treeStructure;
    private PathFinder bfs;
    private GraphLinker linker;
    private Vector2[] safeExitWaypointsCopy;
    private Vector2[] safePatrolPoint;
    private Dictionary<int, List<int>> originalEnemyConnectionGraph;
    private Dictionary<int, List<int>> originalEnemyConnectionGraphPatrolPoints;
    private WeaponSpawner spawner;
    private PlayerScript playerScript;
    private WeaponManager playerWeaponManager;
    private AudioSource audioSrc;
    private GoonAnimationScript goonAnimationScript;

    // IMPORTANT! define the list of army that this type of enemy (in this case Goon) can equip
    private List<Type> typesThatCanBeEquipped = new List<Type>{
        typeof(IRanged), // the order matters! it says to what type of weapon give priority when searching
        typeof(IMelee)
    };

    private bool awakeReady = false;
    private bool isEnemyDead = false;
    private int basePoint = 10;
    private float originalStoppingDistance;
    private float stunnedEndTime;

    public bool AwakeReady()
    {
        return awakeReady;
    }

    private void Awake()
    {
        if (exitWaypoints == null || exitWaypoints.Length == 0)
        {
            Debug.LogError("ExitWaypoints is null or empty in Awake!");
            return;
        }

        playerDetector = gameObject.GetComponent<Detector>();
        this.linker = new GraphLinker();
        this.safeExitWaypointsCopy = new Vector2[exitWaypoints.Length];
        this.safePatrolPoint = new Vector2[patrolWaypoints.Length];
        Array.Copy(patrolWaypoints, 0, safePatrolPoint, 0, patrolWaypoints.Length);
        Array.Copy(exitWaypoints, 0, safeExitWaypointsCopy, 0, exitWaypoints.Length);
        GraphLinker.Subgraph s = linker.CreateGraph(exitWaypoints, playerDetector.GetObstacleLayers()); // graph connection
        this.originalEnemyConnectionGraph = s.Graph;
        this.exitWaypoints = s.Nodes;
        this.safeExitWaypointsCopy = s.Nodes;
        this.originalEnemyConnectionGraphPatrolPoints = linker.GenerateCircularConnectionGraph(patrolWaypoints); // graph connection
        awakeReady = true;
    }

    IEnumerator Start()
    {
        // 1) perform the usual Awake + component grabs
        GlobalWaypoints glob = InitializeParameters();

        // 2) wait until our central graph is ready
        while (!glob.GetIsGlobalReady())
            yield return null;

        // 3) get built structures
        bfs = glob.GetPathFinder();
        treeStructure = glob.GetKdTree();

        // 4) Set up Movement‐objects just as before
        FinalizeInitialization();
    }

    private GlobalWaypoints InitializeParameters()
    {
        // Update speeds and obtain components
        this.chaseSpeed = patrolSpeed * 3.5f + chaseSpeed;
        this.findAWaponSpeed = patrolSpeed * 2f + findAWaponSpeed;
        this.runAwaySpeed = patrolSpeed * 4 + runAwaySpeed;

        this.spawner = GameObject.FindGameObjectWithTag(Utils.Const.WEAPON_SPAWNER_TAG).GetComponent<WeaponSpawner>();
        this.body = transform.parent.GetComponentInChildren<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag(Utils.Const.PLAYER_TAG);
        playerScript = player.GetComponent<PlayerScript>();
        this.goonAnimationScript = transform.parent.GetComponentInChildren<GoonAnimationScript>();
        playerWeaponManager = player.GetComponentInChildren<WeaponManager>();
        audioSrc = transform.parent.GetComponent<AudioSource>();

        // Get the global waypoints object locally (no new instance variable)
        GlobalWaypoints glob = GameObject.FindGameObjectWithTag(Utils.Const.GLOBAL_WAYPOINTS_TAG)
                                      .GetComponent<GlobalWaypoints>();

        // Debug.Log("before : " + Utils.Functions.Vector2ArrayToString(exitWaypoints));
        return glob;
    }

    private void FinalizeInitialization()
    {
        // Enemy Weapon manager
        weaponManager = transform.parent.GetComponentInChildren<EnemyWeaponManager>();
        weaponManager.SetWeaponThatCanBeEquipped(typesThatCanBeEquipped);

        Func<float> getStoppingDistance = () => stoppingDistance;
        // Add movement components and initialize them
        patrolMovement = gameObject.AddComponent<PatrolMovement>()
            .New(exitWaypoints[0], patrolWaypoints, playerDetector, treeStructure, bfs, patrolSpeed);
        chaseMovement = gameObject.AddComponent<ChaseMovement>()
            .New(player, playerDetector, treeStructure, bfs, chaseSpeed, getStoppingDistance);
        findForAWeapon = gameObject.AddComponent<WeaponFinderMovement>()
            .New(treeStructure, bfs, typesThatCanBeEquipped, playerDetector, spawner, weaponManager, findAWaponSpeed);
        cowardMovement = gameObject.AddComponent<CowardMovement>()
            .New(exitWaypoints[0], patrolWaypoints, treeStructure, bfs, playerDetector, runAwaySpeed);

        listOfMovements.Add(patrolMovement);
        listOfMovements.Add(chaseMovement);
        listOfMovements.Add(findForAWeapon);
        listOfMovements.Add(cowardMovement);

        // Set the default movement and get the enemy weapon manager
        currentMovement = patrolMovement;
        originalStoppingDistance = this.stoppingDistance;
    }

    // Refactored for clarity and maintainability while preserving original logic
    void FixedUpdate()
    {

        if (patrolWaypoints.Length < 1 || exitWaypoints.Length < 1)
        {
            Debug.LogError(ToString() + " must have at list one patrol waypoint and one exit waypoint");
            return;
        }

        if (weaponManager == null || currentMovement == null)
        {
            return;
        }

        if (HandleStun() | HandleDeathOrPlayerDown())
        {
            // Enemy or player dead => actions stopped
            // Already stunned and handled
            return;
        }

        UpdateAwarenessAndWeaponStatus();
        UpdateStoppingDistance();

        if (ProcessWeaponFind())
        {
            return;
        }

        if (!playerDetector.GetIsEnemyAwareOfPlayer())
        {
            ProcessPatrol();
            return;
        }

        ProcessChase();
    }

    // Returns true if stun was active and handled
    private bool HandleStun()
    {
        if (!IsStunned())
            return false;

        // Stop movement and shooting
        body.linearVelocity = Vector2.zero;
        weaponManager.ChangeEnemyStatus(false);
        return true;
    }

    // Returns true if enemy or player death was handled
    private bool HandleDeathOrPlayerDown()
    {
        if (!isEnemyDead && playerScript.IsPlayerAlive())
            return false;

        // Stop everything
        body.linearVelocity = Vector2.zero;
        playerDetector.SetStopDetector(true); // stop detector since raycast circle call can be expensive
        weaponManager.ChangeEnemyStatus(false); // stop shooting
        foreach (IMovement mov in listOfMovements)
        {
            mov.StopCoroutines(true);
        }

        if (isEnemyDead)
        {
            audioSrc.PlayOneShot(deathSfx);
            goonAnimationScript.SetGoonDeadSprite(playerWeaponManager.GetCurrentLoadedWeapon());
        }

        currentMovement = null;

        return true;
    }

    private void UpdateAwarenessAndWeaponStatus()
    {
        bool isHidden = playerDetector.GetIsPlayerHiddenByObstacle();
        bool isAware = playerDetector.GetIsEnemyAwareOfPlayer();

        weaponManager.SetIsPlayerBehindAWall(isHidden);
        weaponManager.ChangeEnemyStatus(isAware);
    }

    private void UpdateStoppingDistance()
    {
        // set min distance for melee weapons
        if (weaponManager.GetCurrentLoadedWeapon() is IMelee melee)
        {
            stoppingDistance = melee.MinDistanceForSwing();
            return;
        }

        stoppingDistance = originalStoppingDistance;
    }

    // Returns true if a weapon-find action was performed
    private bool ProcessWeaponFind()
    {
        if (!weaponManager.NeedsToFindAWeapon())
            return false;

        currentMovement = findForAWeapon;

        if (currentMovement is WeaponFinderMovement finder)
        {
            bool hasNearbyWeapon = finder.CloserWeaponToEnemy(body.position, null).HasValue;
            if (!hasNearbyWeapon)
            {
                currentMovement = cowardMovement;
            }
            else
            {
                // Stop coward behavior when weapon available
                cowardMovement.StopCoroutines(true);
            }
        }

        currentMovement.Move(body);
        return true;
    }

    private void ProcessPatrol()
    {
        if (currentMovement != patrolMovement)
        {
            patrolMovement.NeedsRepositioning(true);
        }

        currentMovement = patrolMovement;
        currentMovement.Move(body);
    }

    private void ProcessChase()
    {
        currentMovement = chaseMovement;
        currentMovement.Move(body);
    }

    public Vector2[] GetEnemyWaypoints()
    {
        return this.safeExitWaypointsCopy;
    }

    public Dictionary<int, List<int>> GetEnemyConnections()
    {
        return this.originalEnemyConnectionGraph;
    }

    // Debugging purposes you can ignore this
    private void OnDrawGizmos()
    {
        if (transform.position == null)
            return;

        if (patrolWaypoints == null || exitWaypoints == null)
        {
            return;
        }

        float circleRadius = 0.8f;
        foreach (Vector2 point in patrolWaypoints)
        {
            // Calculate the starting point and ending point of the cast
            Vector3 startPoint = point;
            Vector3 endPoint = startPoint + (Vector3)(new Vector2(0, 0).normalized * circleRadius);

            // Set Gizmo color for visualization
            Gizmos.color = Color.cyan;

            // Draw the starting circle
            Gizmos.DrawWireSphere(startPoint, circleRadius);
            // Draw the ending circle
            Gizmos.DrawWireSphere(endPoint, circleRadius);

            // Draw a line connecting the two circles to illustrate the cast path
            Gizmos.DrawLine(startPoint, endPoint);
        }

        foreach (Vector2 point in exitWaypoints)
        {
            // Calculate the starting point and ending point of the cast
            Vector3 startPoint = point;
            Vector3 endPoint = startPoint + (Vector3)(new Vector2(0, 0).normalized * circleRadius);

            // Set Gizmo color for visualization
            Gizmos.color = Color.white;

            // Draw the starting circle
            Gizmos.DrawWireSphere(startPoint, circleRadius);
            // Draw the ending circle
            Gizmos.DrawWireSphere(endPoint, circleRadius);

            // Draw a line connecting the two circles to illustrate the cast path
            Gizmos.DrawLine(startPoint, endPoint);
        }
    }

    public bool IsEnemyDead()
    {
        return isEnemyDead;
    }

    public void SetIsEnemyDead(bool cond)
    {
        Utils.Functions.SetLayerRecursively(transform.parent.gameObject, (int)Utils.Enums.ObjectLayers.BodyOnTheGround);
        this.isEnemyDead = cond;
    }

    public int GetBasePoints()
    {
        return basePoint;
    }

    public int GetTotalShotsDelivered()
    {
        return weaponManager.GetTotalShotsDelivered();
    }

    public float GetTotalChasedTime()
    {
        return playerDetector.GetTotalChasedTime();
    }

    public void SetIsEnemyStunned(float duration = 3f) // 3 seconds by default
    {
        stunnedEndTime = Time.time + duration;
    }

    public bool IsStunned()
    {
        return Time.time < stunnedEndTime;
    }

    public IMovement GetCurrentMovement()
    {
        return currentMovement;
    }

    public Vector2[] GetEnemyPatrolPoints()
    {
        return this.safePatrolPoint;
    }

    public Dictionary<int, List<int>> GetEnemyConnectionsPatrolPoints()
    {
        return this.originalEnemyConnectionGraphPatrolPoints;
    }
}
