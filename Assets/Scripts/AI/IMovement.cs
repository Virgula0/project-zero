using UnityEngine;

public interface IMovement{
    void Move(Rigidbody2D enemyTransform);
    void CustomSetter<T>(T varToSet);
}