using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class AI : MonoBehaviour
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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        this.chaseSpeed = patrolSpeed * 3;
        this.body = transform.parent.GetComponentInChildren<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag(Utils.Const.PLAYER_TAG);
        playerDetector = gameObject.GetComponent<Detector>();
        this.treeStructure = new KdTree(exitWaypoints);

        Dictionary<int, List<int>> connections = Utils.Level1.Scene1.GOON_1_CONNECTIONS;
        
        GlobalWaypoints glob = GameObject.FindGameObjectWithTag(Utils.Const.GLOBAL_WAYPOINTS_TAG).GetComponent<GlobalWaypoints>();
        Dictionary<int,int> dict = glob.GetGlobalWaypointsRemapped();
        // connect each global waypoint to the nearest element of our current Vector2[] set
        foreach (var item in dict){
            Vector2 elemToLink = glob.GetElementFromRemappedIndex(item.Key);
            Vector2 _ = treeStructure.FindNearest(elemToLink, out int index);
            this.exitWaypoints = Utils.Functions.AddToVector2Array(this.exitWaypoints, elemToLink, out int addedIndex); // ad to current Vector2 set
            connections.Add(addedIndex, new List<int> { index }); // add the element actually in the connection graph
            connections[index].Add(addedIndex); // add connection to existing node in connection graph
            treeStructure.UpdateVectorSet(elemToLink); // update the treeStructure
        }

        // Utils.Functions.PrintDictionary(connections);

        // Define connections and build the connection graph
        this.bfs = new BFSPathfinder(exitWaypoints, connections);

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
        if (currentMovement == null){
            return;
        }

        if (!playerDetector.GetIsEnemyAwareOfPlayer())
        {
            // Debug.Log("Enemy is not alerted anymore");
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
}
