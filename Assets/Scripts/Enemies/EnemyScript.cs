using UnityEngine;

// updates sprites based on the movement
public class EnemyScript : MonoBehaviour
{
    private Rigidbody2D body;
    private Vector2 lastPosition;
    private SpriteRenderer sprite;
    private Detector playerDetector;
    private Rigidbody2D playerBody;

    void Start()
    {   
        sprite = GetComponentInChildren<SpriteRenderer>();
        body = GetComponentInChildren<Rigidbody2D>();
        playerDetector = gameObject.GetComponentInChildren<Detector>();
        playerBody = GameObject.FindGameObjectWithTag(Utils.Const.PLAYER_TAG).GetComponentInChildren<Rigidbody2D>();
        lastPosition = body.position;
    }

    void FixedUpdate()
    {
        Vector2 currentPosition = body.position;
        Vector2 movement = currentPosition - lastPosition;

        if (playerDetector.GetIsEnemyAwareOfPlayer())
        {
            movement = playerBody.position - currentPosition;
        }

        if (playerDetector.GetIsEnemyAwareOfPlayer() || movement.sqrMagnitude > 0.001f)
        {
            float angle = Mathf.Atan2(movement.y, movement.x) * Mathf.Rad2Deg;
            sprite.transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        lastPosition = currentPosition;
    }
}
