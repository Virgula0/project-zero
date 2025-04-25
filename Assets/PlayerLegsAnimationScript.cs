using UnityEngine;

public class PlayerLegsAnimationScript : MonoBehaviour
{
    private Animator legsAnim;
    private Transform legsTransf;

    void Start()
    {
        this.legsAnim = gameObject.GetComponent<Animator>();
        this.legsTransf = gameObject.transform;   
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;

        if (moveInput != Vector2.zero)
        {
            float angle = Mathf.Atan2(moveInput.y, moveInput.x) * Mathf.Rad2Deg;
            legsTransf.rotation = Quaternion.Euler(0f, 0f, angle); // Adjust based on your art
            legsAnim.SetBool("isWalking", true);
        }
        else
        {
            legsAnim.SetBool("isWalking", false);
        }
    }
}
