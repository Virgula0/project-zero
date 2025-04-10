using System.Collections;
using UnityEngine;

public class PatrolMovement : MonoBehaviour, IMovement
{
    private Vector2[] waypoints;
    private int currentWaypoint;
    private float patrolSpeed;
    private Vector2[] doorWayPoint;
    private bool busy = false;

    public PatrolMovement New(Vector2[] waypoints, Vector2[] doorWayPoint, float speed)
    {
        this.waypoints = waypoints;
        this.patrolSpeed = speed;
        currentWaypoint = 0;
        this.doorWayPoint = doorWayPoint;
        return this;
    }

    private bool wasComingBack = false;

    public void Move(Rigidbody2D enemyTransform)
    {
        if (waypoints == null || waypoints.Length == 0)
            return;

        if (busy) // closure guard to ensure the enemy is not trying to getting back to the doorWayPoint first
        {
            return;
        }

        // if the door entrance is closer than the distance to the current waypoint, we move towards the door waypoints
        // doorWayPoint[0] must be the external one to the romm, the order must be set from the most external one to the most inernal point
        if (doorWayPoint.Length != 0 && Vector2.Distance(enemyTransform.position, doorWayPoint[0]) < Vector2.Distance(enemyTransform.position, waypoints[currentWaypoint]))
        {
            Debug.Log("Returning to Door Way point for patrolling");
            busy = true;
            wasComingBack = true;
            StartCoroutine(MoveDoorWaypointsCoroutine(enemyTransform));
            return;
        }

        // Use the rigidbody's position for accurate physics-based movement.
        Vector2 currentPos = enemyTransform.position;
        Vector2 targetPos = waypoints[currentWaypoint];

        // Compute the new position towards the target.
        Vector2 newPos = Vector2.MoveTowards(currentPos, targetPos, patrolSpeed * Time.fixedDeltaTime);

        // Move the enemy using the physics engine.
        enemyTransform.MovePosition(newPos);

        // Switch to the next waypoint if close enough.
        if (Vector2.Distance(newPos, targetPos) < 0.1f)
        {
            currentWaypoint = (currentWaypoint + 1) % waypoints.Length;
        }
    }

    private IEnumerator MoveDoorWaypointsCoroutine(Rigidbody2D enemyTransform)
    {
        foreach (Vector2 waypoint in doorWayPoint)
        {
            // Continue moving towards this particular door waypoint until it is reached
            while (Vector2.Distance(enemyTransform.position, waypoint) > 0.1f)
            {
                Vector2 newPos = Vector2.MoveTowards(enemyTransform.position, waypoint, patrolSpeed * Time.fixedDeltaTime);
                enemyTransform.MovePosition(newPos);
                yield return new WaitForFixedUpdate();
            }
        }
        Debug.Log("Finished moving through door waypoints.");
        busy = false;
    }
}
