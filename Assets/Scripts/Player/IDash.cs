using UnityEngine;

public interface IDash
{
    public IDash New(Rigidbody2D body); // define constuctor
    bool Dash(Vector2 direction, bool dashRequested);
    bool IsDashing { get; }
}