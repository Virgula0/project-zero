using UnityEngine;

public class BulletScript : MonoBehaviour
{
    private Camera playerCamera; // Used for getting coordinates and calculating the de-spawn point
    private const float speed = 150f;
    private Vector2 moveDirection; // Stores the initial direction toward the mouse when shot

    void Start()
    {
        playerCamera = Camera.main; // THIS WORKS IF THE MAIN CAMERA IS THE ONE THAT FOLLOWS THE PLAYER
        Vector2 mousePosition = playerCamera.ScreenToWorldPoint(Input.mousePosition);
        moveDirection = (mousePosition - (Vector2)transform.position).normalized;
    }

    // FixedUpdate is better for physics-based movement
    void FixedUpdate()
    {
        if (IsOutsideCameraView())
        {
            Debug.Log("OUTSIDE CAMERA VIEW, BULLET DESTROYED");
            Destroy(gameObject);
        }
        else
        {
            transform.position = MoveBullet();
        }
    }

    private bool IsOutsideCameraView()
    {
        // Convert the object's world position to viewport coordinates.
        Vector2 viewportPos = playerCamera.WorldToViewportPoint(transform.position);
        // The viewport x and y coordinates should be between 0 and 1 when visible.
        bool offScreen = viewportPos.x < 0 || viewportPos.x > 1 ||
                         viewportPos.y < 0 || viewportPos.y > 1;
        return offScreen;
    }

    private Vector2 MoveBullet()
    {
        // Move the bullet in the stored direction
        return (Vector2)transform.position + speed * Time.deltaTime * moveDirection;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == (int)Utils.Enums.ObjectLayers.Player)
        {
            Debug.Log("BULLET GOT THE PLAYER");
            // Optionally, add additional behavior here before destroying the bullet
        }
        Debug.Log("BULLET DESTROYED");
        Destroy(gameObject); // Destroy the bullet when it collides with any object
    }
}
