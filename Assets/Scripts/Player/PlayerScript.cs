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
    private float dashCooldownTimer = 0;
    private Vector2 dashDirection;
    private AudioSource audioSrc;

    [SerializeField] float startDashTime = 0.07f;
    [SerializeField] float dashSpeed = 50f;
    [SerializeField] float dashCooldown = 3f;
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

    void Update(){
        if (!isDashing && canDash && Input.GetKeyDown(KeyCode.Space)){
            ProcessInputs(); //get dash direction
            if (moveDirection == Vector2.zero){ //if the direction is 0 use the last non 0 direction
                dashDirection = lastMoveDirection;
            }else{
                dashDirection = moveDirection;
            }

            isDashing = true;
            dashTimer = startDashTime;
            canDash = false;
            canMove = false;
        }

        if (!canDash){
            dashCooldownTimer += Time.deltaTime;
            if (dashCooldownTimer > dashCooldown){
                canDash = true;
                dashCooldownTimer = 0f;
                audioSrc.PlayOneShot(dashCooldownAlert);
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
            return; //skip normal movement during dash
        }

        if(!canMove){
            return; //you can't move while dashing
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
            playerRb.linearVelocity = Vector2.zero; //stop dashing
        }
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
