using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaseMovement : MonoBehaviour, IMovement
{
    private float chaseSpeed;
    private float stoppingDistance = 5f;
    private Rigidbody2D playerBody;
    private Detector playerDetector;
    private KdTree kdTree;
    private BFSPathfinder bfs;
    private Vector2 enemyLatestPosition;
    private bool busy = false;
    private float additionalSpeedWhenFollowingPath = 1.5f;

    public IMovement New(GameObject player, Detector detector, KdTree tree, BFSPathfinder bfs, float chaseSpeed, float stoppingDistance)
    {
        if (player == null || chaseSpeed < 1)
            throw new ArgumentException("Invalid argument passed to chase movement");

        this.playerDetector = detector;
        this.playerBody = player.GetComponent<Rigidbody2D>();
        this.chaseSpeed = chaseSpeed;
        this.stoppingDistance = stoppingDistance;
        this.kdTree = tree;
        this.bfs = bfs;
        return this;
    }

    // Helper method to move enemy towards a target position
    private void MoveTowardsTarget(Rigidbody2D enemyRB, Vector2 targetPosition, float additionalSpeed = 1)
    {
        Vector2 enemyPos = enemyRB.position;
        Vector2 newPos = Vector2.MoveTowards(enemyPos, targetPosition, chaseSpeed * additionalSpeed * Time.fixedDeltaTime);
        enemyRB.MovePosition(newPos);
    }

    private IEnumerator MoveDoorWaypointsCoroutine(Rigidbody2D enemyTransform, Vector2 startPoint)
    {
        // TODO: this FindNearest needs improvement
        // in fact FindNearest is ok but not if there are walls in the between
        // find nearest among points not behind walls or objects
        // until this imprvement the best way is to comment out the // This is checks if the player is closer enough. strategy
        Vector2 playerBestWaypoint = kdTree.FindNearest(playerBody.position, out _);
        Vector2[] path = bfs.PathToPoint(startPoint, playerBestWaypoint);
        foreach (Vector2 v in path)
        {
            while (Vector2.Distance(enemyTransform.position, v) > 0.1f)
            {
                // This is checks if the player is closer enough. If yes we can ignore to follow the path and resume the chasing normally
                // One strategy can be: if enemy is closer to the player respect to its current waypoint and player is not behind a wall we break
                // this needs more tests
                /*
                if (Vector2.Distance(enemyTransform.position, playerBody.position) <= Vector2.Distance(enemyTransform.position, v)
                    && !playerDetector.GetIsPlayerHiddenByObstacle())
                {
                    busy = false;
                    yield return null;
                }
                */

                if (!playerDetector.GetIsEnemyAwareOfPlayer() && playerDetector.GetIsPlayerHiddenByObstacle()){
                    busy = false;
                    yield return null;
                }
                
                MoveTowardsTarget(enemyTransform, v, additionalSpeedWhenFollowingPath);
                yield return new WaitForFixedUpdate();
            }
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

        return enemyWaypoint;
    }

    public void Move(Rigidbody2D enemyRB)
    {
        if (busy)
        {
            return;
        }

        Vector2 enemyPos = enemyRB.position;
        float distanceToPlayer = Vector2.Distance(enemyPos, playerBody.position);

        // if we're too close to the player we do not move, but checking that he is not hidden, we can return
        if (distanceToPlayer <= stoppingDistance && !playerDetector.GetIsPlayerHiddenByObstacle())
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
    
    public string ToString(){
        return "chasing";
    }
    public void CustomSetter<T>(T var)
    {
        return;
    }
}
