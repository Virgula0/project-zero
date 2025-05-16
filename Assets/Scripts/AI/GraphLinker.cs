using System.Collections.Generic;
using UnityEngine;
using System.Linq;

// Finds the pair of Vector2 (one from each array) with the minimum squared distance.
public class GraphLinker
{
    private const float DefaultWaypointRadius = 0.8f;
    private const float DefaultClearance = 0.5f;
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
    /// Finds every pair (one point from A, one point from B) whose connecting
    /// “tube” of radius (waypointRadius + clearance) is completely clear of colliders.
    /// </summary>
    /// <param name="waypointRadius">the nominal radius of each waypoint</param>
    /// <param name="clearance">
    /// extra safety margin you want around each circle (so total radius = waypointRadius + clearance)
    /// </param>
    private static List<(int aIndex, int bIndex)> FindAllVisiblePairs(
        Vector2[] arrayA,
        Vector2[] arrayB,
        LayerMask obstacleMask,
        float waypointRadius = DefaultWaypointRadius,
        float clearance = DefaultClearance)
    {
        var result = new List<(int, int)>();
        // total “sweep” radius
        float sweepR = waypointRadius + clearance;

        for (int i = 0; i < arrayA.Length; i++)
        {
            Vector2 a = arrayA[i];
            for (int j = 0; j < arrayB.Length; j++)
            {
                Vector2 b = arrayB[j];
                Vector2 delta = b - a;
                float fullDist = delta.magnitude;

                // if the grown circles actually overlap, we can consider them mutually visible
                if (fullDist <= 2f * sweepR)
                {
                    result.Add((i, j));
                    continue;
                }

                // direction from A→B
                Vector2 dir = delta / fullDist;

                // start the cast just outside A’s grown circle, 
                // and only sweep the middle segment (so we don't double-cast the endpoints)
                Vector2 castStart = a + dir * sweepR;
                float castDist = fullDist - 2f * sweepR;

                // cast a circle of radius=sweepR
                RaycastHit2D hit = Physics2D.CircleCast(
                    castStart,     // origin
                    sweepR,        // radius
                    dir,           // direction
                    castDist,      // max distance
                    obstacleMask);

                if (hit.collider == null)
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


    /// <summary>
    /// Returns true if the “tube” of radius (waypointRadius+clearance)
    /// between a and b is completely obstacle‑free.
    /// </summary>
    public static bool IsVisible(
        Vector2 a,
        Vector2 b,
        LayerMask obstacleMask,
        float waypointRadius = DefaultWaypointRadius,
        float clearance = DefaultClearance)
    {
        float sweepR = waypointRadius + clearance;
        Vector2 delta = b - a;
        float fullDist = delta.magnitude;

        // if the two grown circles overlap, it's trivially clear
        if (fullDist <= 2f * sweepR)
            return true;

        Vector2 dir = delta / fullDist;
        Vector2 castStart = a + dir * sweepR;
        float castDist = fullDist - 2f * sweepR;

        // sweep the middle segment
        RaycastHit2D hit = Physics2D.CircleCast(
            castStart,
            sweepR,
            dir,
            castDist,
            obstacleMask);

        return hit.collider == null;
    }

    public Subgraph[] CreateSubgraphs(
        Vector2[] points,
        LayerMask obstacleMask)
    {
        int n = points.Length;

        // 1) Build the full visibility graph using our clearance‐aware check
        var fullAdj = new List<int>[n];
        for (int i = 0; i < n; i++)
            fullAdj[i] = new List<int>();

        for (int i = 0; i < n; i++)
        {
            for (int j = i + 1; j < n; j++)
            {
                if (IsVisible(points[i], points[j], obstacleMask))
                {
                    fullAdj[i].Add(j);
                    fullAdj[j].Add(i);
                }
            }
        }

        // 2) BFS to extract connected components
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

            // 3) build the Subgraph for this component
            int m = comp.Count;
            var mapGlobalToLocal = new Dictionary<int, int>(m);
            var localNodes = new Vector2[m];
            for (int k = 0; k < m; k++)
            {
                mapGlobalToLocal[comp[k]] = k;
                localNodes[k] = points[comp[k]];
            }

            var localGraph = new Dictionary<int, List<int>>(m);
            for (int k = 0; k < m; k++)
            {
                int gi = comp[k];
                localGraph[k] = fullAdj[gi]
                    .Where(gj => mapGlobalToLocal.ContainsKey(gj))
                    .Select(gj => mapGlobalToLocal[gj])
                    .ToList();
            }

            subs.Add(new Subgraph { Nodes = localNodes, Graph = localGraph });
        }

        return subs.ToArray();
    }

    public Subgraph CreateGraph(
        Vector2[] points,
        LayerMask obstacleMask)
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
            for (int j = i + 1; j < n; j++)
                if (IsVisible(points[i], points[j], obstacleMask))
                {
                    fullAdj[i].Add(j);
                    fullAdj[j].Add(i);
                }

        // 2) BFS from index 0
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
                if (!seen[v])
                {
                    seen[v] = true;
                    queue.Enqueue(v);
                }
        }

        // 3) Local remapping
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

        // 4) Build local adjacency
        var localGraph = new Dictionary<int, List<int>>(m);
        foreach (int gi in globalComponent)
        {
            int ki = mapGlobalToLocal[gi];
            localGraph[ki] = fullAdj[gi]
                .Where(gj => mapGlobalToLocal.ContainsKey(gj))
                .Select(gj => mapGlobalToLocal[gj])
                .ToList();
        }

        return new Subgraph { Nodes = localNodes, Graph = localGraph };
    }
}
