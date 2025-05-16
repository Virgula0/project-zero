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
