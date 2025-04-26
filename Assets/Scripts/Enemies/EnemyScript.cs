using UnityEngine;

// updates sprites based on the movement
public class EnemyScript : MonoBehaviour
{
    private Rigidbody2D body;
    private Vector2 lastPosition;
    private SpriteRenderer sprite;

    void Start()
    {   
        sprite = GetComponentInChildren<SpriteRenderer>();
        body = GetComponentInChildren<Rigidbody2D>();
        lastPosition = body.position;
    }

    void FixedUpdate()
    {
        Vector2 currentPosition = body.position;
        Vector2 movement = currentPosition - lastPosition;

        if (movement.sqrMagnitude > 0.001f)
        {
            float angle = Mathf.Atan2(movement.y, movement.x) * Mathf.Rad2Deg;
            sprite.transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
        lastPosition = currentPosition;
    }
}
