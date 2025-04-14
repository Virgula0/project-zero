using UnityEngine;

public interface IDash
{
    bool Dash(Rigidbody2D playerRb, float dashTimer);
    void SetDashDirection(Vector2 direction);
}
