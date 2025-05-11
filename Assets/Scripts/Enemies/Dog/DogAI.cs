using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class DogAI : MonoBehaviour, IEnemy, IPoints
{
    [SerializeField] private float patrolSpeed = 3f;
    [SerializeField] private float chaseSpeed;
    [SerializeField] private float runAwaySpeed;
    [SerializeField] private float findAWaponSpeed;
    [SerializeField] private float stoppingDistance = 2f; // set to a lower distance when it can equip melee too
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
    private PlayerScript playerScript;
    private WeaponManager playerWeaponManager;
    private AudioSource audioSrc;
    private DogAnimationScript animationScript;
    private bool awakeReady = false;
    private bool isEnemyDead = false;
    private int basePoint = 30;
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

        this.linker = new GraphLinker();
        this.safeExitWaypointsCopy = new Vector2[exitWaypoints.Length];
        this.safePatrolPoint = new Vector2[patrolWaypoints.Length];
        Array.Copy(patrolWaypoints, 0, safePatrolPoint, 0, patrolWaypoints.Length);
        Array.Copy(exitWaypoints, 0, safeExitWaypointsCopy, 0, exitWaypoints.Length);
        this.originalEnemyConnectionGraph = linker.GenerateConnections(exitWaypoints);
        this.originalEnemyConnectionGraphPatrolPoints = linker.GenerateCircularConnectionGraph(patrolWaypoints); // graph connection
        awakeReady = true;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
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

        // 4) Set up Movement‐objects
        FinalizeInitialization();
    }

    private GlobalWaypoints InitializeParameters()
    {
        // Update speeds and obtain components
        this.chaseSpeed = patrolSpeed * 3.5f + chaseSpeed;
        this.findAWaponSpeed = patrolSpeed * 2f + findAWaponSpeed;
        this.runAwaySpeed = patrolSpeed * 4 + runAwaySpeed;

        this.body = transform.parent.GetComponentInChildren<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag(Utils.Const.PLAYER_TAG);
        playerDetector = gameObject.GetComponent<Detector>();
        playerScript = player.GetComponent<PlayerScript>();
        this.animationScript = transform.parent.GetComponentInChildren<DogAnimationScript>();
        playerWeaponManager = player.GetComponentInChildren<WeaponManager>();
        audioSrc = transform.parent.GetComponent<AudioSource>();

        // Get the global waypoints object locally (no new instance variable)
        GlobalWaypoints glob = GameObject.FindGameObjectWithTag(Utils.Const.GLOBAL_WAYPOINTS_TAG)
                                      .GetComponent<GlobalWaypoints>();

        return glob;
    }
    private void FinalizeInitialization()
    {
        // Enemy Weapon manager
        weaponManager = transform.parent.GetComponentInChildren<EnemyWeaponManager>();

        Func<float> getStoppingDistance = () => stoppingDistance;
        // Add movement components and initialize them
        patrolMovement = gameObject.AddComponent<PatrolMovement>()
            .New(exitWaypoints[0], patrolWaypoints, playerDetector, treeStructure, bfs, patrolSpeed);
        chaseMovement = gameObject.AddComponent<ChaseMovement>()
            .New(player, playerDetector, treeStructure, bfs, chaseSpeed, getStoppingDistance);

        listOfMovements.Add(patrolMovement);
        listOfMovements.Add(chaseMovement);

        // Set the default movement and get the enemy weapon manager
        currentMovement = patrolMovement;
        originalStoppingDistance = this.stoppingDistance;
    }

    // Refactored for clarity and maintainability while preserving original logic
    void FixedUpdate()
    {
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
            animationScript.SetDogDeadSprite(playerWeaponManager.GetCurrentLoadedWeapon());
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
