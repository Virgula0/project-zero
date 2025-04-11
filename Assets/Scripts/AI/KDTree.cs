using System;
using UnityEngine;

// KDTree is a class which provides O(log n) average case complexity for finding 
// the nearest waypoint once a tree has been built from a given set of waypoints.
// Tree build time: O(n log n)
// Best search time: O(log n)
// Worst search: O(n)
public class KdTree
{
    // Internal helper to hold a point and its original index.
    private class IndexedPoint
    {
        public Vector2 Point;
        public int Index;

        public IndexedPoint(Vector2 point, int index)
        {
            Point = point;
            Index = index;
        }
    }

    // Internal tree node definition.
    private class Node
    {
        public Vector2 Point;  // The 2D point stored in this node.
        public int Index;      // The original index in the input array.
        public Node Left;      // Left subtree.
        public Node Right;     // Right subtree.
        public int Axis;       // Splitting axis (0 for x, 1 for y).

        public Node(Vector2 point, int index, int axis)
        {
            Point = point;
            Index = index;
            Axis = axis;
            Left = null;
            Right = null;
        }
    }

    private Node root;
    public KdTree(Vector2[] points)
    {
        // Build an array of IndexedPoint to track the original index.
        IndexedPoint[] indexedPoints = new IndexedPoint[points.Length];
        for (int i = 0; i < points.Length; i++)
        {
            indexedPoints[i] = new IndexedPoint(points[i], i);
        }
        // Build the kd‑tree using the indexed points.
        root = Build(indexedPoints, 0);
    }

    private Node Build(IndexedPoint[] points, int depth)
    {
        if (points == null || points.Length == 0)
        {
            return null;
        }

        // Determine the splitting axis: 0 for x, 1 for y.
        int axis = depth % 2;

        // Sort points based on the current axis.
        Array.Sort(points, (a, b) =>
        {
            return axis == 0 ? a.Point.x.CompareTo(b.Point.x) : a.Point.y.CompareTo(b.Point.y);
        });

        // Choose the median point as the root for this subtree.
        int medianIndex = points.Length / 2;
        Node node = new Node(points[medianIndex].Point, points[medianIndex].Index, axis);

        // Build left subtree from points before the median.
        IndexedPoint[] leftPoints = new IndexedPoint[medianIndex];
        Array.Copy(points, 0, leftPoints, 0, medianIndex);

        // Build right subtree from points after the median.
        int rightLength = points.Length - medianIndex - 1;
        IndexedPoint[] rightPoints = new IndexedPoint[rightLength];
        Array.Copy(points, medianIndex + 1, rightPoints, 0, rightLength);

        node.Left = Build(leftPoints, depth + 1);
        node.Right = Build(rightPoints, depth + 1);

        return node;
    }

    public Vector2 FindNearest(Vector2 target, out int index)
    {
        Vector2 best = Vector2.zero;
        float bestDistanceSqr = float.MaxValue;
        int bestIndex = -1;
        NearestNeighbor(root, target, ref best, ref bestDistanceSqr, ref bestIndex);
        index = bestIndex;
        return best;
    }

    private void NearestNeighbor(Node node, Vector2 target, ref Vector2 best, ref float bestDistanceSqr, ref int bestIndex)
    {
        if (node == null)
        {
            return;
        }

        // Compute squared distance from this node's point to the target.
        float distanceSqr = (node.Point - target).sqrMagnitude;
        if (distanceSqr < bestDistanceSqr)
        {
            bestDistanceSqr = distanceSqr;
            best = node.Point;
            bestIndex = node.Index;
        }

        // Determine which side to explore first.
        int axis = node.Axis;
        float diff = axis == 0 ? target.x - node.Point.x : target.y - node.Point.y;

        // Choose the branch that is likely to contain the target.
        Node firstBranch = diff < 0 ? node.Left : node.Right;
        Node secondBranch = diff < 0 ? node.Right : node.Left;

        // Explore the first branch.
        NearestNeighbor(firstBranch, target, ref best, ref bestDistanceSqr, ref bestIndex);

        // Check if the hypersphere crosses the splitting plane.
        if (diff * diff < bestDistanceSqr)
        {
            NearestNeighbor(secondBranch, target, ref best, ref bestDistanceSqr, ref bestIndex);
        }
    }
}
