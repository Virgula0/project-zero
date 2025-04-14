using UnityEngine;

public class DashScript : MonoBehaviour
{   
    private float dashCooldownTimer = 0f;
    private AudioSource audioSrc;
    private Vector2 dashDirection;
    private bool isDashing = false;
    private bool canDash = true;
    private float dashTimer = 0f;
    private PlayerScript playerSc;
    private Rigidbody2D playerRb;

    [SerializeField] float startDashTime = 0.07f;
    [SerializeField] float dashSpeed = 50f;
    [SerializeField] AudioClip dashSound;
    [SerializeField] float dashCooldown = 3f;
    [SerializeField] AudioClip dashCooldownAlert;

    public void Start()
    {
        this.playerSc = gameObject.GetComponent<PlayerScript>();
        this.playerRb = gameObject.GetComponent<Rigidbody2D>();
        this.audioSrc = gameObject.GetComponent<AudioSource>();
    }

    public void StartDash(){
        if (!isDashing && canDash){
            SetDashDirection(playerSc.GetDirection());

            isDashing = true;
            dashTimer = startDashTime;
            canDash = false;
            dashCooldownTimer = 0f;
        }
    }

    public void DashMovement()
    {
        if (!canDash){
            dashCooldownTimer += Time.fixedDeltaTime;
            if (dashCooldownTimer > dashCooldown){
                canDash = true;
                audioSrc.PlayOneShot(dashCooldownAlert);
            }
        }
        if(isDashing){
            Debug.Log("PlayerVelocity: " + playerSc.GetLinearVelocity());
            if (dashTimer > 0)
            {
                dashTimer -= Time.fixedDeltaTime;
                playerSc.SetLinearVelocity(dashDirection * dashSpeed);
                Debug.Log("PlayerVelocity2: " + playerSc.GetLinearVelocity());
            }
            else
            {   
                Debug.Log("Second If, second branch");
                audioSrc.PlayOneShot(dashSound);
                isDashing = false;
                playerRb.linearVelocity = Vector2.zero; //stop dashing
            }
        }
    }

    public void SetDashDirection(Vector2 direction)
    {
        this.dashDirection = direction;
    }

    public bool IsDashing(){
        return isDashing;
    }
}
