using System;
using UnityEngine;

public class ChaseMovement : IMovement
{
    private float chaseSpeed;
    private float stoppingDistance = 5f;
    private Rigidbody2D playerBody;
    // door waypoints is a vector containing the coordinates of doors or obstacles (manually defined in the editor) 
    // in order to surpass them when chasing the player
    // if the player is on the other side of one of doorPoints within the vector we use that waypoint which indicates the exit 
    // from the room
    // We have the playerObject reference so we know its position easly.
    private Vector2[] doorWaypoints;

    public ChaseMovement(GameObject player, float chaseSpeed, float stoppingDistance, Vector2[] doorWaypoints)
    {
        if (player == null || chaseSpeed < 1)
            throw new ArgumentException("Invalid argument passed to chase movement");

        this.playerBody = player.GetComponent<Rigidbody2D>();
        this.chaseSpeed = chaseSpeed;
        this.stoppingDistance = stoppingDistance;
        this.doorWaypoints = doorWaypoints ?? Array.Empty<Vector2>();
    }

    private Vector2? FindBestWaypoint(Vector2 enemyPosition)
    {
        Vector2 playerPos = playerBody.position;
        Vector2? bestWaypoint = null;
        float minDistance = float.MaxValue;

        foreach (Vector2 waypoint in doorWaypoints)
        {
            Vector2 toWaypoint = waypoint - enemyPosition;
            Vector2 waypointToPlayer = playerPos - waypoint;

            // Check if player is beyond the waypoint from enemy's perspective
            if (Vector2.Dot(toWaypoint, waypointToPlayer) > 0)
            {
                float distance = Vector2.Distance(enemyPosition, waypoint);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    bestWaypoint = waypoint;
                }
            }
        }
        return bestWaypoint;
    }

    public void Move(Rigidbody2D enemyRB)
    {
        Vector2 enemyPos = enemyRB.position;
        Vector2? bestWaypoint = FindBestWaypoint(enemyPos);

        Vector2 targetPosition;
        bool useWaypoint = bestWaypoint.HasValue;

        if (useWaypoint)
        {
            Debug.Log("Using DoorWayPoint to districate");
            targetPosition = bestWaypoint.Value;
        }
        else
        {
            Debug.Log("Chasing the player");
            float distanceToPlayer = Vector2.Distance(enemyPos, playerBody.position);
            if (distanceToPlayer <= stoppingDistance)
            {
                enemyRB.linearVelocity = Vector2.zero;
                return;
            }
            targetPosition = playerBody.position;
        }

        Vector2 newPos = Vector2.MoveTowards(enemyPos, targetPosition, chaseSpeed * Time.fixedDeltaTime);
        enemyRB.MovePosition(newPos);
    }
}