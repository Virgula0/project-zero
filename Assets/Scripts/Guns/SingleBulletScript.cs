using UnityEngine;

public class SingleBulletScript : MonoBehaviour
{
    private Camera playerCamera;         // Reference to main camera for viewport checks
    private const float speed = 150f;    // Bullet travel speed in units per second
    private Vector2 moveDirection;       // Normalized direction vector for bullet movement
    
    [SerializeField] private LayerMask hitLayers; // Layers that can be hit by the bullet
    [SerializeField] private float collisionBuffer = 0.1f; // Small distance buffer to prevent edge-case misses
    private LayerMask shooterLayer; // Layer of the entity that fired this bullet

    // Initializes the bullet with the shooter's layer to prevent self-collision
    public void Initialize(LayerMask shooterLayer)
    {
        this.shooterLayer = shooterLayer;
    }

    void Start()
    {
        // Camera reference for viewport calculations
        playerCamera = Camera.main;
        // Calculate initial direction towards mouse position
        Vector2 mousePosition = playerCamera.ScreenToWorldPoint(Input.mousePosition);
        moveDirection = (mousePosition - (Vector2)transform.position).normalized;
    }

    void FixedUpdate()
    {
        float moveDistance = speed * Time.fixedDeltaTime;
        
        // Create collision mask that ignores the shooter's layer
            int shooterLayerValue = (int)Mathf.Pow(2, shooterLayer);
            LayerMask finalHitLayers = hitLayers - shooterLayerValue; // exclude the player layer

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
            Debug.Log("outside of viewable area, despawning bullet");
            Destroy(gameObject);
        }
    }

    /// Checks if bullet has left the camera's viewable area
    private bool IsOutsideCameraView()
    {
        Vector2 viewportPos = playerCamera.WorldToViewportPoint(transform.position);
        // Returns true if position is outside [0,1] range in either axis
        return viewportPos.x < 0 || viewportPos.x > 1 || viewportPos.y < 0 || viewportPos.y > 1;
    }

    /// Handles collision effects and logging
    private void HandleHit(Collider2D collider)
    {
        if (collider.gameObject.layer == (int)Utils.Enums.ObjectLayers.Player)
        {
            Debug.Log("Hit Player");
        }
        else if (collider.gameObject.layer == (int)Utils.Enums.ObjectLayers.Wall)
        {
            Debug.Log("Hit Wall");
        }
        else if (collider.gameObject.layer == (int)Utils.Enums.ObjectLayers.Enemy)
        {
            Debug.Log("Hit Enemy");
        }
    }
}