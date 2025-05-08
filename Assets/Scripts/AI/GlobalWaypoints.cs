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
    private int baseCounter = 100000; // starting from 100000

    private Dictionary<IEnemy, Vector2[]> enemyWaypointsMap;
    private List<Dictionary<int, List<int>>> enemyConnectionMap;
    private List<IEnemy> enemies;
    private GraphLinker linker;

    private bool isGlobalReady = false;

    public bool GetIsGlobalReady(){
        return isGlobalReady;
    }

    void Awake()
    {
        linker = new GraphLinker();
        StartCoroutine(this.PopulateEnemyWaypointsMap());
    }

    private IEnumerator PopulateEnemyWaypointsMap()
    {
        this.enemyWaypointsMap = new Dictionary<IEnemy, Vector2[]>();
        this.enemyConnectionMap = new List<Dictionary<int, List<int>>>();
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
            enemyConnectionMap.Add(enemy.GetEnemyConnections());
        }

        // at this point we have all enemies internal graph

        foreach (Dictionary<int, List<int>> enemyInternalGraph in enemyConnectionMap)
        {
            
        }

        isGlobalReady = true;
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

    public Vector2[] GetGlobalWaypointsVector(){
        return globalWaypoints;
    }

    public Dictionary<int, List<int>> GetConnectionMap()
    {
        return null;
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
