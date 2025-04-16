using System;
using System.Collections.Generic;
using UnityEngine;

// Finds the pair of Vector2 (one from each array) with the minimum squared distance.
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
