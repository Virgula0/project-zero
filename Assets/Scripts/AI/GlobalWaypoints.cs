using System.Collections.Generic;
using UnityEngine;

public class GlobalWaypoints : MonoBehaviour
{
    /*
    * GLOBALWAYPOINTS defines global waypoints for each scene.
    * They're used as waypoint from all enemies in the scane to understand "open spaces" which are common spaces between rooms
    * and they're useful for an enemy to find the path to they're rooms
    * The data vector is merged with each enemy waypointVector before to define structure and BFS searches...
    */
    [SerializeField] private Vector2[] globalWaypoints;

    private Dictionary<int, int> globalWaypointsRemapped; // remapping with high indexes so they won't collide with real indexes of enemies graphs

    private int baseCounter = 100000; // starting from 100000
    void Awake()
    {
         this.globalWaypointsRemapped = GenerateMapping();
    }

    private Dictionary<int, int> GenerateMapping()
    {
        Dictionary<int, int> mapping = new Dictionary<int, int>();
        
        // Loop over each element in the waypoints array.
        for (int i = 0; i < globalWaypoints.Length; i++)
        {
            int generatedValue = baseCounter++;
            mapping.Add(generatedValue, i);
        }

        return mapping;
    }

    public Dictionary<int, int> GetGlobalWaypointsRemapped(){
        return globalWaypointsRemapped;
    }

    public Vector2 GetElementFromRemappedIndex(int remappedIndex){
        return globalWaypoints[globalWaypointsRemapped.GetValueOrDefault(remappedIndex, -1)];
    }

    // Debugging purposes you can ignore this
    private void OnDrawGizmos()
    {
        if (transform.position == null)
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
