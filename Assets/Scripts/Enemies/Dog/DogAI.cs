using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Collections;

public class DogAI : MonoBehaviour, IEnemy, IPoints
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
    private List<IMovement> listOfMovements = new List<IMovement>();
    private EnemyWeaponManager weaponManager;
    private Detector playerDetector;
    private Rigidbody2D body;
    private KdTree treeStructure;
    private BFSPathfinder bfs;
    private GraphLinker linker;
    private Vector2[] safeExitWaypointsCopy;
    private Dictionary<int, List<int>> connectionGraph;
    private Dictionary<int, List<int>> originalEnemyConnectionGraph;
    private PlayerScript playerScript;
    private WeaponManager playerWeaponManager;
    private AudioSource audioSrc;
    private DogAnimationScript animationScript;

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

        this.linker = new GraphLinker();
        this.safeExitWaypointsCopy = new Vector2[exitWaypoints.Length];
        Array.Copy(exitWaypoints, 0, safeExitWaypointsCopy, 0, exitWaypoints.Length);
        this.originalEnemyConnectionGraph = linker.GenerateConnections(exitWaypoints);
        this.connectionGraph = originalEnemyConnectionGraph.ToDictionary(entry => entry.Key, entry => new List<int>(entry.Value)); // deep copy
        awakeReady = true;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    IEnumerator Start()
    {
        // Perform initializations and get global waypoints
        GlobalWaypoints glob = InitializeParameters();

        while (!glob.GetIsGlobalReady())
        {
            yield return null;
        }

        // Add patrol waypoints
        // ConnectPatrolWaypoints();
        // Connect the global waypoints into our local graph
        ConnectGlobalWaypoints(glob);
        // Connect waypoints from other enemies using the global waypoints reference
        ConnectOtherEnemyWaypoints(glob);
        // Finalize the initialization: log info and set up pathfinder and movements
        FinalizeInitialization(glob);
    }

    private void ConnectPatrolWaypoints()
    {
        // THIS PART IS EXPERIMENTAL
        if (patrolWaypoints == null || patrolWaypoints.Length < 1)
            return;

        // Merge connection graphs using the linker helper
        this.connectionGraph = linker.LinkGraphs(
            this.connectionGraph,
            linker.GenerateConnections(patrolWaypoints),
            this.exitWaypoints,
            patrolWaypoints,
            playerDetector.GetObstacleLayers()
        );

        // Add patrol waypoints to the current set and update the kd-tree accordingly
        foreach (Vector2 node in patrolWaypoints)
        {
            this.exitWaypoints = Utils.Functions.AddToVector2Array(this.exitWaypoints, node, out _);
            treeStructure.UpdateVectorSetOnInsert(node);
        }
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
        animationScript = gameObject.transform.parent.GetComponentInChildren<DogAnimationScript>();
        playerWeaponManager = player.GetComponentInChildren<WeaponManager>();
        audioSrc = transform.parent.GetComponent<AudioSource>();

        treeStructure = new KdTree(exitWaypoints);

        // Get the global waypoints object locally (no new instance variable)
        GlobalWaypoints glob = GameObject.FindGameObjectWithTag(Utils.Const.GLOBAL_WAYPOINTS_TAG)
                                      .GetComponent<GlobalWaypoints>();

        // Debug.Log("before : " + Utils.Functions.Vector2ArrayToString(exitWaypoints));
        return glob;
    }

    private void ConnectGlobalWaypoints(GlobalWaypoints glob)
    {
        // Get the remapped global waypoints dictionary
        Dictionary<int, int> dict = glob.GetGlobalWaypointsRemapped();

        // Connect each global waypoint to the nearest element of our current set
        foreach (var item in dict)
        {
            Vector2 elemToLink = glob.GetElementFromRemappedIndex(item.Key);
            // Find the nearest element using the kd-tree
            Vector2 _ = treeStructure.FindNearest(elemToLink, out int index);
            // Add the global waypoint to our local set and update the connection graph
            this.exitWaypoints = Utils.Functions.AddToVector2Array(this.exitWaypoints, elemToLink, out int addedIndex);
            this.connectionGraph.Add(addedIndex, new List<int> { index });
            this.connectionGraph[index].Add(addedIndex);
            treeStructure.UpdateVectorSetOnInsert(elemToLink);
        }
    }

    private void ConnectOtherEnemyWaypoints(GlobalWaypoints glob)
    {
        // Get the waypoints of other enemies (excluding self)
        List<IEnemy> otherEnemies = glob.GetEnemies(this);

        // For each enemy, link their waypoint graph to our current graph
        foreach (IEnemy enemy in otherEnemies)
        {
            Vector2[] enemyWaypoints = glob.GetWaypointMapForAnEnemy(enemy);

            if (enemyWaypoints == null || enemyWaypoints.Length < 1)
                continue;

            // Merge connection graphs using the linker helper
            this.connectionGraph = linker.LinkGraphs(
                this.connectionGraph,
                glob.GetConnectionMapForAnEnemy(enemy),
                this.exitWaypoints,
                enemyWaypoints,
                playerDetector.GetObstacleLayers()
            );

            // Add enemy waypoints to the current set and update the kd-tree accordingly
            foreach (Vector2 node in enemyWaypoints)
            {
                this.exitWaypoints = Utils.Functions.AddToVector2Array(this.exitWaypoints, node, out _);
                treeStructure.UpdateVectorSetOnInsert(node);
            }
        }
    }

    private void FinalizeInitialization(GlobalWaypoints glob)
    {
        // Debug.Log("after: " + Utils.Functions.Vector2ArrayToString(exitWaypoints));
        // Debug.Log("connections after");
        // Utils.Functions.PrintDictionary(connectionGraph);

        // Create the BFS pathfinder using the finalized waypoint set and connection graph
        bfs = new BFSPathfinder(exitWaypoints, connectionGraph);

        // Enemy Weapon manager
        weaponManager = transform.parent.GetComponentInChildren<EnemyWeaponManager>();

        Func<float> getStoppingDistance = () => stoppingDistance;
        // Add movement components and initialize them
        patrolMovement = gameObject.AddComponent<PatrolMovement>()
            .New(patrolWaypoints, playerDetector, treeStructure, bfs, patrolSpeed);
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
}
