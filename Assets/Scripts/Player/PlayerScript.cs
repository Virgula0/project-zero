using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    public float moveSpeed = 30f;
    private Rigidbody2D playerRb;
    private SpriteRenderer playerSprite;
    private IDash dash;
    private bool dashRequested = false; // flag for input

    void Start()
    {
        this.playerSprite = GetComponentInChildren<SpriteRenderer>();
        this.playerRb = GetComponent<Rigidbody2D>();
        this.dash = GetComponent<IDash>().New(this.playerRb);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            dashRequested = true;
        }
    }

    void FixedUpdate()
    {
        LookAt();
        Vector2 moveDirection = ProcessKeyboardInputs();

        bool dashWasPerformed = dash.Dash(moveDirection, dashRequested);

        if (dashWasPerformed || dash.IsDashing)
        {
            return;
        }
        
        dashRequested = false;
        Movement(moveDirection);
    }

    private void Movement(Vector2 moveDirection)
    {
        // Remove Time.fixedDeltaTime for correct velocity units
        playerRb.linearVelocity = moveDirection * moveSpeed;
    }

    private Vector2 ProcessKeyboardInputs()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        return new Vector2(moveX, moveY).normalized;
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
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0f, 0f, angle), Time.fixedDeltaTime * rotationSpeed);
    }
}
