using UnityEngine;
public class Detector : MonoBehaviour
{
    [Header("Circle Cast Settings")]
    [SerializeField] private float circleRadius = 8f;          // The radius of the circle used for the cast
    [SerializeField] private LayerMask physicalLayer;          // Layer mask for detection (Player,Enemies etc..)

    [Header("Line of Sight Settings")]
    [SerializeField] private LayerMask obstacleLayer;          // Layer mask for obstacles (walls, tables, and so on..)
    [SerializeField] private float lineOfSightOffset = 0f;     // Offset for the raycast start (e.g., adjust for "eye-level")

    private bool isEnemyAwareOfPlayer = false;
    private float elapsedLatestDetectionOfPlayerInSeconds = 0f;
    private float alertedEnemySeconds = 5f; // Seconds in which the enemy will remain in alert status after player detection

    private Rigidbody2D body;
    private bool playerWasHiddenByObstacle = false;

    void Start()
    {
        this.body = transform.parent.GetComponentInChildren<Rigidbody2D>();
    }

    void Update()
    {
        // After N seconds without detection, reset the alert status
        if (elapsedLatestDetectionOfPlayerInSeconds > alertedEnemySeconds)
        {
            isEnemyAwareOfPlayer = false;
            playerWasHiddenByObstacle = false;
        }

        // Check for all colliders within the detection circle
        // CircleCast does not work becuase it does not work for stationary objects.
        // CircleCast is the equivalent of ShpereCast for 2D
        // For this reason we use OverlaCircleAll here which seems to works better and circumnvent the problem
        Collider2D[] hits = Physics2D.OverlapCircleAll(body.position, circleRadius, physicalLayer);
        bool detectedPlayer = false;

        foreach (Collider2D hitCollider in hits)
        {
            if (hitCollider.gameObject.layer == (int)Utils.Enums.ObjectLayers.Player)
            {
                // Set the base positions for enemy and player.
                Vector2 enemyPos = body.position + Vector2.up * lineOfSightOffset;
                Vector2 playerPos = hitCollider.transform.position;
                Vector2 directionToPlayer = (playerPos - enemyPos).normalized;
                float distanceToPlayer = Vector2.Distance(enemyPos, playerPos);

                // Perform a raycast between enemy and player to check for obstacles in the betwen
                RaycastHit2D sightHit = Physics2D.Raycast(enemyPos, directionToPlayer, distanceToPlayer, obstacleLayer);

                // If no obstacle is hit, then the enemy has a clear line of sight
                switch (sightHit.collider)
                {
                    case not null:
                        // Debug.Log("Player is hidden by an obstacle: " + sightHit.collider.gameObject.name);
                        playerWasHiddenByObstacle = true;
                        break;
                    case null:
                        // Debug.Log("OBJECT DETECTED BY ENEMY: " + hitCollider.gameObject.name);
                        isEnemyAwareOfPlayer = true;
                        playerWasHiddenByObstacle = false;
                        elapsedLatestDetectionOfPlayerInSeconds = 0f;
                        detectedPlayer = true;
                        break;
                }
            }
        }

        if (!detectedPlayer) // optimization, this may be not needed
        {
            elapsedLatestDetectionOfPlayerInSeconds += Time.deltaTime;
            return;
        }
    }

    public bool GetIsEnemyAwareOfPlayer()
    {
        return isEnemyAwareOfPlayer;
    }

    public bool GetIsPlayerHiddenByObstacle(){
        return playerWasHiddenByObstacle;
    }

    // Visualize the detection circle and line of sight in the Scene view.
    // use transform.position to set up in the editor too
    private void OnDrawGizmos()
    {
        if (transform.position == null)
            return;
        
        Gizmos.color = Color.red;

        if (body != null)
            // Draw detection circle
            Gizmos.DrawWireSphere(body.position, circleRadius);
        else 
            Gizmos.DrawWireSphere(transform.position, circleRadius);

        // Draw line of sight if player is detected
        if (isEnemyAwareOfPlayer)
        {
            Gizmos.color = Color.green;
            // Simplified line visualization (remove old castDirection/circleDistance logic)
            Gizmos.DrawLine(
                transform.position + Vector3.up * lineOfSightOffset,
                transform.position + Vector3.up * lineOfSightOffset + Vector3.right * circleRadius // Example direction
            );
        }
    }
}