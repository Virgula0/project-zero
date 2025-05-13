using System.Collections.Generic;
using UnityEngine;

public interface IEnemy
{
    public Vector2[] GetEnemyWaypoints();
    public Vector2[] GetEnemyPatrolPoints();
    public Dictionary<int, List<int>> GetEnemyConnections();
     public Dictionary<int, List<int>> GetEnemyConnectionsPatrolPoints();
    bool AwakeReady();
    bool IsEnemyDead();
    void SetIsEnemyDead(bool cond);
    void SetIsEnemyStunned(float duration = 3f); // by default stun will be 3 seconds for any enemy
    bool IsStunned();
    IMovement GetCurrentMovement();
}