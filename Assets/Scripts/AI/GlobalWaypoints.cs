using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
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
    [SerializeField] private LayerMask obstacles;
    private Dictionary<IEnemy, Vector2[]> enemyWaypointsMap;
    private Dictionary<IEnemy, Dictionary<int, List<int>>> enemyConnectionMap;
    private List<IEnemy> enemies;
    private GraphLinker linker;
    private GraphLinker.Subgraph[] globalSubraphs;

    private Dictionary<IEnemy, bool> startsReady = new Dictionary<IEnemy, bool>();

    private bool isGlobalReady = false;

    public bool GetIsGlobalReady(){
        return isGlobalReady;
    }

    void Awake()
    {
        this.linker = new GraphLinker();
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
            startsReady.Add(enemy, false);
        }

        GenerateGlobalSubgraphs();
        isGlobalReady = true;
    }

    private void GenerateGlobalSubgraphs()
    {
        // Create subgraphs

        if (obstacles.IsUnityNull()){
            Debug.LogError("Obstacles masks in global waypoint is null");
            return;
        }

        this.globalSubraphs = linker.CreateSubgraphs(globalWaypoints, obstacles);
    }

    public void SetEnemyStartReady(IEnemy enemy){
        startsReady[enemy] = true;
    }

    public bool GetAllEnemiesReady()
    {
        return startsReady.Values.All(x => x);
    }

    public GraphLinker.Subgraph[] GetGlobalSubgraphs()
    {
        return this.globalSubraphs;
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

    public Vector2[] GetGlobalWaypoints(){
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
