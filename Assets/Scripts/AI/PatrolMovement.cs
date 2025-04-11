using System.Collections;
using UnityEngine;
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
    int closestPointIndex;
    Vector2 closestPoint = FindClosestWayPoint(enemyTransform, out closestPointIndex);

    // Continue moving through waypoints until we reach the first one
    while (true)
    {
        // Move towards the closest waypoint
        while (Vector2.Distance(enemyTransform.position, closestPoint) > 0.1f)
        {
            Vector2 newPos = Vector2.MoveTowards(enemyTransform.position, closestPoint, patrolSpeed * Time.fixedDeltaTime);
            enemyTransform.MovePosition(newPos);
            yield return new WaitForFixedUpdate();
        }

        Debug.Log("PATROL Return moving to coordinates: " + closestPoint);

        // If we've reached the first waypoint, exit the loop
        if (closestPoint == doorWayPoint[0])
            break;

        // Find the next closest point in the path exluding the current point
        closestPoint = kdTree.FindNearest(enemyTransform.position, closestPointIndex, out closestPointIndex);
    }

    Debug.Log("Finished moving through door waypoints.");
    busy = false;
    needsRepositioning = false;  // Mark that we already transitioned through the door.
}


    public void CustomSetter<T>(T var)
    {
        if (var is bool booleanValue)
        {
            this.needsRepositioning = booleanValue;
        }
    }
}
