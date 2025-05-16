using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

// Finds the pair of Vector2 (one from each array) with the minimum squared distance.
public class GraphLinker
{
    /// <summary>
    /// Defines connections between Vector2 points with the following pattern:
    /// - Index 0: connected to index 1.
    /// - Index 1: connected to indices 0, 2 (if exists).
    /// - For other elements (indexes 2 to n - 2): connected to the previous and next indexes.
    /// - The last element (index n - 1): connected to the previous index (n - 2) and index 0.
    /// </summary>
    /// <param name="points">Array of 2D points.</param>
    /// <returns>
    /// A dictionary mapping each index to a list of its connected neighbor indices.
    /// </returns>
    public Dictionary<int, List<int>> GenerateCircularConnectionGraph(Vector2[] points)
    {
        var connections = new Dictionary<int, List<int>>();
        int n = points.Length;

        if (n == 0)
        {
            return connections;
        }
        if (n == 1)
        {
            // Single element: no neighbors.
            connections.Add(0, new List<int>());
            return connections;
        }

        for (int i = 0; i < n; i++)
        {
            var neighbors = new List<int>();

            if (i == 0)
            {
                // Element 0 is connected to element 1.
                neighbors.Add(1);
            }
            else if (i == 1)
            {
                // Element 1 is connected to 0, (if exists) 2.
                neighbors.Add(0);
                if (n > 2)
                    neighbors.Add(2);
            }
            else if (i == n - 1)
            {
                // Last element: connected to its predecessor and to index 0.
                neighbors.Add(n - 2);
                // Avoid duplicate when n == 2 (then n-2 == 0).
                if (n > 2)
                    neighbors.Add(0);
            }
            else
            {
                // Middle elements: connected to previous and next.
                neighbors.Add(i - 1);
                neighbors.Add(i + 1);
            }

            connections.Add(i, neighbors);
        }

        return connections;
    }

    /// <summary>
    /// Finds every pair (one point from A, one from B) whose connecting segment
    /// is completely clear of colliders in the given obstacleMask.
    /// </summary>
    /// <returns>
    /// A list of index‑pairs (i,j) where A[i] can see B[j].
    /// </returns>
    private static List<(int aIndex, int bIndex)> FindAllVisiblePairs(
        Vector2[] arrayA,
        Vector2[] arrayB,
        LayerMask obstacleMask)
    {
        var result = new List<(int, int)>();

        for (int i = 0; i < arrayA.Length; i++)
        {
            Vector2 a = arrayA[i];
            for (int j = 0; j < arrayB.Length; j++)
            {
                Vector2 b = arrayB[j];

                // Line‐of‐sight check: Physics2D.Linecast returns true if something is hit
                if (Physics2D.Linecast(a, b, obstacleMask))
                    continue;

                // No obstacle between a and b → record this pair
                result.Add((i, j));
            }
        }

        return result;
    }

