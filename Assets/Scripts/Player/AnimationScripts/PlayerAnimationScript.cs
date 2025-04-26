using UnityEngine;

public class PlayerAnimationScript : MonoBehaviour
{
    public void OnAttackAnimationEnd()
    {
        // Disable animator
        gameObject.GetComponentInChildren<Animator>().enabled = false;
    }
}
