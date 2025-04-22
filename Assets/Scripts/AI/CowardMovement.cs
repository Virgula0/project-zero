using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CowardMovement : MonoBehaviour, IMovement
{
    private BFSPathfinder bfs;
    private Vector2[] waypoints;
    private Vector2[] patrolWaypoints;
    private KdTree kdTree;
    private float speed;
    private bool busy;
    private Coroutine _cowardRoutine;
    private Detector playerDetector;
    private Vector2[] originalWaypoints;

    public IMovement New(Vector2[] waypoints, Vector2[] globalWaypoints, Vector2[] patrolWaypoints, KdTree treeStructure, BFSPathfinder bfs, Detector playerDetector, float speed)
    {
        this.patrolWaypoints = patrolWaypoints;
        this.waypoints = Utils.Functions.RemoveAll(waypoints, globalWaypoints); // remove global waypoints from waypoints
        this.waypoints = Utils.Functions.RemoveAtIndex(this.waypoints, 0); // remove element 0 because we don't want to enter the room anymore
        this.originalWaypoints = waypoints;
        this.kdTree = treeStructure;
        this.speed = speed;
        this.bfs = bfs;
        this.playerDetector = playerDetector;
        return this;
    }

    public void Move(Rigidbody2D enemyRB)
    {
        if (busy) return;
        _cowardRoutine = StartCoroutine(MoveInCircleRoutine(enemyRB));
    }

    public Vector2 FindClosestWayPoint(Rigidbody2D enemyRigidbody, Vector2[] toExclude, out int index)
    {
        if (kdTree == null)
        {
            throw new InvalidOperationException("KdTree is not built. Make sure doorWayPoint array is assigned.");
        }

        return kdTree.FindNearestExcluding(enemyRigidbody.position, toExclude, out index);
    }

    private Vector2 CalculateClosesPoint(Rigidbody2D rb)
    {
        busy = true;
        bool clearPath = false;
        Vector2 closestPoint = new();
        List<Vector2> vectorsToExclude = new List<Vector2>();
        while (busy && !clearPath)
        {
            closestPoint = FindClosestWayPoint(rb, vectorsToExclude.ToArray(), out _);
            Vector2 directionToClosest = (closestPoint - rb.position).normalized;
            float distanceToClosest = Vector2.Distance(rb.position, closestPoint);
            RaycastHit2D hit = Physics2D.Raycast(rb.position, directionToClosest, distanceToClosest, playerDetector.GetObstacleLayers());
            clearPath = hit.collider == null;

            if (!clearPath)
            {
                vectorsToExclude.Add(closestPoint);
                Debug.Log("Obstacle detected between enemy and closest waypoint while trying to coward. Recalculating.");
            }
        }
        return closestPoint;
    }

    private IEnumerator MoveInCircleRoutine(Rigidbody2D rb)
    {
        Vector2 closestPoint = CalculateClosesPoint(rb);
        Vector2[] path = bfs.PathToTheFirst(closestPoint);

        // Step 1: move to the nearest point first

        foreach (Vector2 v in path.Take(path.Length - 1))
        {
            yield return MoveToWithChecks(rb, v);
        }

        List<Vector2> circularPath;
        // Step 2: build the circular path (elements after the start index, then wrap)

        // Step 3: loop forever (or until stopped), visiting each point in order
        while (busy)
        {
            closestPoint = CalculateClosesPoint(rb);
            circularPath = closestPoint == originalWaypoints[0] ? GetCircularTraversal(patrolWaypoints, 0)
                : GetCircularTraversal(waypoints, 0);
            foreach (var point in circularPath)
            {
                yield return MoveToWithChecks(rb, point);
            }
        }
    }

    private List<Vector2> GetCircularTraversal(Vector2[] points, int startIndex)
    {
        int count = points.Length;
        List<Vector2> circularPath = new List<Vector2>();

        // Add the waypoints starting from startIndex + 1 to the end of the list
        for (int i = startIndex + 1; i < count; i++)
        {
            circularPath.Add(points[i]);
        }

        // Wrap around and add waypoints from the beginning to startIndex
        for (int i = 0; i <= startIndex; i++)
        {
            circularPath.Add(points[i]);
        }

        return circularPath;
    }

    private IEnumerator MoveToWithChecks(Rigidbody2D rb, Vector2 destination)
    {
        while (busy && Vector2.Distance(rb.position, destination) > 0.1f)
        {
            MoveTowards(rb, destination);
            yield return new WaitForFixedUpdate();
        }
    }

    private void MoveTowards(Rigidbody2D rb, Vector2 targetPosition, float speedMultiplier = 1f)
    {
        Vector2 newPos = Vector2.MoveTowards(rb.position, targetPosition, speed * speedMultiplier * Time.fixedDeltaTime);
        rb.MovePosition(newPos);
    }

    public void StopCoroutines(bool stop)
    {
        if (!stop || _cowardRoutine == null)
        {
            return;
        }
        busy = false;
        StopCoroutine(_cowardRoutine);
        _cowardRoutine = null;
    }

    public void NeedsRepositioning(bool reposition)
    {
        return;
    }
}
