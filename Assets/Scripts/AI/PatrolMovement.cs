using System.Collections;
using UnityEngine;

public class PatrolMovement : MonoBehaviour, IMovement
{
    private Vector2[] waypoints;
    private Vector2[] doorWayPoint;
    private int currentWaypoint;
    private float patrolSpeed;
    private bool busy = false;
    private bool isPatrolling = false;

    public PatrolMovement New(Vector2[] waypoints, Vector2[] doorWayPoint, float speed)
    {
        this.waypoints = waypoints;
        patrolSpeed = speed;
        currentWaypoint = 0;
        this.doorWayPoint = doorWayPoint;
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

        // Only attempt door waypoint transition if it hasn't been done already.
        if (!isPatrolling && doorWayPoint.Length != 0 &&
            Vector2.Distance(enemyTransform.position, doorWayPoint[0]) < Vector2.Distance(enemyTransform.position, waypoints[currentWaypoint]))
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

    private IEnumerator MoveDoorWaypointsCoroutine(Rigidbody2D enemyTransform)
    {
        foreach (Vector2 waypoint in doorWayPoint)
        {
            // Continue moving towards this door waypoint until it is reached.
            while (Vector2.Distance(enemyTransform.position, waypoint) > 0.1f)
            {
                Vector2 newPos = Vector2.MoveTowards(enemyTransform.position, waypoint, patrolSpeed * Time.fixedDeltaTime);
                enemyTransform.MovePosition(newPos);
                yield return new WaitForFixedUpdate();
            }
        }
        Debug.Log("Finished moving through door waypoints.");
        busy = false;
        // Mark that we already transitioned through the door.
        isPatrolling = true;
    }

    public void CustomSetter<T>(T var)
    {
        if (var is bool booleanValue){
            this.isPatrolling = booleanValue;
        }
    }
}
