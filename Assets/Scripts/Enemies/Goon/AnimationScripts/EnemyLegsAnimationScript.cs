using UnityEngine;

public class EnemyLegsAnimationScript : MonoBehaviour
{
    private Transform legsTransform;
    private Animator legAnimator;
    private Vector2 lastPosition;

    [SerializeField] private Transform enemyTransform;

    void Start()
    {
        legAnimator = gameObject.GetComponent<Animator>();
        legsTransform = gameObject.transform;
        lastPosition = enemyTransform.position;
    }

    void Update()
    {
        Vector2 currentPosition = enemyTransform.position;
        Vector2 moveDir = (currentPosition - lastPosition).normalized;

        if (moveDir != Vector2.zero)
        {
            float angle = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg;
            legsTransform.rotation = Quaternion.Euler(0f, 0f, angle);
            legAnimator.SetBool(Utils.Const.WALKING_ANIM_VAR, true);
        }
        else
        {
            legAnimator.SetBool(Utils.Const.WALKING_ANIM_VAR, false);
        }

        lastPosition = currentPosition;
    }
}

