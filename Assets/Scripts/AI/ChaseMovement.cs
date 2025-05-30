using System;
using System.Collections;
using UnityEngine;

public class ChaseMovement : MonoBehaviour, IMovement
{
    [Tooltip("Base speed when directly chasing the player")]
    [SerializeField] private float chaseSpeed = 3f;

    [Tooltip("Additional multiplier when following a path of waypoints")]
    [SerializeField] private float additionalSpeedWhenFollowingPath = 1.5f;

    [Tooltip("Approximately how long it takes to reach target speed on the first segment")]
    private float smoothTime = 0.15f;

    private float stoppingDistance;
    private Rigidbody2D playerBody;
    private Detector playerDetector;
    private KdTree kdTree;
    private PathFinder bfs;
    private Vector2 enemyLatestPosition;
    private bool busy = false;
    private Coroutine _chaseCoroutine;
    private Func<float> getStoppingDistance;

    // Velocity tracker for the one SmoothDamp call
    private Vector2 pathVelocity = Vector2.zero;

    const float kThresholdSqr = 0.1f * 0.1f;

    public IMovement New(GameObject player, Detector detector, KdTree tree, PathFinder bfs, float chaseSpeed, Func<float> getStoppingDistance)
    {
        if (player == null || chaseSpeed < 1)
            throw new ArgumentException("Invalid argument passed to chase movement");

        this.playerDetector = detector;
        this.playerBody = player.GetComponent<Rigidbody2D>();
        this.chaseSpeed = chaseSpeed;
        this.stoppingDistance = getStoppingDistance();
        this.getStoppingDistance = getStoppingDistance;
        this.kdTree = tree;
        this.bfs = bfs;
        return this;
    }

    // Helper method: smooth movement (only used on the very first waypoint)
    private void MoveSmooth(Rigidbody2D enemyRB, Vector2 targetPosition, ref Vector2 velocityTracker, float speedMultiplier = 1f)
    {
        Vector2 newPos = Vector2.SmoothDamp(
            current: enemyRB.position,
            target: targetPosition,
            currentVelocity: ref velocityTracker,
            smoothTime: smoothTime,
            maxSpeed: chaseSpeed * speedMultiplier);
        enemyRB.MovePosition(newPos);
    }

    // Direct movement helper
    private void MoveDirect(Rigidbody2D enemyRB, Vector2 targetPosition, float speedMultiplier = 1f)
    {
        Vector2 enemyPos = enemyRB.position;
        Vector2 newPos = Vector2.MoveTowards(
            enemyPos,
            targetPosition,
            chaseSpeed * speedMultiplier * Time.fixedDeltaTime);
        enemyRB.MovePosition(newPos);
    }

    public Vector2 FindClosestWayPoint(Rigidbody2D enemyRigidbody, Vector2[] toExclude, out int index)
    {
        if (kdTree == null)
        {
            throw new InvalidOperationException("KdTree is not built. Make sure doorWayPoint array is assigned.");
        }

        return kdTree.FindNearestExcluding(enemyRigidbody.position, toExclude, out index);
    }

    private Vector2 FindCloserClearWaypoint()
    {
        return kdTree.FindNearestRayCasting(playerBody.position, playerDetector.GetObstacleLayers(), out _);
    }

    private IEnumerator MoveDoorWaypointsCoroutine(Rigidbody2D enemyRB, Vector2 startPoint)
    {
        // Determine the best waypoint on the player’s side.
        // The problem here is that it is allowed to be hidden because the player could be 
        // behind an obstacle but enemy wants to chase it anyway
        // this of course may be improved
        // the line between ----- attempts to solve this problem in a similar way as done in patrol a coward coroutines
        // ---------------------------------------------------------------------------------------------
        Vector2 playerBestWaypoint = FindCloserClearWaypoint();
        // ---------------------------------------------------------------------------------------------

        //Vector2 playerBestWaypoint = kdTree.FindNearest(playerBody.position, out _);
        Vector2[] path = bfs.PathToPoint(startPoint, playerBestWaypoint);

        if (path == null || path.Length < 1)
        {
            busy = false;
            yield break;
        }

        bool firstSegment = true;
        foreach (Vector2 waypoint in path)
        {
            while (Vector2.Distance(enemyRB.position, waypoint) > 0.1f)
            {
                // During each fixed update, check if a direct line of sight has opened up.
                float distanceToPlayer = Vector2.Distance(enemyRB.position, playerBody.position);
                RaycastHit2D hit = Physics2D.Raycast(
                    enemyRB.position,
                    (playerBody.position - enemyRB.position).normalized,
                    distanceToPlayer,
                    playerDetector.GetObstacleLayers());
                bool clearLine = hit.collider == null;

                if ((clearLine && distanceToPlayer <= stoppingDistance * 1.5f)
                    || !playerDetector.GetIsEnemyAwareOfPlayer())
                {
                    // Player is now directly approachable or movement changed
                    busy = false;
                    yield break;
                }

                if (firstSegment)
                {
                    MoveSmooth(enemyRB, waypoint, ref pathVelocity, additionalSpeedWhenFollowingPath);
                }
                else
                {
                    MoveDirect(enemyRB, waypoint, additionalSpeedWhenFollowingPath);
                }

                yield return new WaitForFixedUpdate();
            }

            // after we finish the first waypoint, switch off smoothing
            if (firstSegment)
            {
                firstSegment = false;
                pathVelocity = Vector2.zero;
            }
        }

        busy = false;
        Debug.Log("Enemy finished moving using waypoints");
    }

    public Vector2 FindClosestWayPoint(Vector2 enemyPos)
    {
        if (kdTree == null)
        {
            throw new InvalidOperationException("KdTree is not built. Make sure doorWayPoint array is assigned.");
        }

        Vector2 enemyWaypoint = kdTree.FindNearestRayCasting(enemyPos, playerDetector.GetObstacleLayers(), out _);
        return enemyWaypoint;
    }

    public void Move(Rigidbody2D enemyRB)
    {
        if (busy)
            return;

        this.stoppingDistance = getStoppingDistance();
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
            (enemyPos - enemyLatestPosition).sqrMagnitude < kThresholdSqr) // if the latest position is too small we may want to find an exit through a waypoint
        {
            Vector2? bestWaypoint = FindClosestWayPoint(enemyPos);
            if (bestWaypoint == null)
                return;

            Debug.Log("Using DoorWayPoint to find an exit");
            busy = true;
            _chaseCoroutine = StartCoroutine(MoveDoorWaypointsCoroutine(enemyRB, bestWaypoint.Value));
            return;
        }

        enemyLatestPosition = enemyPos;

        // Normal chasing when no waypoint and no historic player positions are available
        // Debug.Log("Chasing the player normally");
        MoveDirect(enemyRB, playerBody.position);
    }

    public void NeedsRepositioning(bool reposition)
    {
        return;
    }

    public void StopCoroutines(bool stop)
    {
        if (!stop || _chaseCoroutine == null)
        {
            return;
        }
        busy = false;
        StopCoroutine(_chaseCoroutine);
        _chaseCoroutine = null;
    }
}
