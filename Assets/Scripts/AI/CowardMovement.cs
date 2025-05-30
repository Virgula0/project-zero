using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CowardMovement : MonoBehaviour, IMovement
{
    private PathFinder bfs;
    private Vector2[] patrolWaypoints;
    private KdTree kdTree;
    private float speed;
    private bool busy;
    private Coroutine _cowardRoutine;
    private Detector playerDetector;
    private Vector2 basePoint;

    public IMovement New(Vector2 basePoint, Vector2[] patrolWaypoints, KdTree treeStructure, PathFinder bfs, Detector playerDetector, float speed)
    {
        this.basePoint = basePoint;
        this.patrolWaypoints = patrolWaypoints;
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

    private Vector2 CalculateClosestPoint(Rigidbody2D rb)
    {
        // this method can be overwritten in future using kdtree.FindNearestRayCasting which should do the same
        return kdTree.FindNearestRayCasting(rb.position, playerDetector.GetObstacleLayers(), out _);
    }

    private IEnumerator MoveInCircleRoutine(Rigidbody2D rb)
    {
        busy = true;
        Vector2 closestPoint = CalculateClosestPoint(rb);
        Vector2[] path = bfs.PathToPoint(closestPoint, basePoint);

        // Step 1: move to the nearest point first

        foreach (Vector2 v in path.Take(path.Length))
        {
            yield return MoveToWithChecks(rb, v);
        }

        // Step 2: build the circular path (elements after the start index, then wrap)
        List<Vector2> circularPath = GetCircularTraversal(patrolWaypoints, 0);

        // Step 3: loop forever (or until stopped), visiting each point in order
        while (busy)
        {
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
        for (int i = startIndex; i < count; i++)
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
