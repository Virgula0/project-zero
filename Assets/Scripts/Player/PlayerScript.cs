using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    public float moveSpeed = 1500f;
    public Rigidbody2D playerRb;
    private Vector2 moveDirection;
    private SpriteRenderer playerSprite;
    private bool canDash;
    private bool canMove;
    private float currentDashTime;
    private Vector2 lastMoveDirection = Vector2.right; // Default direction


    [SerializeField] float startDashTime = 0.1f;
    [SerializeField] float dashSpeed = 100f;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //add something later
        playerSprite = GetComponentInChildren<SpriteRenderer>();
        this.canDash = true;
        this.canMove = true;
    }

    void Update()
    {
        if (canDash && Input.GetKeyDown(KeyCode.Space))
        {
            canDash = false;
            canMove = false;
            ProcessInputs();
            StartCoroutine(Dash(moveDirection));
        }
    }

    void FixedUpdate()
    {
        //best suited for physics operations, using along with interpolation as interpolate
        LookAt();
        if(!canMove){
            return;
        }
        ProcessInputs();
        Movement();
    }

    IEnumerator Dash(Vector2 direction){
        currentDashTime = startDashTime; // Reset the dash timer.

        Debug.Log("Dashing");
        Debug.Log(direction);

        while (currentDashTime > 0f)
        {
            currentDashTime -= Time.deltaTime; // Lower the dash timer each frame.

            playerRb.linearVelocity = direction * dashSpeed; // Dash in the direction that was held down.
            // No need to multiply by Time.DeltaTime here, physics are already consistent across different FPS.

            yield return null; // Returns out of the coroutine this frame so we don't hit an infinite loop.
        }

        playerRb.linearVelocity = new Vector2(0f, 0f); // Stop dashing.

        canDash = true;
        Debug.Log("YOU CAN DASH AGAIN");
        canMove = true;
    }

    private void ProcessInputs(){
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        Vector2 input = new Vector2(moveX, moveY);
        moveDirection = input.normalized;

        if (input != Vector2.zero)
        {
            lastMoveDirection = moveDirection;
        }
    }

    private void Movement(){
        playerRb.linearVelocity = new Vector2(moveDirection.x * moveSpeed * Time.fixedDeltaTime , moveDirection.y * moveSpeed * Time.fixedDeltaTime); // in fixed updates you may want to use fixed deltaTime instead of deltatime
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
