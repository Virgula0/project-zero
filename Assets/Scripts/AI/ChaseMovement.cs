using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class ChaseMovement : IMovement
{
    private float chaseSpeed = 0f;
    // offsetDistanceFromPlayer is used for letting an enemy to stop nearly or more far away from the player
    // this is useful for distinguish between melee or ranged enemies
    private float inverseOffsetDistanceFromPlayer = 0f;
    private Rigidbody2D playerBody; // used for getting the current coordinates of the player
    private float maxDistanceFromPlayer = 0f; // this will be padded adding offsetDistanceFromPlayer
    
    // door waypoints is a vector containing the coordinates of doors or obstacles (manually defined in the editor) 
    // in order to surpass them when chasing the player
    // if the player is on the other side of one of doorPoints within the vector we use that waypoint which indicates the exit 
    // from the room
    // We have the playerObject reference so we know its position easly.
    private Vector2[] doorWaypoints;

    public ChaseMovement(GameObject player, float chaseSpeed, float offsetDistanceFromPlayer, Vector2[] doorWaypoints)
    {
        if (player == null || chaseSpeed < 1)
        {
            throw new ArgumentException("invalid argument passed to chase movement");
        }
        this.playerBody = player.GetComponent<Rigidbody2D>();
        this.chaseSpeed = chaseSpeed;
        this.inverseOffsetDistanceFromPlayer = offsetDistanceFromPlayer;
        this.doorWaypoints = doorWaypoints;
    }


    // returns the closest exit waypoint, it may be empty
    private Vector2 IterateWaypoints(Rigidbody2D enemyPos)
    {
        float minDistance = float.PositiveInfinity;

        Vector2 min = new(float.PositiveInfinity, float.PositiveInfinity);
        foreach (Vector2 point in doorWaypoints)
        {
            float distance = Vector2.Distance(enemyPos.position.normalized, point.normalized);
            if (distance < minDistance)
            {
                min = point;
            }
        }
        return min;
    }

    public void Move(Rigidbody2D enemyTransform)
    {
        Vector2 nearestWayPoint = IterateWaypoints(enemyTransform);
        // calculate distance from player
        float distanceToPlayer = Vector2.Distance(enemyTransform.position, playerBody.position);
        // if we're far away from the player enemy will try to get closer
        if (distanceToPlayer > maxDistanceFromPlayer - inverseOffsetDistanceFromPlayer) // offsetDistanceFromPlayer will be decreased
        {
            Debug.Log("Enemy is getting closer to the player");
            // Move towards the current waypoint.
            Vector2 newPos = Vector2.MoveTowards(enemyTransform.position, playerBody.position, chaseSpeed * Time.fixedDeltaTime);
            // Update enemy's position while keeping the original z-coordinate.
            enemyTransform.MovePosition(newPos);

            // Switch to the next waypoint if any and if closer than player position
            if (nearestWayPoint.x != float.PositiveInfinity)
            {
                Debug.Log("Using door waypoint to let the enemy to find the exit");
                Vector2 newWayPos = Vector2.MoveTowards(enemyTransform.position, nearestWayPoint, chaseSpeed * Time.fixedDeltaTime);
                enemyTransform.MovePosition(newWayPos);
            }
        }
    }
}