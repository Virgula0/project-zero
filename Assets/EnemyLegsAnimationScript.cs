using UnityEngine;

public class EnemyLegsAnimationScript : MonoBehaviour
{
    private Transform legsTransform;
    private Animator legAnimator;

    private Vector2 lastPosition;

    void Start()
    {
        legAnimator = gameObject.GetComponent<Animator>();
        legsTransform = gameObject.transform;
        lastPosition = transform.position;
        legAnimator.Play("EnemyLegsWalkingAnimation");
    }

    void Update()
    {
        Vector2 currentPosition = transform.position;
        Vector2 moveDir = (currentPosition - lastPosition).normalized;

        if (moveDir != Vector2.zero)
        {
            float angle = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg;
            legsTransform.rotation = Quaternion.Euler(0f, 0f, angle);
            legAnimator.SetBool("isWalking", true);
        }
        else
        {
            legAnimator.SetBool("isWalking", false);
        }

        lastPosition = currentPosition;
    }
}

