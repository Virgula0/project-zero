using System.Collections.Generic;
using UnityEngine;

public interface IEnemy
{
    public Vector2[] GetEnemyWaypoints();
    public Dictionary<int, List<int>> GetEnemyConnections();
}