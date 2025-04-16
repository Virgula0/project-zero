using System;
using System.Collections.Generic;
using UnityEngine;

// KDTree is a class which provides O(log n) average case complexity for finding 
// the nearest waypoint once a tree has been built from a given set of waypoints.
// Tree build time: O(n log n)
// Best search time: O(log n)
// Worst search: O(n)
// AVL tree is not naturally suited for multidimensional spatial queries while KDTree is.
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

    public void UpdateVectorSet(Vector2 newPoint)
    {
        // Create a new array with one extra slot.
        Vector2[] newPoints = new Vector2[points.Length + 1];
        Array.Copy(points, newPoints, points.Length);
        newPoints[points.Length] = newPoint;

        // Update internal point list.
        points = newPoints;

        // Rebuild the tree with the updated point set.
        IndexedPoint[] indexedPoints = new IndexedPoint[points.Length];
        for (int i = 0; i < points.Length; i++)
        {
            indexedPoints[i] = new IndexedPoint(points[i], i);
        }

        root = Build(indexedPoints, 0);
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
    private Vector2[] points;

    public KdTree(Vector2[] points)
    {
        this.points = points;
        // Build an array of IndexedPoint to track the original index.
        IndexedPoint[] indexedPoints = new IndexedPoint[points.Length];
        for (int i = 0; i < points.Length; i++)
        {
            indexedPoints[i] = new IndexedPoint(points[i], i);
        }
        // Build the kd‑tree using the indexed points.
        root = Build(indexedPoints, 0);
    }

    public Vector2[] GetPoints()
    {
        return points;
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

    // New FindNearest that ignores a specific index.
    public Vector2 FindNearest(Vector2 target, int indexToIgnore, out int index)
    {
        Vector2 best = Vector2.zero;
        float bestDistanceSqr = float.MaxValue;
        int bestIndex = -1;
        NearestNeighborIgnoreIndex(root, target, indexToIgnore, ref best, ref bestDistanceSqr, ref bestIndex);
        index = bestIndex;
        return best;
    }

    private void NearestNeighborIgnoreIndex(Node node, Vector2 target, int indexToIgnore, ref Vector2 best, ref float bestDistanceSqr, ref int bestIndex)
    {
        if (node == null)
        {
            return;
        }

        // Skip this node if its index matches the one we want to ignore.
        if (node.Index == indexToIgnore)
        {
            NearestNeighborIgnoreIndex(node.Left, target, indexToIgnore, ref best, ref bestDistanceSqr, ref bestIndex);
            NearestNeighborIgnoreIndex(node.Right, target, indexToIgnore, ref best, ref bestDistanceSqr, ref bestIndex);
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

        int axis = node.Axis;
        float diff = axis == 0 ? target.x - node.Point.x : target.y - node.Point.y;
        Node firstBranch = diff < 0 ? node.Left : node.Right;
        Node secondBranch = diff < 0 ? node.Right : node.Left;

        NearestNeighborIgnoreIndex(firstBranch, target, indexToIgnore, ref best, ref bestDistanceSqr, ref bestIndex);
        if (diff * diff < bestDistanceSqr)
        {
            NearestNeighborIgnoreIndex(secondBranch, target, indexToIgnore, ref best, ref bestDistanceSqr, ref bestIndex);
        }
    }

    // New FindNearest that ignores a specific point.
    public Vector2 FindNearest(Vector2 target, Vector2 pointToIgnore, out int index)
    {
        Vector2 best = Vector2.zero;
        float bestDistanceSqr = float.MaxValue;
        int bestIndex = -1;
        NearestNeighborIgnorePoint(root, target, pointToIgnore, ref best, ref bestDistanceSqr, ref bestIndex);
        index = bestIndex;
        return best;
    }

    private void NearestNeighborIgnorePoint(Node node, Vector2 target, Vector2 pointToIgnore, ref Vector2 best, ref float bestDistanceSqr, ref int bestIndex)
    {
        if (node == null)
        {
            return;
        }

        // Skip this node if its point matches the one we want to ignore.
        if (node.Point == pointToIgnore)
        {
            NearestNeighborIgnorePoint(node.Left, target, pointToIgnore, ref best, ref bestDistanceSqr, ref bestIndex);
            NearestNeighborIgnorePoint(node.Right, target, pointToIgnore, ref best, ref bestDistanceSqr, ref bestIndex);
            return;
        }

        float distanceSqr = (node.Point - target).sqrMagnitude;
        if (distanceSqr < bestDistanceSqr)
        {
            bestDistanceSqr = distanceSqr;
            best = node.Point;
            bestIndex = node.Index;
        }

        int axis = node.Axis;
        float diff = axis == 0 ? target.x - node.Point.x : target.y - node.Point.y;
        Node firstBranch = diff < 0 ? node.Left : node.Right;
        Node secondBranch = diff < 0 ? node.Right : node.Left;

        NearestNeighborIgnorePoint(firstBranch, target, pointToIgnore, ref best, ref bestDistanceSqr, ref bestIndex);
        if (diff * diff < bestDistanceSqr)
        {
            NearestNeighborIgnorePoint(secondBranch, target, pointToIgnore, ref best, ref bestDistanceSqr, ref bestIndex);
        }
    }

    // New method: FindNearestExcluding returns the nearest point to "target"
    // while ignoring any points contained in the "pointsToExclude" array.
    public Vector2 FindNearestExcluding(Vector2 target, Vector2[] pointsToExclude, out int index)
    {
        Vector2 best = Vector2.zero;
        float bestDistanceSqr = float.MaxValue;
        int bestIndex = -1;

        // Create a HashSet for quick lookup of points to ignore.
        HashSet<Vector2> excludeSet = new HashSet<Vector2>(pointsToExclude);

        NearestNeighborIgnorePoints(root, target, excludeSet, ref best, ref bestDistanceSqr, ref bestIndex);
        index = bestIndex;
        return best;
    }

    // Recursive helper that skips any node whose point is in the excludeSet.
    private void NearestNeighborIgnorePoints(Node node, Vector2 target, HashSet<Vector2> excludeSet, ref Vector2 best, ref float bestDistanceSqr, ref int bestIndex)
    {
        if (node == null)
        {
            return;
        }

        // Skip node if its point is in the exclusion set.
        if (excludeSet.Contains(node.Point))
        {
            NearestNeighborIgnorePoints(node.Left, target, excludeSet, ref best, ref bestDistanceSqr, ref bestIndex);
            NearestNeighborIgnorePoints(node.Right, target, excludeSet, ref best, ref bestDistanceSqr, ref bestIndex);
            return;
        }

        // Compute squared distance and update best if needed.
        float distanceSqr = (node.Point - target).sqrMagnitude;
        if (distanceSqr < bestDistanceSqr)
        {
            bestDistanceSqr = distanceSqr;
            best = node.Point;
            bestIndex = node.Index;
        }

        int axis = node.Axis;
        float diff = axis == 0 ? target.x - node.Point.x : target.y - node.Point.y;
        Node firstBranch = diff < 0 ? node.Left : node.Right;
        Node secondBranch = diff < 0 ? node.Right : node.Left;

        NearestNeighborIgnorePoints(firstBranch, target, excludeSet, ref best, ref bestDistanceSqr, ref bestIndex);

        // Only traverse the second branch if the hypersphere might cross the splitting plane.
        if (diff * diff < bestDistanceSqr)
        {
            NearestNeighborIgnorePoints(secondBranch, target, excludeSet, ref best, ref bestDistanceSqr, ref bestIndex);
        }
    }

    // Given a target point, this method returns an array of all points stored in the kd-tree,
    // where the 0th index is the nearest and the last index is the farthest from the target.
    public Vector2[] FindFromNearestToFarthest(Vector2 target)
    {
        // Use a list to hold each node's point and the computed squared distance.
        List<NodeDistance> distances = new List<NodeDistance>();
        CollectNodes(root, target, distances);

        // Sort the list by distance in ascending order.
        distances.Sort((a, b) => a.distanceSqr.CompareTo(b.distanceSqr));

        // Build an array with the sorted points.
        Vector2[] sortedPoints = new Vector2[distances.Count];
        for (int i = 0; i < distances.Count; i++)
        {
            sortedPoints[i] = distances[i].point;
        }
        return sortedPoints;
    }

    // Helper class to store a point along with its squared distance from the target.
    private class NodeDistance
    {
        public Vector2 point;
        public float distanceSqr;
        public int index;

        public NodeDistance(Vector2 point, float distanceSqr, int index)
        {
            this.point = point;
            this.distanceSqr = distanceSqr;
            this.index = index;
        }
    }

    // Recursive helper function that traverses the tree and adds each node's point 
    // along with its squared distance to the target into the provided list.
    private void CollectNodes(Node node, Vector2 target, List<NodeDistance> distances)
    {
        if (node == null)
        {
            return;
        }

        // Calculate the squared distance from this node's point to the target.
        float d = (node.Point - target).sqrMagnitude;
        distances.Add(new NodeDistance(node.Point, d, node.Index));

        // Continue with left and right subtrees.
        CollectNodes(node.Left, target, distances);
        CollectNodes(node.Right, target, distances);
    }
}
