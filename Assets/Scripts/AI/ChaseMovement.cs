using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaseMovement : MonoBehaviour, IMovement
{
    private float chaseSpeed;
    private float stoppingDistance = 5f;
    private Rigidbody2D playerBody;
    // door waypoints is a vector containing the coordinates of doors or obstacles (manually defined in the editor) 
    // in order to surpass them when chasing the player
    // if the player is on the other side of one of doorPoints within the vector we use that waypoint which indicates the exit 
    // from the room
    // We have the playerObject reference so we know its position easly.
    private PlayerDetector playerDetector;
    private KdTree kdTree;
    private bool busy = false;

    public IMovement New(GameObject player, PlayerDetector detector, float chaseSpeed, float stoppingDistance, Vector2[] doorWayPoints)
    {
        if (player == null || chaseSpeed < 1)
            throw new ArgumentException("Invalid argument passed to chase movement");

        this.playerDetector = detector;
        this.playerBody = player.GetComponent<Rigidbody2D>();
        this.chaseSpeed = chaseSpeed;
        this.stoppingDistance = stoppingDistance;
        kdTree = new KdTree(doorWayPoints);
        return this;
    }

    // Helper method to move enemy towards a target position
    private void MoveTowardsTarget(Rigidbody2D enemyRB, Vector2 targetPosition)
    {
        Vector2 enemyPos = enemyRB.position;
        Vector2 newPos = Vector2.MoveTowards(enemyPos, targetPosition, chaseSpeed * Time.fixedDeltaTime);
        enemyRB.MovePosition(newPos);
    }

    private IEnumerator MoveDoorWaypointsCoroutine(Rigidbody2D enemyTransform, Vector2 pointToReach)
    {
        while (Vector2.Distance(enemyTransform.position, pointToReach) > 0.1f)
        {
            MoveTowardsTarget(enemyTransform, pointToReach);
            yield return new WaitForFixedUpdate();
        }
        busy = false;
        Debug.Log("Enemy finished to move to the gadget");
    }

    public Vector2 FindClosestWayPoint(Vector2 enemyPos)
    {
        if (kdTree == null)
        {
            throw new InvalidOperationException("KdTree is not built. Make sure doorWayPoint array is assigned.");
        }

        Vector2 enemyWaypoint = kdTree.FindNearest(enemyPos, out _);
        //Vector2 playerWaypoint = kdTree.FindNearest(playerBody.position, out _);

        // if enemy is closer to its waypoint return the enemy waypoint otherwise the player one
        //return Vector2.Distance(enemyPos, enemyWaypoint) < Vector2.Distance(enemyPos, playerWaypoint) ? enemyWaypoint  : playerWaypoint;

        return enemyWaypoint;
    }

    private Vector2 enemyLatestPosition;

    public void Move(Rigidbody2D enemyRB)
    {
        if (busy)
        {
            return;
        }

        Vector2 enemyPos = enemyRB.position;
        float distanceToPlayer = Vector2.Distance(enemyPos, playerBody.position);
        
        // if we're too close to the player we do not move, we can return
        if (distanceToPlayer <= stoppingDistance)
        {
            enemyRB.linearVelocity = Vector2.zero;
            return;
        }

        // Retrieve player detected positions only once
        // Use door waypoint if available (helpful when enemy is stuck on a wall)
        if (playerDetector.GetIsPlayerHiddenByObstacle() || 
            Vector2.Distance(enemyPos, enemyLatestPosition) < 0.1f) // if the latest position is too small we may want to find an exit through a waypoint
        {
            Vector2? bestWaypoint = FindClosestWayPoint(enemyPos);
            Debug.Log("Using DoorWayPoint to find an exit");
            busy = true;
            StartCoroutine(MoveDoorWaypointsCoroutine(enemyRB, bestWaypoint.Value));
            return;
        }

        enemyLatestPosition = enemyPos;

        // Normal chasing when no waypoint and no historic player positions are available
        Debug.Log("Chasing the player normally");
        MoveTowardsTarget(enemyRB, playerBody.position);
    }

    public void CustomSetter<T>(T var)
    {
        return;
    }
}
