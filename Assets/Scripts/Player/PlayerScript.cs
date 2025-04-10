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
    private bool isDashing = false;
    private float dashTimer = 0f;
    private Vector2 dashDirection;
    private AudioSource audioSrc;

    [SerializeField] float startDashTime = 0.07f;
    [SerializeField] float dashSpeed = 50f;
    [SerializeField] float dashCooldown = 0f;
    [SerializeField] AudioClip dashSound;
    [SerializeField] AudioClip dashCooldownAlert;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //add something later
        playerSprite = GetComponentInChildren<SpriteRenderer>();
        this.canDash = true;
        this.canMove = true;
        this.audioSrc = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (!isDashing && canDash && Input.GetKeyDown(KeyCode.Space))
    {
        ProcessInputs(); // Get the current movement direction
        if (moveDirection != Vector2.zero)
            dashDirection = moveDirection;
        else
            dashDirection = lastMoveDirection;

        isDashing = true;
        dashTimer = startDashTime;
        canDash = false;
        canMove = false;
    }

    if (!canDash)
    {
        dashCooldown += Time.deltaTime;
        if (dashCooldown > 3f)
        {
            canDash = true;
            dashCooldown = 0f;
            audioSrc.PlayOneShot(dashCooldownAlert);
            Debug.Log("YOU CAN DASH AGAIN");
        }
    }
    }

    void FixedUpdate()
    {
        //best suited for physics operations, using along with interpolation as interpolate
        LookAt();
        if (isDashing)
        {
            DashMovement();
            return; // Skip normal movement during dash
        }

        if(!canMove){
            return;
        }
        ProcessInputs();
        Movement();
    }

    private void DashMovement()
    {
        if (dashTimer > 0)
        {
            dashTimer -= Time.fixedDeltaTime;
            playerRb.linearVelocity = dashDirection * dashSpeed;
        }
        else
        {
            audioSrc.PlayOneShot(dashSound);
            isDashing = false;
            canMove = true;
            playerRb.linearVelocity = Vector2.zero;
        }
    }

    /*IEnumerator Dash(Vector2 direction){
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

        canMove = true;
    }*/

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
