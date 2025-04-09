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

    public void Move(Rigidbody2D enemyTransform)
    {
        if (waypoints == null || waypoints.Length == 0)
            return;

        // Use the rigidbody's position for accurate physics-based movement.
        Vector2 currentPos = enemyTransform.position;
        Vector2 targetPos = waypoints[currentWaypoint];

        // Compute the new position towards the target.
        Vector2 newPos = Vector2.MoveTowards(currentPos, targetPos, speed * Time.fixedDeltaTime);

        // Move the enemy using the physics engine.
        enemyTransform.MovePosition(newPos);

        // Switch to the next waypoint if close enough.
        if (Vector2.Distance(newPos, targetPos) < 0.1f)
        {
            currentWaypoint = (currentWaypoint + 1) % waypoints.Length;
        }
    }
}
