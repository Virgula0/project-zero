using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem.iOS;

public class PlayerScript : MonoBehaviour
{
    public float moveSpeed;
    public Rigidbody2D playerRb;

    private Vector2 moveDirection;
    private Vector2 mousePos;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //add something later
    }

    // Update is called once per frame
    void Update()
    {
        LookAt();
        ProcessInputs();
        Movement();
    }

    void FixedUpdate()
    {
        //best suited for physics operations
    }

    private void ProcessInputs(){
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        moveDirection = new Vector2(moveX, moveY).normalized;
    }

    private void Movement(){
        float time = Time.deltaTime;
        playerRb.linearVelocity = new Vector2(moveDirection.x * moveSpeed * time, moveDirection.y * moveSpeed * time);
    }

    private void LookAt(){
        mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.right = mousePos - new Vector2(transform.position.x, transform.position.y);
        
        /*var dir = Input.mousePosition - Camera.main.ScreenToWorldPoint(transform.position);
        var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);*/
    }
}
