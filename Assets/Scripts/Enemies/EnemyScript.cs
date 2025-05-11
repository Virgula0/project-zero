using UnityEngine;

// updates sprites based on the movement
public class EnemyScript : MonoBehaviour
{
    private Rigidbody2D body;
    private Vector2 lastPosition;
    private SpriteRenderer sprite;
    private Rigidbody2D playerBody;
    private IEnemy enemyScript;
    private bool canSeePlayer = false;
    private EnemyWeaponManager weaponManagerRef;

    void Start()
    {   
        sprite = GetComponentInChildren<SpriteRenderer>();
        body = GetComponentInChildren<Rigidbody2D>();
        playerBody = GameObject.FindGameObjectWithTag(Utils.Const.PLAYER_TAG).GetComponentInChildren<Rigidbody2D>();
        enemyScript = gameObject.GetComponentInChildren<IEnemy>();
        weaponManagerRef = GetComponentInChildren<EnemyWeaponManager>();
        lastPosition = body.position;
    }

    void FixedUpdate()
    {

        if (enemyScript.IsEnemyDead() || enemyScript.IsStunned())
        {
            return;
        }

        Vector2 currentPosition = body.position;
        Vector2 movement = currentPosition - lastPosition;

        bool followPlayer = false;
        if (enemyScript.GetCurrentMovement() is ChaseMovement && weaponManagerRef.GetCurrentLoadedWeapon() is IRanged)
        {
            movement = playerBody.position - currentPosition;
            followPlayer = true;
        }

        if (followPlayer || movement.sqrMagnitude > 0.001f)
        {
            float angle = Mathf.Atan2(movement.y, movement.x) * Mathf.Rad2Deg;
            sprite.transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        lastPosition = currentPosition;
    }

    public void SetCanSeePlayer(bool newBool){
        canSeePlayer = newBool;
    }
}
