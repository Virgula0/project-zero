using UnityEngine;

public class ThrowableScript : MonoBehaviour
{
    private Camera playerCamera;                            // Reference to main camera for viewport checks
    [SerializeField] private const float speed = 20f;       // Object travel speed in units per second
    [SerializeField] private float rotationSpeed = 720f;    // Rotation speed in degrees per second
    private Vector2 moveDirection;                          // Normalized direction vector for object movement
    [SerializeField] private LayerMask hitLayers;           // Layers that can be hit by the object
    [SerializeField] private float collisionBuffer = 0.1f;  // Small distance buffer to prevent edge-case misses
    private LogicManager logic;
    private SpriteRenderer spriteToRender;
    private Rigidbody2D objBody;
    private BoxCollider2D box;

    public void Initialize(SpriteRenderer sprite)
    {
        gameObject.layer = (int)Utils.Enums.ObjectLayers.BulletByPlayer;
        this.spriteToRender = sprite;
    }

    void Start()
    {
        logic = GameObject.FindGameObjectWithTag(Utils.Const.LOGIC_MANAGER_TAG)
            .GetComponent<LogicManager>();

        objBody = GetComponent<Rigidbody2D>();
        playerCamera = Camera.main;

        Vector2 mousePosition = playerCamera.ScreenToWorldPoint(Input.mousePosition);
        moveDirection = (mousePosition - objBody.position).normalized;
        transform.right = moveDirection;

        if (spriteToRender == null)
        {
            Debug.LogError("Sprite to render while throwing weapon cannot be null");
            return;
        }
        var childRenderer = GetComponentInChildren<SpriteRenderer>();
        childRenderer.sprite = spriteToRender.sprite;

        box = GetComponent<BoxCollider2D>();
        if (box != null && childRenderer.sprite != null)
        {
            var b = childRenderer.sprite.bounds;
            box.size = b.size;
            box.offset = b.center;
        }
    }

    void FixedUpdate()
    {
        float moveDistance = speed * Time.fixedDeltaTime;

        int shooterLayerValue = (int)Mathf.Pow(2, (int)Utils.Enums.ObjectLayers.Player);
        LayerMask finalHitLayers = hitLayers - shooterLayerValue;

        Bounds b = box.bounds;
        // cast that box forward along moveDirection
        RaycastHit2D hit = Physics2D.BoxCast(
            b.center,
            b.size,
            objBody.rotation,             // angle of the box in degrees
            moveDirection,
            moveDistance + collisionBuffer,
            finalHitLayers.value);

        if (hit.collider != null)
        {
            HandleHit(hit.collider);
            Destroy(gameObject);
            return;
        }

        // Move and rotate via Rigidbody2D
        Vector2 nextPos = objBody.position + moveDirection * moveDistance;
        objBody.MovePosition(nextPos);
        float nextRot = objBody.rotation + rotationSpeed * Time.fixedDeltaTime;
        objBody.MoveRotation(nextRot);

        if (IsOutsideCameraView(nextPos))
        {
            Debug.Log("Outside of viewable area, despawning object");
            Destroy(gameObject);
        }
    }

    private bool IsOutsideCameraView(Vector2 position)
    {
        Vector2 viewportPos = playerCamera.WorldToViewportPoint(position);
        return viewportPos.x < 0 || viewportPos.x > 1 ||
               viewportPos.y < 0 || viewportPos.y > 1;
    }

    private void HandleHit(Collider2D collider)
    {
        switch (collider.gameObject.layer)
        {
            case (int)Utils.Enums.ObjectLayers.Wall:
                Debug.Log("Throwed Hit Wall");
                break;
            case (int)Utils.Enums.ObjectLayers.Enemy:
                Debug.Log("Throwed Hit Enemy");
                if (collider.transform.parent.GetComponentInChildren<IEnemy>()
                        is IEnemy enemy && !enemy.IsEnemyDead())
                {
                    enemy.SetIsEnemyDead(true);
                    logic.AddEnemyKilledPoints(enemy as IPoints);
                }
                break;
        }
    }
}
