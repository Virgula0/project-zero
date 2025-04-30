using System.Collections;
using UnityEngine;

public class SwordScript : MonoBehaviour
{
    [SerializeField] private float coneAngle = 160f;
    [SerializeField] private float coneRange = 5f;
    private bool isPlayer = false;
    private bool canSwing = true;
    private GameObject wielder; // wielder is the game object which contains the sprite
    private AudioClip swingSound;
    private AudioClip parrySound;
    [SerializeField] private LayerMask hitLayers;
    private float fireRate = 1f;
    private LogicManager logic;
    private LayerMask finalHitLayers;
    private GameObject player;
    private float swingTimer = 0f;
    private bool isSwinging = false;
    private float waitBeforeCallCameOver = 0.2f;
    private IGun swordRef;

    public void Initialize(GameObject wielder, AudioClip swingSound, AudioClip parrySound, IGun sword)
    {
        player = GameObject.FindGameObjectWithTag(Utils.Const.PLAYER_TAG);
        if (wielder.layer == (int)Utils.Enums.ObjectLayers.Player)
        {
            gameObject.layer = (int)Utils.Enums.ObjectLayers.SwingByPlayer;
            isPlayer = true;
        }

        this.wielder = wielder;
        this.swingSound = swingSound;
        this.parrySound = parrySound;
        this.swordRef = sword;

        int shooterLayerValue;
        if (isPlayer)
        {
            shooterLayerValue = 1 << (int)Utils.Enums.ObjectLayers.Player;
            finalHitLayers = hitLayers & ~shooterLayerValue;
            return;
        }

        shooterLayerValue = 1 << (int)Utils.Enums.ObjectLayers.Enemy;
        finalHitLayers = hitLayers & ~shooterLayerValue;
    }

    void Start()
    {
        logic = GameObject.FindGameObjectWithTag(Utils.Const.LOGIC_MANAGER_TAG)
                         .GetComponent<LogicManager>();
    }

    public bool GetCanSwing()
    {
        return canSwing;
    }

    void Update()
    {
        if (isSwinging)
        {
            swingTimer += Time.deltaTime;

            // Hit happens after 0.1s
            if (swingTimer >= 0.1f && swingTimer < 0.1f + Time.deltaTime)
            {
                PerformHit();
            }

            // Swing cooldown ends
            if (swingTimer >= 1f / fireRate)
            {
                if (!isPlayer)
                {
                    // restore original layer for enemy
                    Utils.Functions.SetLayerRecursively(wielder.transform.parent.gameObject, (int)Utils.Enums.ObjectLayers.Enemy);
                }
                canSwing = true;
                isSwinging = false;
            }
        }
    }

    public void Swing()
    {
        if (!canSwing) return;

        Vector2 dir;
        if (isPlayer)
        {
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            dir = mouseWorld - wielder.transform.position;
        }
        else
        {
            // enemy
            Utils.Functions.SetLayerRecursively(wielder.transform.parent.gameObject, (int)Utils.Enums.ObjectLayers.ParriableLayer);
            dir = player.transform.position - wielder.transform.position;
        }

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
        transform.position = wielder.transform.position;

        if (swingSound != null)
            AudioSource.PlayClipAtPoint(swingSound, wielder.transform.position);

        canSwing = false;
        isSwinging = true;
        swingTimer = 0f;
    }

    private void PerformHit()
    {
        Transform hitOrigin = transform;
        Collider2D[] hits = Physics2D.OverlapCircleAll((Vector2)hitOrigin.position, coneRange, finalHitLayers);

        for (int i = 0; i < hits.Length; i++)
        {
            Vector2 toTarget = (Vector2)hits[i].transform.position - (Vector2)hitOrigin.position;
            if (Vector2.Angle(hitOrigin.right, toTarget) <= coneAngle * 0.5f)
            {
                HandleHit(hits[i]);
            }
        }
    }

    // Refactored for clarity by using enum casting, early exits and helper methods
    private void HandleHit(Collider2D collider)
    {
        // Cast layer to enum for readability
        var hitLayer = (Utils.Enums.ObjectLayers)collider.gameObject.layer;

        switch (hitLayer)
        {
            case Utils.Enums.ObjectLayers.ParriableLayer when isPlayer:
                ProcessParry(collider);
                break;

            case Utils.Enums.ObjectLayers.Player:
                ProcessPlayerHit();
                break;

            case Utils.Enums.ObjectLayers.Wall:
                Debug.Log("Hit wall");
                break;

            case Utils.Enums.ObjectLayers.Enemy:
                ProcessEnemyHit(collider);
                break;

            default:
                // Layer not relevant
                break;
        }
    }

    private void ProcessParry(Collider2D collider)
    {
        Debug.Log("PARRY DETECTED");
        var enemy = collider.transform.parent
                             .GetComponentInChildren<IEnemy>();

        if (enemy == null || enemy.IsEnemyDead() || enemy.IsStunned())
            return;

        AudioSource.PlayClipAtPoint(parrySound, wielder.transform.position);
        enemy.SetIsEnemyStunned();
    }

    // Initiates player hit logic with a delay before stunned check
    private void ProcessPlayerHit()
    {
        // Delay the stunned check by 0.2 seconds
        StartCoroutine(DelayedProcessPlayerHit());
    }

    private IEnumerator DelayedProcessPlayerHit()
    {
        if (waitBeforeCallCameOver >= fireRate)
        {
            Debug.LogError("WARNING! the waitBeforeCallCameOver seems to be >= of fireRate, which is not correct!");
        }

        yield return new WaitForSeconds(waitBeforeCallCameOver); // give 0.2 seconds of gap otherwise player will die even if parry was successfull

        var wielderEnemy = wielder.transform.parent
                                     .GetComponentInChildren<IEnemy>();
        if (wielderEnemy == null || wielderEnemy.IsStunned())
            yield break;

        Debug.Log("Hit player");
        logic.GameOver(swordRef);
    }

    private void ProcessEnemyHit(Collider2D collider)
    {
        Debug.Log("Hit enemy");
        var enemy = collider.transform.parent
                             .GetComponentInChildren<IEnemy>();

        if (enemy == null || enemy.IsEnemyDead())
            return;

        enemy.SetIsEnemyDead(true);
        logic.AddEnemyKilledPoints(enemy as IPoints);
    }


#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (transform == null) return;

        Gizmos.color = Color.yellow;

        Vector3 origin = transform.position;
        float halfAngle = coneAngle * 0.5f;

        Gizmos.DrawWireSphere(origin, coneRange);

        Vector3 forward = transform.right;
        Quaternion leftRot = Quaternion.Euler(0f, 0f, halfAngle);
        Quaternion rightRot = Quaternion.Euler(0f, 0f, -halfAngle);

        Vector3 leftDir = leftRot * forward;
        Vector3 rightDir = rightRot * forward;

        Gizmos.DrawLine(origin, origin + leftDir * coneRange);
        Gizmos.DrawLine(origin, origin + rightDir * coneRange);
    }
#endif
}
