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

    public void UpdateVectorSetOnInsert(Vector2 newPoint)
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

    // Removes the first occurrence of the given point from the kd‑tree’s data set,
    // rebuilds the tree, and returns true if the point was found and removed.
    // Returns false if the point was not present.
    public bool UpdateVectorSetOnDeleteFirstOccurence(Vector2 pointToRemove)
    {
        // Find index of the point to remove
        int removeIndex = Array.FindIndex(points, p => p == pointToRemove);
        if (removeIndex < 0)
        {
            // Point not found
            return false;
        }

        // Create a new array one element smaller
        Vector2[] newPoints = new Vector2[points.Length - 1];

        // Copy elements before the removed point
        if (removeIndex > 0)
            Array.Copy(points, 0, newPoints, 0, removeIndex);

        // Copy elements after the removed point
        if (removeIndex < points.Length - 1)
            Array.Copy(points, removeIndex + 1, newPoints, removeIndex, points.Length - removeIndex - 1);

        // Update the internal point list
        points = newPoints;

        // Rebuild the kd‑tree from the updated set
        IndexedPoint[] indexedPoints = new IndexedPoint[points.Length];
        for (int i = 0; i < points.Length; i++)
        {
            indexedPoints[i] = new IndexedPoint(points[i], i);
        }
        root = Build(indexedPoints, 0);

        return true;
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

    public Vector2[] GetPoints()
    {
        return this.points;
    }

    public Vector2 FindNearestRayCasting(Vector2 target, LayerMask obstacleMask, out int foundIndex)
    {
        // 1) Gather every waypoint + its squared distance to 'target'
        List<NodeDistance> all = new List<NodeDistance>();
        CollectNodes(root, target, all);

        const float EPS = 0.05f;   // small inset to avoid self-hits

        // 2) Filter to only those with an unobstructed raycast
        var visible = new List<NodeDistance>();
        foreach (var nd in all)
        {
            if (nd.distanceSqr == 0f)
                continue;  // skip if exactly on top

            Vector2 candidate = nd.point;
            Vector2 dir = (candidate - target).normalized;
            float fullDist = Mathf.Sqrt(nd.distanceSqr);

            // nudge the origin/shorten the ray so you don't immediately hit your own collider
            Vector2 origin = target + dir * EPS;
            float rayDist = Mathf.Max(0f, fullDist - 2 * EPS);

            RaycastHit2D hit = Physics2D.Raycast(
                origin,
                dir,
                rayDist,
                obstacleMask
            );

            if (hit.collider == null)
                visible.Add(nd);
        }

        // 3) If none are visible, bail out
        if (visible.Count == 0)
        {
            foundIndex = -1;
            return Vector2.zero;
        }

        // 4) Otherwise, sort the *visible* ones by distance and return the very nearest
        visible.Sort((a, b) => a.distanceSqr.CompareTo(b.distanceSqr));

        foundIndex = visible[0].index;
        return visible[0].point;
    }

}
