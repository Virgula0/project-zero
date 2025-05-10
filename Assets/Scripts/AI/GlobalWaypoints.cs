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
    private Dictionary<IEnemy, Dictionary<int, List<int>>> enemyConnectionMap;
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
                                 // keep your ordering hack
                                 string[] parts = ((MonoBehaviour)e).transform.parent.name.Split(' ');
                                 return int.TryParse(parts[1], out int n) ? n : 0;
                             })
                             .ToArray();

        enemyWaypointsMap = new Dictionary<IEnemy, Vector2[]>();
        enemyConnectionMap = new Dictionary<IEnemy, Dictionary<int, List<int>>>();

        foreach (var e in enemies)
        {
            while (!e.AwakeReady())
                yield return null;

            enemyWaypointsMap[e] = e.GetEnemyWaypoints();
            enemyConnectionMap[e] = e.GetEnemyConnections();
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

    private void BuildFullGraph(GraphLinker linker)
    {
        // 1) Gather all components
        var enemyComps = enemyWaypointsMap
            .Select(kv => (graph: enemyConnectionMap[kv.Key], nodes: kv.Value, isGlobal: false))
            .ToList();

        var globalComps = linker
            .CreateSubgraphs(globalWaypoints, obstacleLayers)
            .Select(sub => (graph: sub.Graph, nodes: sub.Nodes, isGlobal: true))
            .ToList();

        // Combined list we’ll pull from
        var allComps = new List<(Dictionary<int, List<int>> graph, Vector2[] nodes, bool isGlobal)>();
        allComps.AddRange(enemyComps);
        allComps.AddRange(globalComps);

        if (allComps.Count == 0)
        {
            allNodes = new Vector2[0];
            allConnections = new Dictionary<int, List<int>>();
            return;
        }

        // 2) Initialize master graph from the very first component
        var first = allComps[0];
        var masterGraph = first.graph
            .ToDictionary(kv => kv.Key, kv => new List<int>(kv.Value));
        var nodeList = new List<Vector2>(first.nodes);

        // Remove it from the pool
        allComps.RemoveAt(0);

        // Helper to compute the minimal squared distance between any point
        // in `masterNodes` and any point in `compNodes`, *ignoring* obstacles.
        float EuclideanSqrDist(Vector2 a, Vector2 b) => (a - b).sqrMagnitude;

        // Copy of GraphLinker’s visibility test to ensure a valid link
        bool IsVisible(Vector2 a, Vector2 b)
        {
            return !Physics2D.Linecast(a, b, obstacleLayers);
        }

        // Finds true minimal *visible* pair distance between our current master and comp.
        // Returns sqrt‐distance (so we can compare globally).
        float ComputeMinVisibleDistance(Vector2[] masterNodes, Vector2[] compNodes)
        {
            float best = float.MaxValue;
            foreach (var a in masterNodes)
            {
                foreach (var b in compNodes)
                {
                    if (!IsVisible(a, b))
                        continue;
                    float d2 = EuclideanSqrDist(a, b);
                    if (d2 < best)
                        best = d2;
                }
            }
            return best == float.MaxValue ? float.PositiveInfinity : Mathf.Sqrt(best);
        }

        // 3) Iteratively merge the “closest” remaining component
        while (allComps.Count > 0)
        {
            // Precompute distances for each remaining comp
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

            // Decide which to merge: prefer global on tie
            int pickIdx;
            if (bestGlobalIdx >= 0 && bestGlobalDist <= bestEnemyDist)
                pickIdx = bestGlobalIdx;
            else
                pickIdx = bestEnemyIdx;

            // If both are infinite, no visible link—break out
            if (pickIdx < 0 || float.IsInfinity(
                allComps[pickIdx].isGlobal ? bestGlobalDist : bestEnemyDist))
            {
                Debug.LogWarning("Could not visibly link component → it remains disconnected.");
                break;
            }

            // Merge the picked component
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

        // 4) Finalize
        allNodes = nodeList.ToArray();
        allConnections = masterGraph;
    }

    public Vector2[] GetGlobalWaypoints()
    {
        return globalWaypoints.ToArray();
    }

    // Debugging purposes you can ignore this
    private void OnDrawGizmos()
    {
        if (transform.position == null)
            return;

        if (globalWaypoints == null)
            return;

        float circleRadius = 0.8f;
        foreach (Vector2 point in globalWaypoints)
        {
            // Calculate the starting point and ending point of the cast
            Vector3 startPoint = point;
            Vector3 endPoint = startPoint + (Vector3)(new Vector2(0, 0).normalized * circleRadius);

            // Set Gizmo color for visualization
            Gizmos.color = Color.magenta;

            // Draw the starting circle
            Gizmos.DrawWireSphere(startPoint, circleRadius);
            // Draw the ending circle
            Gizmos.DrawWireSphere(endPoint, circleRadius);

            // Draw a line connecting the two circles to illustrate the cast path
            Gizmos.DrawLine(startPoint, endPoint);
        }
    }
}
