using System.Collections;
using UnityEngine;
using System;

public class PatrolMovement : MonoBehaviour, IMovement
{
    private Vector2[] waypoints;
    private int currentWaypoint;
    private float patrolSpeed;
    private bool busy = false;
    private bool needsRepositioning = false;
    private Detector playerDetector;
    private KdTree kdTree;
    private BFSPathfinder bfs;

    public PatrolMovement New(Vector2[] waypoints, Detector playerDetector, KdTree kdTree,  BFSPathfinder bfs , float speed)
    {
        this.waypoints = waypoints;
        patrolSpeed = speed;
        currentWaypoint = 0;
        this.playerDetector = playerDetector;
        this.kdTree = kdTree;
        this.bfs = bfs;
        return this;
    }

    public void Move(Rigidbody2D enemyTransform)
    {
        if (waypoints == null || waypoints.Length == 0)
            return;

        if (busy) // prevent starting a new transition if currently busy
        {
            return;
        }

        // NeedsRepositioning is set to true whatever the enemy is chasing or finding a gun
        // Then if is not aware of the player so is not chasing the player we can restore is patrolling position
        if (needsRepositioning && !playerDetector.GetIsEnemyAwareOfPlayer())
        {
            Debug.Log("Returning to Door Way point for patrolling");
            busy = true;
            StartCoroutine(MoveDoorWaypointsCoroutine(enemyTransform));
            return;
        }

        // Continue moving towards current patrol waypoint:
        Vector2 currentPos = enemyTransform.position;
        Vector2 targetPos = waypoints[currentWaypoint];

        Vector2 newPos = Vector2.MoveTowards(currentPos, targetPos, patrolSpeed * Time.fixedDeltaTime);
        enemyTransform.MovePosition(newPos);

        if (Vector2.Distance(newPos, targetPos) < 0.1f)
        {
            currentWaypoint = (currentWaypoint + 1) % waypoints.Length;
        }
    }

    public Vector2 FindClosestWayPoint(Rigidbody2D enemyRigidbody, out int index)
    {
        if (kdTree == null)
        {
            throw new InvalidOperationException("KdTree is not built. Make sure doorWayPoint array is assigned.");
        }

        return kdTree.FindNearest(enemyRigidbody.position, out index);
    }

    private IEnumerator MoveDoorWaypointsCoroutine(Rigidbody2D enemyTransform)
    {
        Vector2 closestPoint = FindClosestWayPoint(enemyTransform, out _);
        Vector2[] path = bfs.PathToTheFirst(closestPoint);
        Debug.Log("The path will be " + Utils.Functions.Vector2ArrayToString(path));

        foreach (Vector2 v in path)
        {
            while (Vector2.Distance(enemyTransform.position, v) > 0.1f)
            {
                /*
                if (playerDetector.GetIsEnemyAwareOfPlayer()){
                    yield return null;
                }
                */

                Vector2 newPos = Vector2.MoveTowards(enemyTransform.position, v, patrolSpeed * Time.fixedDeltaTime);
                enemyTransform.MovePosition(newPos);
                yield return new WaitForFixedUpdate();
            }
        }

        Debug.Log("Finished moving through door waypoints.");
        busy = false;
        needsRepositioning = false;  // Mark that we already transitioned through the door.
    }

    public string ToString(){
        return "patrolling";
    }

    public void CustomSetter<T>(T var)
    {
        if (var is bool booleanValue)
        {
            this.needsRepositioning = booleanValue;
        }
    }
}
