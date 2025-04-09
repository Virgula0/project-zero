using UnityEngine;

public class PlayerDetector : MonoBehaviour
{
    [Header("Circle Cast Settings")]
    [SerializeField] private float circleRadius = 8f;          // The radius of the circle used for the cast.
    [SerializeField] private float circleDistance = 0f;        // How far the circle cast travels.
    [SerializeField] private LayerMask physicalLayer;          // Layer mask for detection.
    [SerializeField] private Vector2 castDirection = new(0, 0); // Direction for the cast.
    private bool isEnemyAwareOfPlayer = false;
    private float elapsedLatestDetectionOfPlayerInSeconds = 0f;

    void Start()
    {
        transform.position = transform.parent.position;
    }

    void FixedUpdate()
    {        

        if (elapsedLatestDetectionOfPlayerInSeconds > 2f) // after 2 seconds we reset the player alert status
        {
            isEnemyAwareOfPlayer = false;
        }

        // Perform the CircleCast using Physics2D
        RaycastHit2D hitInfo = Physics2D.CircleCast(transform.parent.position, circleRadius, castDirection, circleDistance, physicalLayer);
        if (hitInfo.collider != null)
        {
            switch (hitInfo.collider.gameObject.layer)
            {
                case (int)Utils.Enums.ObjectLayers.Player:
                    Debug.Log("OBJECT DETECTED BY ENEMY: " + hitInfo.collider.gameObject.name);
                    isEnemyAwareOfPlayer = true;
                    elapsedLatestDetectionOfPlayerInSeconds = 0f;
                    break;
            }
        }

        elapsedLatestDetectionOfPlayerInSeconds += Time.fixedDeltaTime;
    }

    public bool GetIsEnemyAwareOfPlayer(){
        return isEnemyAwareOfPlayer;
    }

    // Visualize the circle cast in the Scene view (even without playing the game)
    private void OnDrawGizmos()
    {
        // Calculate the starting point and ending point of the cast
        Vector3 startPoint = transform.parent.position;
        Vector3 endPoint = startPoint + (Vector3)(castDirection.normalized * circleDistance);

        // Set Gizmo color for visualization
        Gizmos.color = Color.red;

        // Draw the starting circle
        Gizmos.DrawWireSphere(startPoint, circleRadius);
        // Draw the ending circle
        Gizmos.DrawWireSphere(endPoint, circleRadius);

        // Draw a line connecting the two circles to illustrate the cast path
        Gizmos.DrawLine(startPoint, endPoint);
    }
}
