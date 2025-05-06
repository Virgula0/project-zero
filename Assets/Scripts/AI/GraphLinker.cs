using System;
using System.Collections.Generic;
using UnityEngine;

// Finds the pair of Vector2 (one from each array) with the minimum squared distance.
public class GraphLinker
{
    /// <summary>
    /// Finds the closest pair (one point from A, one from B) whose connecting segment
    /// is completely clear of colliders in the given obstacleMask.
    /// </summary>
    /// <returns>
    /// True if at least one unblocked pair was found (and bestFromA/B set);
    /// false if every pair was blocked, in which case bestFromA/B remain Vector2.zero.
    /// </returns>
    private static bool FindClosestVisiblePair(
        Vector2[] arrayA,
        Vector2[] arrayB,
        LayerMask obstacleMask,
        out Vector2 bestFromA,
        out Vector2 bestFromB)
    {
        float minSqrDist = float.MaxValue;
        bestFromA = Vector2.zero;
        bestFromB = Vector2.zero;
        bool found = false;

        for (int i = 0; i < arrayA.Length; i++)
        {
            Vector2 a = arrayA[i];
            for (int j = 0; j < arrayB.Length; j++)
            {
                Vector2 b = arrayB[j];
                // 1) Quick distance check
                float d2 = (a - b).sqrMagnitude;
                if (d2 >= minSqrDist)
                    continue;

                // 2) Line‐of‐sight check: Physics2D.Linecast returns true if something is hit
                if (Physics2D.Linecast(a, b, obstacleMask))
                    continue;

                // This pair is both closer and unobstructed
                minSqrDist = d2;
                bestFromA = a;
                bestFromB = b;
                found = true;
            }
        }

        return found;
    }

    /// <summary>
    /// Merges two adjacency‐list graphs and links them by adding one edge between
    /// their closest mutually visible nodes.
    /// </summary>
    /// <param name="graph1">First graph (indices match points1).</param>
    /// <param name="graph2">Second graph (indices match points2).</param>
    /// <param name="points1">Positions of graph1’s nodes.</param>
    /// <param name="points2">Positions of graph2’s nodes.</param>
    /// <param name="obstacleMask">LayerMask indicating which layers count as obstacles.</param>
    /// <returns>The merged graph (with graph2’s indices offset, plus one new connection).</returns>
    public Dictionary<int, List<int>> LinkGraphs(
        Dictionary<int, List<int>> graph1,
        Dictionary<int, List<int>> graph2,
        Vector2[] points1,
        Vector2[] points2,
        LayerMask obstacleMask)
    {
        // 1) Copy graph1
        var merged = new Dictionary<int, List<int>>(graph1.Count);
        foreach (var kv in graph1)
            merged[kv.Key] = new List<int>(kv.Value);

        // 2) Copy graph2 with re-indexed keys & neighbors
        int offset = graph1.Count;
        foreach (var kv in graph2)
        {
            int key2 = kv.Key + offset;
            var neigh = new List<int>();
            foreach (int n in kv.Value)
                neigh.Add(n + offset);
            merged[key2] = neigh;
        }

        // 3) Find closest *visible* pair
        Vector2 aPos, bPos;
        bool ok = FindClosestVisiblePair(points1, points2, obstacleMask, out aPos, out bPos);

        if (!ok)
        {
            Debug.LogWarning("GraphLinker: no unobstructed link found—sub‐graphs remain disconnected.");
            return merged;
        }

        // 4) Get their original indices
        int idx1 = Array.IndexOf(points1, aPos);
        int idx2 = Array.IndexOf(points2, bPos) + offset;

        // 5) Add the bidirectional edge
        if (!merged[idx1].Contains(idx2))
            merged[idx1].Add(idx2);

        if (!merged[idx2].Contains(idx1))
            merged[idx2].Add(idx1);

        return merged;
    }

    // This function defines connections between Vector2 points. 
    /// The connection pattern is:
    /// - Index 0: connected to index 1.
    /// - Index 1: connected to indices 0, 2, and the last index (n - 1).
    /// - For other elements (indexes 2 to n - 2): connected to the previous and next indexes.
    /// - The last element (index n - 1): connected to the previous index (n - 2) and index 1.
    // indexes correspond to indexes of Vector2[] instances
    public Dictionary<int, List<int>> GenerateConnections(Vector2[] points)
    {
        Dictionary<int, List<int>> connections = new Dictionary<int, List<int>>();
        int n = points.Length;

        if (n == 0)
        {
            return connections;
        }
        else if (n == 1)
        {
            // For a single element, we could return an empty connections list.
            connections.Add(0, new List<int>());
            return connections;
        }

        for (int i = 0; i < n; i++)
        {
            List<int> neighbors = new List<int>();

            if (i == 0)
            {
                // Element 0 is connected to element 1.
                neighbors.Add(1);
            }
            else if (i == 1)
            {
                // Element 1 is connected to 0, (if exists, 2), and the last index.
                neighbors.Add(0);
                if (n > 2)
                {
                    neighbors.Add(2);
                }
                // Always add the last index for element 1.
                neighbors.Add(n - 1);
            }
            else if (i == n - 1)
            {
                // Last element is connected to its predecessor and index 1.
                neighbors.Add(n - 2);
                // Avoid duplicate if n - 1 is 1.
                if (1 != n - 2)
                {
                    neighbors.Add(1);
                }
            }
            else
            {
                // All other elements (indexes 2 to n - 2) are connected to the previous and next elements.
                neighbors.Add(i - 1);
                neighbors.Add(i + 1);
            }

            connections.Add(i, neighbors);
        }

        return connections;
    }
}
