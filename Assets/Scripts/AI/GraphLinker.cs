using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Finds the pair of Vector2 (one from each array) with the minimum squared distance.
/// </summary>
public class GraphLinker
{
    private static void FindClosestPair(Vector2[] arrayA, Vector2[] arrayB, out Vector2 bestFromA, out Vector2 bestFromB)
    {
        float minSqrDistance = float.MaxValue;
        bestFromA = Vector2.zero;
        bestFromB = Vector2.zero;

        // Iterate over all pairs.
        foreach (Vector2 a in arrayA)
        {
            foreach (Vector2 b in arrayB)
            {
                // Use squared distance to avoid the cost of the square root.
                float currentSqrDistance = (a - b).sqrMagnitude;
                if (currentSqrDistance < minSqrDistance)
                {
                    minSqrDistance = currentSqrDistance;
                    bestFromA = a;
                    bestFromB = b;
                }
            }
        }
    }

    /// <summary>
    /// Links two graph dictionaries by re-indexing the second graph and adding an edge between the two closest nodes.
    /// </summary>
    /// <param name="graph1">The first graph dictionary, with keys matching the indices of points1.</param>
    /// <param name="graph2">The second graph dictionary, with keys matching the indices of points2.</param>
    /// <param name="points1">The ordered array of Vector2 for graph1.</param>
    /// <param name="points2">The ordered array of Vector2 for graph2.</param>
    /// <returns>A new dictionary representing the merged graph with an added connection between the two closest nodes.</returns>
    public Dictionary<int, List<int>> LinkGraphs(
        Dictionary<int, List<int>> graph1,
        Dictionary<int, List<int>> graph2,
        Vector2[] points1,
        Vector2[] points2)
    {
        // Create a new dictionary for the merged graph
        Dictionary<int, List<int>> mergedGraph = new Dictionary<int, List<int>>();

        // First, add all of graph1 as-is.
        foreach (var kvp in graph1)
        {
            // Clone the list if necessary to avoid modifying the original dictionary.
            mergedGraph.Add(kvp.Key, new List<int>(kvp.Value));
        }

        // Determine the offset as the count of nodes in graph1.
        int offset = graph1.Count;

        // Re-index graph2 and add its nodes into mergedGraph.
        foreach (var kvp in graph2)
        {
            int newKey = kvp.Key + offset;
            List<int> newNeighbors = new List<int>();
            foreach (int neighbor in kvp.Value)
            {
                newNeighbors.Add(neighbor + offset);
            }
            mergedGraph.Add(newKey, newNeighbors);
        }

        // Find the closest pair of nodes between points1 and points2.
        Vector2 bestFrom1, bestFrom2;
        FindClosestPair(points1, points2, out bestFrom1, out bestFrom2);

        // Identify their indices.
        int indexFrom1 = Array.IndexOf(points1, bestFrom1);
        int indexFrom2 = Array.IndexOf(points2, bestFrom2);
        // Re-index for the second graph.
        int reindexedIndexFrom2 = indexFrom2 + offset;

        // Now, add a connection (bidirectional) between these two nodes.
        // For graph1 node:
        if (mergedGraph.ContainsKey(indexFrom1))
        {
            if (!mergedGraph[indexFrom1].Contains(reindexedIndexFrom2))
            {
                mergedGraph[indexFrom1].Add(reindexedIndexFrom2);
            }
        }
        else
        {
            mergedGraph[indexFrom1] = new List<int> { reindexedIndexFrom2 };
        }

        // For graph2 node (re-indexed):
        if (mergedGraph.ContainsKey(reindexedIndexFrom2))
        {
            if (!mergedGraph[reindexedIndexFrom2].Contains(indexFrom1))
            {
                mergedGraph[reindexedIndexFrom2].Add(indexFrom1);
            }
        }
        else
        {
            mergedGraph[reindexedIndexFrom2] = new List<int> { indexFrom1 };
        }

        return mergedGraph;
    }
}
