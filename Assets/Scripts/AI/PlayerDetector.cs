using UnityEngine;

public class PlayerDetector : MonoBehaviour
{
    [Header("Circle Cast Settings")]
    [SerializeField] private float circleRadius = 8f;          // The radius of the circle used for the cast.
    [SerializeField] private float circleDistance = 0f;        // How far the circle cast travels.
    [SerializeField] private LayerMask physicalLayer;          // Layer mask for detection.
    [SerializeField] private Vector2 castDirection = new(0,0); // Direction for the cast.


    void Start()
    {
        transform.position = transform.parent.position;   
    }

    void FixedUpdate()
    {
        // Perform the CircleCast using Physics2D
        RaycastHit2D hitInfo = Physics2D.CircleCast(transform.parent.position, circleRadius, castDirection, circleDistance, physicalLayer);
        if (hitInfo.collider != null)
        {
            // Debug.Log("ENEMY DETECTED: " + hitInfo.collider.gameObject.name);
        }else{
            // Debug.Log("nothing detected");
        }
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
