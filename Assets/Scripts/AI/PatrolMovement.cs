using System.Collections;
using UnityEngine;
using System.Linq;
using System;


public class PatrolMovement : MonoBehaviour, IMovement
{
    private Vector2[] waypoints;
    private Vector2[] doorWayPoint;
    private int currentWaypoint;
    private float patrolSpeed;
    private bool busy = false;
    private bool needsRepositioning = false;
    private PlayerDetector playerDetector;

    private KdTree kdTree; // find the best way to doorWayPoint

    public PatrolMovement New(Vector2[] waypoints, Vector2[] doorWayPoint, float speed, PlayerDetector playerDetector)
    {
        this.waypoints = waypoints;
        patrolSpeed = speed;
        currentWaypoint = 0;
        this.doorWayPoint = doorWayPoint;
        kdTree = new KdTree(doorWayPoint);
        this.playerDetector = playerDetector;
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

        // NeedsRepositioning is set to true whatever the enemy is chasing of finding a gun
        // Them if is not aware of the player so is not chasing the player we can restore is patrolling position
        if (doorWayPoint.Length != 0 && needsRepositioning && !playerDetector.GetIsEnemyAwareOfPlayer())
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

    public Vector2 FindClosestWayPoint(Rigidbody2D enemyRigidbody , out int index)
    {
        if (kdTree == null)
        {
            throw new InvalidOperationException("KdTree is not built. Make sure doorWayPoint array is assigned.");
        }

        return kdTree.FindNearest(enemyRigidbody.position, out index);
    }

    private IEnumerator MoveDoorWaypointsCoroutine(Rigidbody2D enemyTransform)
    {
        // Then we continue to move on the path choosing a branch and assuming that indexes of doorWayPoint are ordered
        int closestPointIndex;
        Vector2 closestPoint = FindClosestWayPoint(enemyTransform, out closestPointIndex);

        // while we're far away from the closest way point we move towards it
        while (Vector2.Distance(enemyTransform.position, closestPoint) > 0.1f)
        {
            Vector2 newPos = Vector2.MoveTowards(enemyTransform.position, closestPoint, patrolSpeed * Time.fixedDeltaTime);
            enemyTransform.MovePosition(newPos);
            yield return new WaitForFixedUpdate();
        }

        Debug.Log("Closest point: " + closestPoint);
        Debug.Log("Closest point index: " + closestPointIndex);
        
        /*
        // We choose to continue on the left path or right path of doorWaypoints
        // take and skip methods include the pass index
        Vector2[] subarray = closestPointIndex <=  doorWayPoint.Length / 2 
                            ? doorWayPoint.Take(closestPointIndex).Reverse().ToArray() 
                            : doorWayPoint.Skip(closestPointIndex).Append(doorWayPoint[0]).ToArray();

        Debug.Log("Chosen subarray: " + string.Join(", ", subarray));

        foreach (Vector2 pp in subarray)
        {
            // Continue moving towards this door waypoint until it is reached.
            while (Vector2.Distance(enemyTransform.position, pp) > 0.1f)
            {
                Vector2 newPos = Vector2.MoveTowards(enemyTransform.position, pp, patrolSpeed * Time.fixedDeltaTime);
                enemyTransform.MovePosition(newPos);
                yield return new WaitForFixedUpdate();
            }
        }
        */
        Debug.Log("Finished moving through door waypoints.");
        busy = false;
        // Mark that we already transitioned through the door.
        needsRepositioning = false;
    }

    public void CustomSetter<T>(T var)
    {
        if (var is bool booleanValue)
        {
            this.needsRepositioning = booleanValue;
        }
    }
}
