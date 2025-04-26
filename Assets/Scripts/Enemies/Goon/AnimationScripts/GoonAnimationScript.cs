using UnityEngine;

public class GoonAnimationScript : MonoBehaviour
{
    public void OnAttackAnimationEnd()
    {
        // Disable animator
        gameObject.GetComponentInChildren<Animator>().enabled = false;
    }
}
