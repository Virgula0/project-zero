using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    public float moveSpeed = 20f;
    public Rigidbody2D playerRb;
    private Vector2 moveDirection;
    private SpriteRenderer playerSprite;
    
    private Vector2 lastMoveDirection = Vector2.right; // Default direction
    private DashScript dash;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //add something later
        playerSprite = GetComponentInChildren<SpriteRenderer>();
        playerRb = gameObject.GetComponent<Rigidbody2D>();
        this.dash = gameObject.GetComponent<DashScript>();
    }

    void Update(){
        if(Input.GetKeyDown(KeyCode.Space)){
            dash.StartDash();
        }
    }

    void FixedUpdate()
    {
        //best suited for physics operations, using along with interpolation as interpolate
        LookAt();
        dash.DashMovement();
        if(dash.IsDashing()){
            return;
        }
        ProcessInputs();
        Movement();
    }

    private void ProcessInputs(){
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        Vector2 input = new Vector2(moveX, moveY);
        moveDirection = input.normalized;

        if (input != Vector2.zero){ //save the last movement direction
            lastMoveDirection = moveDirection;
        }
    }

    private void Movement(){
        playerRb.linearVelocity = new Vector2(moveDirection.x * moveSpeed , moveDirection.y * moveSpeed );
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

    public Vector2 GetDirection(){
        if(moveDirection == Vector2.zero){
            return lastMoveDirection;
        }
        return moveDirection;
    }

    public Vector2 GetLinearVelocity(){
        return playerRb.linearVelocity;
    }

    public void SetLinearVelocity(Vector2 newLinearVelocity){
        playerRb.linearVelocity = newLinearVelocity;
    }
}
