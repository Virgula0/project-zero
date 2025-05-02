using System;
using System.Collections;
using UnityEngine;

public class TeleportScript : MonoBehaviour
{
    private GameObject legsObj;

    public void Initialize(GameObject legsObj)
    {
        this.legsObj = legsObj;
    }

    public void Run(Animator playerAnimatorRef)
    {
        StartCoroutine(WaitForAnimation(playerAnimatorRef, Utils.Animations.TELEPORT_ANIMATION));
    }

    private IEnumerator WaitForAnimation(Animator animator, string stateName)
    {
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
    }
}