using UnityEngine;

public class PlayerLegsAnimationScript : MonoBehaviour
{
    private Animator legsAnim;
    private Transform legsTransf;
    private PlayerScript playerScriptRef;
    private bool isTeleporting = false;

    void Start()
    {
        this.legsAnim = gameObject.GetComponent<Animator>();
        this.legsTransf = gameObject.transform;
        this.playerScriptRef = GetComponentInParent<PlayerScript>();
    }

    // Update is called once per frame
    void Update()
    {
        if(isTeleporting){
            legsAnim.SetBool(Utils.Const.WALKING_ANIM_VAR, false);
            return;
        }
        if (!playerScriptRef.IsPlayerAlive()) //deactivate the animator and legs sprite if the player is dead
        {
            if(gameObject.GetComponent<Animator>().enabled == true && gameObject.GetComponent<SpriteRenderer>().enabled == true){
                gameObject.GetComponent<Animator>().enabled = false;
                gameObject.GetComponent<SpriteRenderer>().enabled = false;
            }
            return;
        }
        Vector2 moveInput = new Vector2(Input.GetAxisRaw(Utils.Const.HORIZONTAL), Input.GetAxisRaw(Utils.Const.VERTICAL)).normalized;

        

        if (moveInput != Vector2.zero)
        {
            float angle = Mathf.Atan2(moveInput.y, moveInput.x) * Mathf.Rad2Deg;
            legsTransf.rotation = Quaternion.Euler(0f, 0f, angle); // Adjust based on your art
            legsAnim.SetBool(Utils.Const.WALKING_ANIM_VAR, true);
        }
        else
        {   
            legsAnim.SetBool(Utils.Const.WALKING_ANIM_VAR, false);
        }
    }

    public void SetIsTeleporting(bool boolVar){
        this.isTeleporting = boolVar;
    }
}
