using UnityEngine;

public interface IMovement{
    void Move(Rigidbody2D enemyTransform);
    void NeedsRepositioning(bool reposition);
    void StopCoroutines(bool stop);
}