using UnityEngine;

public class SingleBulletScript : MonoBehaviour
{
    private Camera playerCamera;         // Reference to main camera for viewport checks
    private const float speed = 120f;    // Bullet travel speed in units per second
    private Vector2 moveDirection;       // Normalized direction vector for bullet movement
    private bool isPlayer = false;
    private LogicManager logic;
    private IPrimary gunRef;

    [SerializeField] private LayerMask hitLayers; // Layers that can be hit by the bullet
    [SerializeField] private float collisionBuffer = 0.1f; // Small distance buffer to prevent edge-case misses

    public void Initialize(GameObject player, IPrimary gun)
    {
        if (player.layer == (int)Utils.Enums.ObjectLayers.Player)
        {
            gameObject.layer = (int)Utils.Enums.ObjectLayers.BulletByPlayer; // set a layer to be detected by enemies but we need to do this because it will detect only if it's by the player
            isPlayer = true;
        }
        gunRef = gun;
    }

    void Start()
    {
        // Camera reference for viewport calculations
        playerCamera = Camera.main; // needed for both player and UI
        this.logic = GameObject.FindGameObjectWithTag(Utils.Const.LOGIC_MANAGER_TAG).GetComponent<LogicManager>();

        // If is the player that is shooting
        if (isPlayer)
        {
            // Calculate initial direction towards mouse position
            Vector2 mousePosition = playerCamera.ScreenToWorldPoint(Input.mousePosition);
            moveDirection = (mousePosition - (Vector2)transform.position).normalized;
            //transform.right = moveDirection;
            gameObject.GetComponent<Rigidbody2D>().transform.right = moveDirection;
            return;
        }

        Debug.Log("Enemy is shooting!!");
        // otherwise move direction is calculated around the player position
        GameObject pp = GameObject.FindGameObjectWithTag(Utils.Const.PLAYER_TAG);
        moveDirection = ((Vector2)pp.transform.position - (Vector2)transform.position).normalized;
        gameObject.GetComponent<Rigidbody2D>().transform.right = moveDirection;
    }

    void FixedUpdate()
    {
        float moveDistance = speed * Time.fixedDeltaTime;
        LayerMask finalHitLayers;

        // If it is the player that is shooting create collision mask that ignores the shooter's layer
        if (isPlayer)
        {
            int shooterLayerValue = (int)Mathf.Pow(2, (int)Utils.Enums.ObjectLayers.Player);
            finalHitLayers = hitLayers - shooterLayerValue; // exclude the player layer
        }
        else
        {
            int shooterLayerValue = (int)Mathf.Pow(2, (int)Utils.Enums.ObjectLayers.Enemy);
            finalHitLayers = hitLayers - shooterLayerValue; // exclude the enemy layer
        }

        // Cast a ray ahead of the bullet's path
        RaycastHit2D hit = Physics2D.Raycast(transform.position, moveDirection, moveDistance + collisionBuffer, finalHitLayers.value, -Mathf.Infinity, Mathf.Infinity);

        // Handle collision detection
        if (hit.collider != null)
        {
            HandleHit(hit.collider);
            transform.position = hit.point;  // Move bullet to exact impact point
            Destroy(gameObject);             // Remove bullet from game
            return;
        }

        // Move bullet forward if no collision detected
        transform.position += (Vector3)(moveDirection * moveDistance);

        // Destroy bullet if it exits camera view
        if (IsOutsideCameraView())
        {
            Debug.Log("Outside of viewable area, despawning bullet");
            Destroy(gameObject);
        }
    }

    /// Checks if bullet has left the camera's viewable area
    private bool IsOutsideCameraView()
    {
        Vector2 viewportPos = playerCamera.WorldToViewportPoint(transform.position);
        if (isPlayer)
        {
            return viewportPos.x < 0 || viewportPos.x > 1 || viewportPos.y < 0 || viewportPos.y > 1;
        }

        // if the bullet is far away for enemies because they may want to shoot the player even when outside the 
        // camera
        return (transform.position.x < -100 && viewportPos.x < 0)
                || (transform.position.x > 100 && viewportPos.x > 1)
                || (transform.position.y < -100 && viewportPos.y < 0)
                || (transform.position.y > 100 && viewportPos.y > 1);
    }

    /// Handles collision effects and logging
    private void HandleHit(Collider2D collider)
    {
        switch (collider.gameObject.layer)
        {
            case (int)Utils.Enums.ObjectLayers.Player:
                Debug.Log("Hit Player");
                this.logic.GameOver(gunRef); // TODO change the approach so we can add more weapon varaiety
                break;
            case (int)Utils.Enums.ObjectLayers.Wall:
                Debug.Log("Hit Wall");
                break;
            case (int)Utils.Enums.ObjectLayers.Enemy:
                Debug.Log("Hit Enemy");
                if (collider.transform.parent.GetComponentInChildren<IEnemy>() is IEnemy enemy)
                {
                    if (!enemy.IsEnemyDead()) // avoid to do actions on already died enemy
                    {
                        // TODO: change the enemy sprite to a dead one, update IEnemy interface
                        enemy.SetIsEnemyDead(true);
                        logic.AddEnemyKilledPoints(enemy as IPoints);
                    }
                }
                break;
        }
    }
}