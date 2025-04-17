using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq; // pay attention, as far as I read this library is not that fast

public class AI : MonoBehaviour, IEnemy
{
    [SerializeField] private float patrolSpeed = 3f;
    [SerializeField] private float chaseSpeed;
    [SerializeField] private float findAWaponSpeed;

    [SerializeField] private float stoppingDistance = 5f;
    [SerializeField] private Vector2[] patrolWaypoints;
    // exitWaypoints is a vector containing the coordinates of doors or obstacles (manually defined in the editor) 
    // in order to surpass them when chasing the player
    // if the player is on the other side of one of doorPoints within the vector we use that waypoint which indicates the exit 
    // from the room
    // We have the playerObject reference so we know its position easly.
    // They're used as well for getting back in the patrolling room.
    [SerializeField] private Vector2[] exitWaypoints;
    private GameObject player; // we need the position and other ottributes of the player
    private IMovement currentMovement;
    private IMovement patrolMovement;
    private IMovement chaseMovement;
    private IMovement findForAWeapon;
    private EnemyWeaponManager weaponManager;
    private Detector playerDetector;
    private Rigidbody2D body;
    private KdTree treeStructure;
    private BFSPathfinder bfs;
    private GraphLinker linker;
    private Vector2[] safeExitWaypointsCopy;
    private Dictionary<int, List<int>> connectionGraph;
    private Dictionary<int, List<int>> originalEnemyConnectionGraph;
    private WeaponSpawner spawner;

    // IMPORTANT! define the list of army that this type of enemy (in this case Goon) can equip
    private List<Type> typesThatCanBeEquipped = new List<Type>{
        typeof(IRanged),
        typeof(IMelee)
    };

    private void Awake()
    {
        this.linker = new GraphLinker();
        this.safeExitWaypointsCopy = new Vector2[exitWaypoints.Length];
        Array.Copy(exitWaypoints, 0, safeExitWaypointsCopy, 0, exitWaypoints.Length);
        this.originalEnemyConnectionGraph = linker.GenerateConnections(exitWaypoints);
        this.connectionGraph = originalEnemyConnectionGraph.ToDictionary(entry => entry.Key, entry => new List<int>(entry.Value)); // deep copy
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Perform initializations and get global waypoints
        GlobalWaypoints glob = InitializeParameters();
        // Connect the global waypoints into our local graph
        ConnectGlobalWaypoints(glob);
        // Connect waypoints from other enemies using the global waypoints reference
        ConnectOtherEnemyWaypoints(glob);
        // Finalize the initialization: log info and set up pathfinder and movements
        FinalizeInitialization();
    }

    private GlobalWaypoints InitializeParameters()
    {
        // Update speeds and obtain components
        this.chaseSpeed = patrolSpeed * 3 + chaseSpeed;
        this.findAWaponSpeed = patrolSpeed * 1.5f + findAWaponSpeed;

        this.spawner = GameObject.FindGameObjectWithTag(Utils.Const.WEAPON_SPAWNER).GetComponent<WeaponSpawner>();
        this.body = transform.parent.GetComponentInChildren<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag(Utils.Const.PLAYER_TAG);
        playerDetector = gameObject.GetComponent<Detector>();
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

            // Merge connection graphs using the linker helper
            this.connectionGraph = linker.LinkGraphs(
                this.connectionGraph,
                glob.GetConnectionMapForAnEnemy(enemy),
                this.exitWaypoints,
                enemyWaypoints
            );

            // Add enemy waypoints to the current set and update the kd-tree accordingly
            foreach (Vector2 node in enemyWaypoints)
            {
                this.exitWaypoints = Utils.Functions.AddToVector2Array(this.exitWaypoints, node, out _);
                treeStructure.UpdateVectorSetOnInsert(node);
            }
        }
    }

    private void FinalizeInitialization()
    {
        // Debug.Log("after: " + Utils.Functions.Vector2ArrayToString(exitWaypoints));
        // Debug.Log("connections after");
        // Utils.Functions.PrintDictionary(connectionGraph);

        // Create the BFS pathfinder using the finalized waypoint set and connection graph
        bfs = new BFSPathfinder(exitWaypoints, connectionGraph);

        // Enemy Weapon manager
        weaponManager = transform.parent.GetComponentInChildren<EnemyWeaponManager>();
        weaponManager.SetWeaponThatCanBeEquipped(typesThatCanBeEquipped);

        // Add movement components and initialize them
        patrolMovement = gameObject.AddComponent<PatrolMovement>()
            .New(patrolWaypoints, playerDetector, treeStructure, bfs, patrolSpeed);
        chaseMovement = gameObject.AddComponent<ChaseMovement>()
            .New(player, playerDetector, treeStructure, bfs, chaseSpeed, stoppingDistance);
        findForAWeapon = gameObject.AddComponent<WeaponFinderMovement>()
            .New(treeStructure, bfs, typesThatCanBeEquipped, spawner, weaponManager, findAWaponSpeed);

        // Set the default movement and get the enemy weapon manager
        currentMovement = patrolMovement;
    }


    void FixedUpdate()
    {
        if (currentMovement == null)
        {
            return;
        }

        // if enemy needs a gun we give priority to find a gun, ignoring the player!
        // this logic can change in another enemy type
        if (weaponManager.NeedsToFindAWeapon() && spawner.GetAllWeaponsOnTheGroundPosition().Length > 0)
        {
            currentMovement = findForAWeapon;
            currentMovement.Move(body);
            return;
        }

        if (!playerDetector.GetIsEnemyAwareOfPlayer())
        {
            if (currentMovement != patrolMovement){
                patrolMovement.NeedsRepositioning(true);
            }
            currentMovement = patrolMovement;
            currentMovement.Move(body); // if the enemy does not know about the player
            weaponManager.ChangeEnemyStatus(false);
            return;
        }

        // if here enemy detected player, start shooting and chasing!
        currentMovement = chaseMovement;
        currentMovement?.Move(body); // ?. means that Move will called if currentMovement is not null
        weaponManager.ChangeEnemyStatus(true);
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
}
