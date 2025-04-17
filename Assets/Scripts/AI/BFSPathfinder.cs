using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BFSPathfinder
{
    private Vector2[] waypoints;
    private Dictionary<int, List<int>> adjacency; // Graph connections: key = waypoint index, value = list of neighbor indices

    public BFSPathfinder(Vector2[] waypoints, Dictionary<int, List<int>> adjacency)
    {
        this.waypoints = waypoints;
        this.adjacency = adjacency;
    }

    public Vector2[] PathToTheFirst(Vector2 start)
    {
        return PathToPoint(start, waypoints[0]);
    }

    public Vector2[] PathToPoint(Vector2 start, Vector2 goal)
    {
        // Find index of the start point.
        int startIndex = -1;
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == start)
            {
                startIndex = i;
                break;
            }
        }
        if (startIndex == -1)
        {
            Debug.LogError("Start point is not in the waypoints array!");
            return null;
        }

        // Find index of the goal point.
        int goalIndex = -1;
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == goal)
            {
                goalIndex = i;
                break;
            }
        }
        if (goalIndex == -1)
        {
            Debug.LogError("Goal point is not in the waypoints array!");
            return null;
        }

        // Initialize BFS components.
        Queue<int> queue = new Queue<int>();
        bool[] visited = new bool[waypoints.Length];
        int[] cameFrom = new int[waypoints.Length];
        for (int i = 0; i < cameFrom.Length; i++)
        {
            cameFrom[i] = -1; // -1 indicates no predecessor.
        }

        queue.Enqueue(startIndex);
        visited[startIndex] = true;

        bool found = false;
        while (queue.Count > 0)
        {
            int current = queue.Dequeue();

            // If we've reached the destination, stop searching.
            if (current == goalIndex)
            {
                found = true;
                break;
            }

            // Check all neighbors of the current waypoint.
            if (adjacency.ContainsKey(current))
            {
                foreach (int neighbor in adjacency[current])
                {
                    if (!visited[neighbor])
                    {
                        visited[neighbor] = true;
                        cameFrom[neighbor] = current;
                        queue.Enqueue(neighbor);
                    }
                }
            }
        }

        // If no path was found, return null.
        if (!found)
        {
            Debug.LogError("No path found from the start point to the goal.");
            return null;
        }

        // Reconstruct the path from the goal back to the start using cameFrom.
        List<int> pathIndices = new List<int>();
        int currentIndex = goalIndex;
        while (currentIndex != -1)
        {
            pathIndices.Add(currentIndex);
            currentIndex = cameFrom[currentIndex];
        }
        // Reverse to get the path from start point to goal.
        pathIndices.Reverse();

        // Convert indices to their corresponding Vector2 positions.
        Vector2[] path = new Vector2[pathIndices.Count];
        for (int i = 0; i < pathIndices.Count; i++)
        {
            path[i] = waypoints[pathIndices[i]];
        }
        return path;
    }
}
