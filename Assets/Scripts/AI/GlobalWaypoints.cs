using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GlobalWaypoints : MonoBehaviour
{
    [SerializeField] private Vector2[] globalWaypoints;
    [SerializeField] private LayerMask obstacleLayers;

    private Vector2[] allNodes;
    private Dictionary<int, List<int>> allConnections;
    private KdTree kdTree;
    private bool isGlobalReady = false;
    private Dictionary<IEnemy, Vector2[]> enemyWaypointsMap;
    private Dictionary<IEnemy, Vector2[]> enemyPatrolMap;
    private Dictionary<IEnemy, Dictionary<int, List<int>>> enemyConnectionMap;
    private Dictionary<IEnemy, Dictionary<int, List<int>>> enemyPatrolConnectionMap;
    private PathFinder bfs;

    public bool GetIsGlobalReady() => isGlobalReady;
    public KdTree GetKdTree() => kdTree;
    public PathFinder GetPathFinder() => bfs;

    void Awake()
    {
        StartCoroutine(PopulateAndBuildGraph());
    }

    private IEnumerator PopulateAndBuildGraph()
    {
        var linker = new GraphLinker();
        // 1) Gather every enemy’s own waypoints & conn-map
        IEnemy[] enemies = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
                             .OfType<IEnemy>()
                             .OrderByDescending(e =>
                             {
                                 string[] parts = ((MonoBehaviour)e).transform.parent.name.Split(' ');
                                 return int.TryParse(parts[1], out int n) ? n : 0;
                             })
                             .ToArray();

        enemyWaypointsMap = new Dictionary<IEnemy, Vector2[]>();
        enemyConnectionMap = new Dictionary<IEnemy, Dictionary<int, List<int>>>();
        enemyPatrolMap = new Dictionary<IEnemy, Vector2[]>();
        enemyPatrolConnectionMap = new Dictionary<IEnemy, Dictionary<int, List<int>>>();

        foreach (var e in enemies)
        {
            while (!e.AwakeReady())
                yield return null;

            enemyWaypointsMap[e] = e.GetEnemyWaypoints();
            enemyConnectionMap[e] = e.GetEnemyConnections();
            enemyPatrolMap[e] = e.GetEnemyPatrolPoints();
            enemyPatrolConnectionMap[e] = e.GetEnemyConnectionsPatrolPoints();
        }

        // 2) Merge global + every enemy subgraph exactly once
        BuildFullGraph(linker);

        // 3) Build the master KD-tree over allNodes
        kdTree = new KdTree(allNodes);

        // 4) Build PathFinder
        bfs = new PathFinder(allNodes, allConnections);

        isGlobalReady = true;
        yield break;
    }

    /// <summary>
    /// Constructs a unified navigation graph by combining per‐enemy waypoint/patrol subgraphs
    /// and global waypoints, then linking them based on visibility and proximity.
    /// </summary>
    /// <remarks>
    /// The overall process proceeds in five main phases:
    /// 
    /// 1) **Enemy Subgraph Assembly**  
    ///    - For each enemy, retrieve its waypoint nodes and existing waypoint‐waypoint connections.  
    ///    - If the enemy also has patrol points:  
    ///      • Merge the waypoint and patrol subgraphs via <c>linker.LinkGraphs</c>, which automatically
    ///        adds any visible cross‐links (i.e. line‐of‐sight connections not blocked by obstacles).  
    ///      • Force a bidirectional link between the first waypoint and the first patrol point, ensuring
    ///        scripted entry/exit between these loops.  
    ///      • Concatenate the waypoint and patrol node arrays into one component.  
    ///    - If no patrol points exist, treat the waypoint graph alone as the component.  
    ///    - Store each component in <c>enemyComps</c> (marking them as non‐global).  
    /// 
    /// 2) **Global Subgraph Assembly**  
    ///    - Call <c>linker.CreateSubgraphs(globalWaypoints, obstacleLayers)</c> to partition the global
    ///      waypoint set into connected components (considering obstacle blocking).  
    ///    - Store each resulting graph & node set in <c>globalComps</c> and flag them as global.  
    /// 
    /// 3) **Master Graph Initialization**  
    ///    - Combine all components (enemy first, then global) into a single list <c>allComps</c>.  
    ///    - If no components exist, initialize <c>allNodes</c> and <c>allConnections</c> to empty and return.  
    ///    - Otherwise, take the first component as the “master” graph:  
    ///      • Copy its adjacency map into <c>masterGraph</c>.  
    ///      • Copy its node positions into <c>nodeList</c>.  
    /// 
    /// 4) **Iterative Merging of Remaining Components**  
    ///    - Define helper functions:  
    ///      • <c>EuclideanSqrDist</c>(a, b): squared distance between two points.  
    ///      • <c>IsVisible</c>(a, b): true if no obstacle blocks the tube of radius (waypointRadius+clearance).  
    ///      • <c>ComputeMinVisibleDistance</c>: for a candidate component, finds the minimum straight‐line
    ///        distance from any master node to any component node that is also visible.  
    ///    - While there are still components to merge:  
    ///      • Compute for each component the min visible distance to the current master.  
    ///      • Track the best (smallest) distance among global and among enemy components separately.
    ///      • Prefer merging the closest global component if its distance is ≤ the best enemy distance; otherwise,
    ///        pick the closest enemy component.  
    ///      • If no visible link exists (distance infinite), abort with a warning (component remains disconnected).  
    ///      • Otherwise, merge the selected component into <c>masterGraph</c> via <c>linker.LinkGraphs</c>,
    ///        then append its nodes to <c>nodeList</c> and remove it from the list.  
    /// 
    /// 5) **Finalize**  
    ///    - Convert <c>nodeList</c> to <c>allNodes</c> (array of Vector2).  
    ///    - Assign <c>allConnections</c> to the fully linked adjacency dictionary.  
    /// 
    /// The resulting <c>allNodes</c> and <c>allConnections</c> provide a complete, connected graph
    /// suitable for pathfinding across all enemy and global waypoints, respecting visibility constraints.
    /// </remarks>
    private void BuildFullGraph(GraphLinker linker)
    {
        // 1) Gather enemy subgraphs—merging each enemy’s waypoints + patrol points first
        var enemyComps = new List<(Dictionary<int, List<int>> graph, Vector2[] nodes, bool isGlobal)>();
        foreach (var kv in enemyWaypointsMap)
        {
            var enemy = kv.Key;
            var wpNodes = enemyWaypointsMap[enemy];
            var wpGraph = new Dictionary<int, List<int>>(enemyConnectionMap[enemy]);

            // If this enemy has patrol points, merge that subgraph in
            if (enemyPatrolMap.TryGetValue(enemy, out var patrolNodes) &&
                enemyPatrolConnectionMap.TryGetValue(enemy, out var patrolGraph) &&
                patrolNodes.Length > 0)
            {
                // First, link the two graphs together (will auto-handle any visible links)
                var mergedGraph = linker.LinkGraphs(
                    wpGraph,
                    patrolGraph,
                    wpNodes,
                    patrolNodes,
                    obstacleLayers
                );

                // Now force a direct connection between waypoint[0] and patrol[0]
                int wpIndex = 0;
                int patrolIndex = wpNodes.Length; // patrol nodes start immediately after
                                                  // ensure adjacency lists exist
                if (!mergedGraph.ContainsKey(wpIndex)) mergedGraph[wpIndex] = new List<int>();
                if (!mergedGraph.ContainsKey(patrolIndex)) mergedGraph[patrolIndex] = new List<int>();
                // add bidirectional link
                mergedGraph[wpIndex].Add(patrolIndex);
                mergedGraph[patrolIndex].Add(wpIndex);

                // combine the node arrays
                var mergedNodes = wpNodes.Concat(patrolNodes).ToArray();
                enemyComps.Add((mergedGraph, mergedNodes, false));
            }
            else
            {
                // no patrol points: just use waypoint subgraph
                enemyComps.Add((wpGraph, wpNodes, false));
            }
        }

        // 2) Gather global subgraphs as before
        var globalComps = linker
            .CreateSubgraphs(globalWaypoints, obstacleLayers)
            .Select(sub => (graph: sub.Graph, nodes: sub.Nodes, isGlobal: true))
            .ToList();

        // Combine all components
        var allComps = new List<(Dictionary<int, List<int>> graph, Vector2[] nodes, bool isGlobal)>();
        allComps.AddRange(enemyComps);
        allComps.AddRange(globalComps);

        if (allComps.Count == 0)
        {
            allNodes = new Vector2[0];
            allConnections = new Dictionary<int, List<int>>();
            return;
        }

        // 3) Initialize the master from the first component
        var first = allComps[0];
        var masterGraph = first.graph
            .ToDictionary(kv => kv.Key, kv => new List<int>(kv.Value));
        var nodeList = new List<Vector2>(first.nodes);
        allComps.RemoveAt(0);

        // Helpers for merging
        float EuclideanSqrDist(Vector2 a, Vector2 b) => (a - b).sqrMagnitude;
        bool Visible(Vector2 a, Vector2 b) =>
            GraphLinker.IsVisible(a, b, obstacleLayers);

        float ComputeMinVisibleDistance(Vector2[] masterNodes, Vector2[] compNodes)
        {
            float best = float.PositiveInfinity;
            foreach (var a in masterNodes)
                foreach (var b in compNodes)
                    if (Visible(a, b))
                    {
                        float d2 = EuclideanSqrDist(a, b);
                        if (d2 < best) best = d2;
                    }

            return best < float.PositiveInfinity
                ? Mathf.Sqrt(best)
                : float.PositiveInfinity;
        }

        // 4) Iteratively merge the remaining comps (preferring globals on ties)
        while (allComps.Count > 0)
        {
            var masterArr = nodeList.ToArray();
            float bestEnemyDist = float.PositiveInfinity, bestGlobalDist = float.PositiveInfinity;
            int bestEnemyIdx = -1, bestGlobalIdx = -1;

            for (int i = 0; i < allComps.Count; i++)
            {
                var (g, nodes, isG) = allComps[i];
                float dist = ComputeMinVisibleDistance(masterArr, nodes);
                if (isG)
                {
                    if (dist < bestGlobalDist)
                    {
                        bestGlobalDist = dist;
                        bestGlobalIdx = i;
                    }
                }
                else
                {
                    if (dist < bestEnemyDist)
                    {
                        bestEnemyDist = dist;
                        bestEnemyIdx = i;
                    }
                }
            }

            int pickIdx;
            if (bestGlobalIdx >= 0 && bestGlobalDist <= bestEnemyDist)
                pickIdx = bestGlobalIdx;
            else
                pickIdx = bestEnemyIdx;

            // If we can't link anything, bail out
            if (pickIdx < 0 ||
                float.IsInfinity(allComps[pickIdx].isGlobal ? bestGlobalDist : bestEnemyDist))
            {
                Debug.LogWarning("Could not visibly link component → it remains disconnected.");
                break;
            }

            // Merge it in
            var comp = allComps[pickIdx];
            masterGraph = linker.LinkGraphs(
                masterGraph,
                comp.graph,
                nodeList.ToArray(),
                comp.nodes,
                obstacleLayers
            );
            nodeList.AddRange(comp.nodes);
            allComps.RemoveAt(pickIdx);
        }

        // 5) Finalize
        allNodes = nodeList.ToArray();
        allConnections = masterGraph;
    }

    public Vector2[] GetGlobalWaypoints() => globalWaypoints.ToArray();

    private void OnDrawGizmos()
    {
        if (globalWaypoints == null) return;
        float circleRadius = 0.8f;
        foreach (Vector2 point in globalWaypoints)
        {
            Vector3 start = point;
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(start, circleRadius);
            Gizmos.DrawWireSphere(start + Vector3.zero * circleRadius, circleRadius);
            Gizmos.DrawLine(start, start + Vector3.zero * circleRadius);
        }
    }
}
