using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    public float moveSpeed = 1500f;
    public Rigidbody2D playerRb;
    private Vector2 moveDirection;
    private SpriteRenderer playerSprite;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //add something later
        playerSprite = GetComponentInChildren<SpriteRenderer>();
    }

    void FixedUpdate()
    {
        //best suited for physics operations, using along with interpolation as interpolate
        LookAt();
        ProcessInputs();
        Movement();
    }

    private void ProcessInputs(){
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        moveDirection = new Vector2(moveX, moveY).normalized;
    }

    private void Movement(){
        playerRb.linearVelocity = new Vector2(moveDirection.x * moveSpeed * Time.deltaTime, moveDirection.y * moveSpeed * Time.deltaTime);
    }

    private void LookAt(){
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 playerMouseDistance = mousePos - playerRb.position;

        // Get half the width or height of the sprite (whichever is larger)
        float safeDistance = Mathf.Max(playerSprite.bounds.extents.x, playerSprite.bounds.extents.y);

        if (playerMouseDistance.magnitude < safeDistance)
        {
            return; // Stop execution if the mouse is too close
        }

        // Calculate the direction from the player/gun to the mouse
        Vector2 direction = (mousePos - (Vector2)transform.position).normalized;

        // Convert direction vector to angle (in degrees)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        if (angle > 90 || angle < -90)
            transform.localScale = new Vector2(2, -2); // Flip sprite
        else
            transform.localScale = new Vector2(2, 2); // Normal sprite

        // Apply rotation (z-axis in 2D)
        float rotationSpeed = 10f;
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0f, 0f, angle), Time.deltaTime * rotationSpeed);
    }
}
