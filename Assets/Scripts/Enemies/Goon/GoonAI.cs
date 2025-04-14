using System;
using System.Collections.Generic;
using UnityEngine;

public class AI : MonoBehaviour, IEnemy
{
    [SerializeField] private float patrolSpeed = 3f;
    [SerializeField] private float chaseSpeed;
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
    private EnemyWeaponManager weaponManager;
    private Detector playerDetector;
    private Rigidbody2D body;
    private KdTree treeStructure;
    private BFSPathfinder bfs;
    private GraphLinker linker;
    private Vector2[] safeExitWaypointsCopy;
    private Dictionary<int, List<int>> connectionGraph;
    private Dictionary<int, List<int>> originalEnemyConnectionGraph;

    private void Awake()
    {
        this.linker = new GraphLinker();
        this.safeExitWaypointsCopy = new Vector2[exitWaypoints.Length];
        Array.Copy(exitWaypoints, 0, safeExitWaypointsCopy, 0, exitWaypoints.Length);
        this.originalEnemyConnectionGraph = Utils.Functions.GenerateConnections(exitWaypoints);
        // TODO: copy not regenerate
        this.connectionGraph = Utils.Functions.GenerateConnections(exitWaypoints);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        this.chaseSpeed = patrolSpeed * 3 + chaseSpeed;
        this.body = transform.parent.GetComponentInChildren<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag(Utils.Const.PLAYER_TAG);
        playerDetector = gameObject.GetComponent<Detector>();
        this.treeStructure = new KdTree(exitWaypoints);
        
        GlobalWaypoints glob = GameObject.FindGameObjectWithTag(Utils.Const.GLOBAL_WAYPOINTS_TAG).GetComponent<GlobalWaypoints>();

        Debug.Log("before : " + Utils.Functions.Vector2ArrayToString(exitWaypoints));
        // Debug.Log("connections before");Utils.Functions.PrintDictionary(connections);
        // CONNECT GLOBAL WAYPOINTS

        /*
        Dictionary<int, int> dict = glob.GetGlobalWaypointsRemapped();
        // connect each global waypoint to the nearest element of our current Vector2[] set
        foreach (var item in dict)
        {
            Vector2 elemToLink = glob.GetElementFromRemappedIndex(item.Key);
            Vector2 _ = treeStructure.FindNearest(elemToLink, out int index);
            this.exitWaypoints = Utils.Functions.AddToVector2Array(this.exitWaypoints, elemToLink, out int addedIndex); // ad to current Vector2 set
            connections.Add(addedIndex, new List<int> { index }); // add the element actually in the connection graph
            connections[index].Add(addedIndex); // add connection to existing node in connection graph
            treeStructure.UpdateVectorSet(elemToLink); // update the treeStructure
        }
        */

        // CONNECT OTHER ENEMIES WAYPOINTS
        List<IEnemy> otherEnemies = glob.GetEnemies(this); // retuns other enemies waypoint (except for me)

        // for each enemy found
        foreach (IEnemy enemy in otherEnemies)
        {   
            Vector2[] enemyWaypoints = glob.GetWaypointMapForAnEnemy(enemy);
            // first link graphs
            this.connectionGraph = linker.LinkGraphs(this.connectionGraph, 
                                glob.GetConnectionMapForAnEnemy(enemy), 
                                this.exitWaypoints, 
                                enemyWaypoints);
    
            foreach (Vector2 node in enemyWaypoints)
            {
                this.exitWaypoints = Utils.Functions.AddToVector2Array(this.exitWaypoints, node, out _); // ad to current Vector2 set
                treeStructure.UpdateVectorSet(node); // update the treeStructure
            }
        }

        Debug.Log("after: " + Utils.Functions.Vector2ArrayToString(exitWaypoints));
        Debug.Log("connections after");Utils.Functions.PrintDictionary(connectionGraph);

        // Define connections and build the connection graph
        this.bfs = new BFSPathfinder(exitWaypoints, connectionGraph);

        // this trick is needed beacuse it needs MohoBehaviour for coroutines
        // and we can't even initialize a new object in unity if it inherits MonoBehaviour
        // this is a little bit shit and I need to do this trick if i want to add manually the script as component 
        // within the game object logic
        patrolMovement = gameObject.AddComponent<PatrolMovement>().New(patrolWaypoints, playerDetector, treeStructure, bfs, patrolSpeed);

        // instantiate normally
        chaseMovement = gameObject.AddComponent<ChaseMovement>().New(player, playerDetector, treeStructure, bfs, chaseSpeed, stoppingDistance);

        // movements
        currentMovement = patrolMovement; // patrol movement when the object is spawned
        weaponManager = gameObject.transform.parent.GetComponentInChildren<EnemyWeaponManager>();
    }

    void FixedUpdate()
    {
        if (currentMovement == null)
        {
            return;
        }

        Debug.Log(currentMovement);

        if (!playerDetector.GetIsEnemyAwareOfPlayer())
        {
            currentMovement = patrolMovement;
            currentMovement.Move(body); // if the enemy does not know about the player
            weaponManager.ChangeEnemyStatus(false);
            return;
        }

        // if here enemy detected player, start shooting and chasing!
        currentMovement.CustomSetter(varToSet: true); // CustomSetter will try to set the needsRepositioning to true
        currentMovement = chaseMovement;
        currentMovement?.Move(body); // ?. means that Move will called if currentMovement is not null
        weaponManager.ChangeEnemyStatus(true);
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

    public Vector2[] GetEnemyWaypoints()
    {
        return this.safeExitWaypointsCopy;
    }

    public Dictionary<int, List<int>> GetEnemyConnections()
    {
        return this.originalEnemyConnectionGraph;
    }
}
