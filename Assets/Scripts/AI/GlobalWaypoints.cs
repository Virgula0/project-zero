using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GlobalWaypoints : MonoBehaviour
{
    /*
    * GLOBALWAYPOINTS defines global waypoints for each scene.
    * They're used as waypoint from all enemies in the scane to understand "open spaces" which are common spaces between rooms
    * and they're useful for an enemy to find the path to they're rooms
    * The data vector is merged with each enemy waypointVector before to define structure and BFS searches...
    * THIS CAN BE IMPROVED
    * GlobalWaypoints could mantain remapped waypoints for all enemies in the scene . Then each enemy gets the full map from the global WayPoints.
    */
    [SerializeField] private Vector2[] globalWaypoints;
    private Dictionary<int, int> globalWaypointsRemapped; // remapping with high indexes so they won't collide with real indexes of enemies graphs
    private int baseCounter = 100000; // starting from 100000

    private Dictionary<IEnemy, Vector2[]> enemyWaypointsMap;
    private Dictionary<IEnemy, Dictionary<int, List<int>>> enemyConnectionMap;
    private List<IEnemy> enemies;

    private bool isGlobalReady = false;

    public bool GetIsGlobalReady(){
        return isGlobalReady;
    }

    void Awake()
    {
        this.globalWaypointsRemapped = GenerateMapping(); // for global waypoints
        StartCoroutine(this.PopulateEnemyWaypointsMap());
    }

    private IEnumerator PopulateEnemyWaypointsMap()
    {
        this.enemyWaypointsMap = new Dictionary<IEnemy, Vector2[]>();
        this.enemyConnectionMap = new Dictionary<IEnemy, Dictionary<int, List<int>>>();
        this.enemies = new List<IEnemy>();
                
        IEnemy[] enemRef = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).
                OfType<IEnemy>().
                ToArray();

        foreach (IEnemy enemy in enemRef)
        {

            while (!enemy.AwakeReady()){
                yield return null;
            }
            
            enemies.Add(enemy);
            enemyWaypointsMap.Add(enemy, enemy.GetEnemyWaypoints());
            enemyConnectionMap.Add(enemy, enemy.GetEnemyConnections());
        }
        isGlobalReady = true;
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

    public List<IEnemy> GetEnemies(IEnemy toSkip){
        // Return all enemies except the one to skip.
        return enemies.Where(enemy => enemy != toSkip).ToList();
    }

    public Vector2[] GetWaypointMapForAnEnemy(IEnemy obj)
    {
        return enemyWaypointsMap[obj];
    }

    public Dictionary<int, List<int>> GetConnectionMapForAnEnemy(IEnemy obj)
    {
        return enemyConnectionMap[obj];
    }

    public Dictionary<int, int> GetGlobalWaypointsRemapped()
    {
        return globalWaypointsRemapped;
    }

    public Vector2 GetElementFromRemappedIndex(int remappedIndex)
    {
        return globalWaypoints[globalWaypointsRemapped.GetValueOrDefault(remappedIndex, -1)];
    }

    public Vector2[] GetGlobalWaypointsNotRemappedVector(){
        return globalWaypoints;
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
