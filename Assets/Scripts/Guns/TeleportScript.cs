using System;
using System.Collections;
using UnityEngine;

public class TeleportScript : MonoBehaviour
{
    private GameObject legsObj;
    private bool isBusy = false;

    public void Initialize(GameObject legsObj)
    {
        this.legsObj = legsObj;
    }

    public bool Run(Animator playerAnimatorRef)
    {
        if (isBusy)
        {
            return false;
        }

        StartCoroutine(WaitForAnimation(playerAnimatorRef, Utils.Animations.TELEPORT_ANIMATION));
        return true;
    }
    
    private IEnumerator WaitForAnimation(Animator animator, string stateName)
    {
        isBusy = true;
        // Wait until the animator enters the state
        while (!animator.GetCurrentAnimatorStateInfo(0).IsName(stateName))
        {
            yield return null;
        }

        // Wait until the animation finishes
        while (animator.GetCurrentAnimatorStateInfo(0).IsName(stateName) &&
               animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
        {
            yield return null;
        }

        legsObj.SetActive(true);
        isBusy = false;
    }
}