    /// <summary>
    /// Merges two adjacency‐list graphs and links them by adding edges between
    /// *every* mutually visible node‐pair (instead of just the single closest).
    /// </summary>
    /// <param name="graph1">First graph (indices match points1).</param>
    /// <param name="graph2">Second graph (indices match points2).</param>
    /// <param name="points1">Positions of graph1’s nodes.</param>
    /// <param name="points2">Positions of graph2’s nodes.</param>
    /// <param name="obstacleMask">LayerMask indicating which layers count as obstacles.</param>
    /// <returns>The merged graph (with graph2’s indices offset, plus new connections).</returns>
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
            merged[key2] = kv.Value.Select(n => n + offset).ToList();
        }

        // 3) Find all visible pairs
        var visiblePairs = FindAllVisiblePairs(points1, points2, obstacleMask);

        if (visiblePairs.Count == 0)
        {
            Debug.LogWarning("LinkAllVisible: no unobstructed links found—sub‐graphs remain disconnected.");
            return merged;
        }

        // 4) Add bidirectional edges for each visible pair
        foreach (var (i, j) in visiblePairs)
        {
            int idx1 = i;
            int idx2 = j + offset;

            // Link idx1 → idx2
            if (!merged[idx1].Contains(idx2))
                merged[idx1].Add(idx2);

            // Link idx2 → idx1
            if (!merged[idx2].Contains(idx1))
                merged[idx2].Add(idx1);
        }

        return merged;
    }

    public struct Subgraph
    {
        public Dictionary<int, List<int>> Graph;
        public Vector2[] Nodes;
    }

    public Subgraph[] CreateSubgraphs(Vector2[] points, LayerMask obstacleMask)
    {
        int n = points.Length;

        // 1) Build the full visibility graph: check each unordered pair (i<j)
        var fullAdj = new List<int>[n];
        for (int i = 0; i < n; i++)
            fullAdj[i] = new List<int>();

        for (int i = 0; i < n; i++)
        {
            for (int j = i + 1; j < n; j++)
            {
                // if there’s no obstacle between points[i] and points[j], link them
                if (!Physics2D.Linecast(points[i], points[j], obstacleMask))
                {
                    fullAdj[i].Add(j);
                    fullAdj[j].Add(i);
                }
            }
        }

        // 2) Find connected components via BFS
        var seen = new bool[n];
        var subs = new List<Subgraph>();

        for (int start = 0; start < n; start++)
        {
            if (seen[start]) continue;

            var queue = new Queue<int>();
            var comp = new List<int>();

            seen[start] = true;
            queue.Enqueue(start);

            while (queue.Count > 0)
            {
                int u = queue.Dequeue();
                comp.Add(u);
                foreach (int v in fullAdj[u])
                {
                    if (!seen[v])
                    {
                        seen[v] = true;
                        queue.Enqueue(v);
                    }
                }
            }

            // 3) Re‐index locally and build the Subgraph
            int m = comp.Count;
            var mapGlobalToLocal = new Dictionary<int, int>(m);
            for (int k = 0; k < m; k++)
                mapGlobalToLocal[comp[k]] = k;

            // local node positions
            var localNodes = new Vector2[m];
            for (int k = 0; k < m; k++)
                localNodes[k] = points[comp[k]];

            // local adjacency
            var localGraph = new Dictionary<int, List<int>>(m);
            for (int k = 0; k < m; k++)
            {
                int gi = comp[k];
                localGraph[k] = fullAdj[gi]
                    .Select(gj => mapGlobalToLocal[gj])
                    .ToList();
            }

            subs.Add(new Subgraph
            {
                Nodes = localNodes,
                Graph = localGraph
            });
        }

        return subs.ToArray();
    }

    /// <summary>
    /// Builds and returns the Subgraph of all points reachable from the first point in the array,
    /// taking obstacles into account (using Physics2D.Linecast).
    /// </summary>
    /// <param name="points">Array of all waypoint positions, with the start point at index 0.</param>
    /// <param name="obstacleMask">LayerMask indicating obstacles.</param>
    /// <returns>
    /// A Subgraph struct containing:
    /// - Nodes: Vector2[] of the reachable points, in original order as they appeared in the 'points' array.
    /// - Graph: Dictionary mapping local indices (0..m-1) to lists of local neighbor indices.
    /// </returns>
    public Subgraph CreateGraph(Vector2[] points, LayerMask obstacleMask)
    {
        int n = points.Length;
        if (n == 0)
        {
            Debug.LogWarning("GraphLinker: points array is empty.");
            return new Subgraph { Nodes = new Vector2[0], Graph = new Dictionary<int, List<int>>() };
        }

        // 1) Build full visibility adjacency list
        var fullAdj = new List<int>[n];
        for (int i = 0; i < n; i++)
            fullAdj[i] = new List<int>();

        for (int i = 0; i < n; i++)
        {
            for (int j = i + 1; j < n; j++)
            {
                if (!Physics2D.Linecast(points[i], points[j], obstacleMask))
                {
                    fullAdj[i].Add(j);
                    fullAdj[j].Add(i);
                }
            }
        }

        // 2) BFS from index 0 to collect reachable component indices
        var seen = new bool[n];
        var queue = new Queue<int>();
        var globalComponent = new List<int>();

        seen[0] = true;
        queue.Enqueue(0);

        while (queue.Count > 0)
        {
            int u = queue.Dequeue();
            globalComponent.Add(u);
            foreach (int v in fullAdj[u])
            {
                if (!seen[v])
                {
                    seen[v] = true;
                    queue.Enqueue(v);
                }
            }
        }

        // 3) Prepare local mapping and Nodes array in original order
        globalComponent.Sort();
        int m = globalComponent.Count;
        var mapGlobalToLocal = new Dictionary<int, int>(m);
        var localNodes = new Vector2[m];

        for (int k = 0; k < m; k++)
        {
            int gi = globalComponent[k];
            mapGlobalToLocal[gi] = k;
            localNodes[k] = points[gi];
        }

        // 4) Build local adjacency graph
        var localGraph = new Dictionary<int, List<int>>(m);
        foreach (int gi in globalComponent)
        {
            int ki = mapGlobalToLocal[gi];
            localGraph[ki] = fullAdj[gi]
                .Where(gj => mapGlobalToLocal.ContainsKey(gj))
                .Select(gj => mapGlobalToLocal[gj])
                .ToList();
        }

        return new Subgraph
        {
            Nodes = localNodes,
            Graph = localGraph
        };
    }
}
