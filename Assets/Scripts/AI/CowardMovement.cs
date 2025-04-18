using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CowardMovement : MonoBehaviour, IMovement
{
    private BFSPathfinder bfs;
    private Vector2[] waypoints;
    private KdTree kdTree;
    private float speed;
    private bool busy;
    private bool stopCowardMovement;
    private Coroutine _cowardRoutine;
    private Detector playerDetector;

    public IMovement New(Vector2[] waypoints, Vector2[] globalWaypoints, KdTree treeStructure, BFSPathfinder bfs, Detector playerDetector, float speed)
    {
        this.waypoints = Utils.Functions.RemoveAll(waypoints, globalWaypoints); // remove global waypoints from waypoints
        // this.waypoints = Utils.Functions.RemoveAtIndex(this.waypoints, 0); // remove element 0 because we don't want to enter the room anymore
        this.kdTree = treeStructure;
        this.speed = speed;
        this.bfs = bfs;
        this.playerDetector = playerDetector;
        return this;
    }

    public void Move(Rigidbody2D enemyRB)
    {
        if (busy || stopCowardMovement) return;
        busy = true;
        _cowardRoutine = StartCoroutine(MoveInCircleRoutine(enemyRB));
    }

    public void NeedsRepositioning(bool stop)
    {
        stopCowardMovement = stop;

        if (stopCowardMovement && _cowardRoutine != null)
        {
            StopCoroutine(_cowardRoutine);
            _cowardRoutine = null;
            busy = false;
            stopCowardMovement = false;
        }
    }

    public Vector2 FindClosestWayPoint(Rigidbody2D enemyRigidbody, Vector2[] toExclude, out int index)
    {
        if (kdTree == null)
        {
            throw new InvalidOperationException("KdTree is not built. Make sure doorWayPoint array is assigned.");
        }

        return kdTree.FindNearestExcluding(enemyRigidbody.position, toExclude, out index);
    }

    private IEnumerator MoveInCircleRoutine(Rigidbody2D rb)
    {
        bool clearPath = false;
        Vector2 closestPoint = new();
        List<Vector2> vectorsToExclude = new List<Vector2>();
        while (!clearPath)
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

        Vector2[] path = bfs.PathToTheFirst(closestPoint);

        // Step 1: move to the nearest point first
        foreach (Vector2 v in path)
        {
            yield return MoveToWithChecks(rb, v);
        }

        // Step 2: build the circular path (elements after the start index, then wrap)
        List<Vector2> circularPath = GetCircularTraversal(0); 

        // Step 3: loop forever (or until stopped), visiting each point in order
        while (true)
        {
            foreach (var point in circularPath)
            {
                yield return MoveToWithChecks(rb, point);
                if (stopCowardMovement)
                    yield break;
            }
        }
    }

    private List<Vector2> GetCircularTraversal(int startIndex)
    {
        int count = waypoints.Length;
        List<Vector2> circularPath = new List<Vector2>();

        // Add the waypoints starting from startIndex + 1 to the end of the list
        for (int i = startIndex + 1; i < count; i++)
        {
            circularPath.Add(waypoints[i]);
        }

        // Wrap around and add waypoints from the beginning to startIndex
        for (int i = 0; i <= startIndex; i++)
        {
            circularPath.Add(waypoints[i]);
        }

        return circularPath;
    }

    private IEnumerator MoveToWithChecks(Rigidbody2D rb, Vector2 destination)
    {
        while (Vector2.Distance(rb.position, destination) > 0.1f)
        {
            if (stopCowardMovement)
            {
                busy = false;
                yield break;
            }

            MoveTowards(rb, destination);
            yield return new WaitForFixedUpdate();
        }
    }

    private void MoveTowards(Rigidbody2D rb, Vector2 targetPosition, float speedMultiplier = 1f)
    {
        Vector2 newPos = Vector2.MoveTowards(rb.position,targetPosition, speed * speedMultiplier * Time.fixedDeltaTime);
        rb.MovePosition(newPos);
    }
}
