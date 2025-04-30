using System.Collections;
using UnityEngine;

public class SwardScript : MonoBehaviour
{
    [SerializeField] private float coneAngle = 45f;
    [SerializeField] private float coneRange = 5f;
    private bool isPlayer = false;
    private bool canSwing = true;
    private GameObject wielder;
    private AudioClip swingSound;
    [SerializeField] private LayerMask hitLayers;
    private float fireRate = 1f;
    private LogicManager logic;
    private LayerMask finalHitLayers;
    private GameObject player;
    private float swingTimer = 0f;
    private bool isSwinging = false;
    private IGun swordRef;

    public void Initialize(GameObject wielder, AudioClip swingSound, IGun sword)
    {
        player = GameObject.FindGameObjectWithTag(Utils.Const.PLAYER_TAG);
        if (wielder.layer == (int)Utils.Enums.ObjectLayers.Player)
        {
            gameObject.layer = (int)Utils.Enums.ObjectLayers.SwingByPlayer;
            isPlayer = true;
        }

        this.wielder = wielder;
        this.swingSound = swingSound;
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

    private void HandleHit(Collider2D collider)
    {
        switch (collider.gameObject.layer)
        {
            case (int)Utils.Enums.ObjectLayers.Player:
                Debug.Log("Hit player");
                logic.GameOver(swordRef); // TODO change the approach so we can add more weapon varaiety
                break;
            case (int)Utils.Enums.ObjectLayers.Wall:
                Debug.Log("Hit wall");
                break;
            case (int)Utils.Enums.ObjectLayers.Enemy:
                Debug.Log("Hit enemy");
                if (collider.transform.parent.GetComponentInChildren<IEnemy>() is IEnemy enemy &&
                    !enemy.IsEnemyDead())
                {
                    enemy.SetIsEnemyDead(true);
                    logic.AddEnemyKilledPoints(enemy as IPoints);
                }
                break;
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Transform hitOrigin = transform;
        if (hitOrigin == null) return;
        Gizmos.color = Color.yellow;
        Vector2 origin = hitOrigin.position;
        float halfAngle = coneAngle * 0.5f;

        Gizmos.DrawWireSphere(origin, coneRange);

        Vector2 forward = hitOrigin.right;
        Quaternion leftRot = Quaternion.Euler(0f, 0f, halfAngle);
        Quaternion rightRot = Quaternion.Euler(0f, 0f, -halfAngle);
        Vector2 leftDir = leftRot * forward;
        Vector2 rightDir = rightRot * forward;

        Gizmos.DrawLine(origin, origin + leftDir * coneRange);
        Gizmos.DrawLine(origin, origin + rightDir * coneRange);
    }
#endif
}
