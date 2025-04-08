using UnityEngine;

public class PatrolMovement : IMovement
{
    private Vector2[] waypoints;
    private int currentWaypoint;
    private float speed;

    // Constructor accepts a set of waypoints and a movement speed.
    public PatrolMovement(Vector2[] waypoints, float speed)
    {
        this.waypoints = waypoints;
        this.speed = speed;
        currentWaypoint = 0;
    }

    public void Move(Transform enemyTransform)
    {
        if (waypoints == null || waypoints.Length == 0)
            return;

        // Convert the enemy's current position to a Vector2 (ignoring the z-axis).
        Vector2 currentPos = enemyTransform.position;
        Vector2 targetPos = waypoints[currentWaypoint];

        // Move towards the current waypoint.
        Vector2 newPos = Vector2.MoveTowards(currentPos, targetPos, speed * Time.fixedDeltaTime);

        // Update enemy's position while keeping the original z-coordinate.
        enemyTransform.position = new Vector3(newPos.x, newPos.y, enemyTransform.position.z);

        // Switch to the next waypoint if close enough.
        if (Vector2.Distance(newPos, targetPos) < 0.1f)
        {
            currentWaypoint = (currentWaypoint + 1) % waypoints.Length;
        }
    }
}
