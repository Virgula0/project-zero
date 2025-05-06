using System.Collections;
using UnityEngine;

public class BiteScript : MonoBehaviour
{
    [SerializeField] private float coneAngle = 45f;
    [SerializeField] private float coneRange = 3f;
    private bool canBite = true;
    private GameObject wielder; // wielder is the game object which contains the sprite
    private AudioClip biteSound;
    [SerializeField] private LayerMask hitLayers;
    private float fireRate = 1f;
    private LogicManager logic;
    private LayerMask finalHitLayers;
    private GameObject player;
    private float biteTimer = 0f;
    private bool isBiting = false;
    private float waitBeforeCallCameOver = 0.2f;
    private IPrimary biteRef;

    public void Initialize(GameObject wielder, AudioClip biteSound, IPrimary bite)
    {
        player = GameObject.FindGameObjectWithTag(Utils.Const.PLAYER_TAG);

        this.wielder = wielder;
        this.biteSound = biteSound;
        this.biteRef = bite;

        int shooterLayerValue;

        shooterLayerValue = 1 << (int)Utils.Enums.ObjectLayers.Enemy;
        finalHitLayers = hitLayers & ~shooterLayerValue;
    }

    void Start()
    {
        logic = GameObject.FindGameObjectWithTag(Utils.Const.LOGIC_MANAGER_TAG)
                         .GetComponent<LogicManager>();
    }

    public bool GetCanBite()
    {
        return canBite;
    }

    void Update()
    {
        //Debug.Log(isBiting);
        if (isBiting)
        {
            biteTimer += Time.deltaTime;

            // Hit happens after 0.1s
            if (biteTimer >= 0.1f && biteTimer < 0.1f + Time.deltaTime)
            {
                Debug.Log("PERFORMING HIT");
                PerformHit();
            }

            // Bite cooldown ends
            if (biteTimer >= 1f / fireRate)
            {
                canBite = true;
                isBiting = false;
            }
        }
    }

    public void Bite()
    {
        Debug.Log("INSIDE BITE");
        if (!canBite) return;

        Vector2 dir;
        // enemy
        dir = player.transform.position - wielder.transform.position;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
        transform.position = wielder.transform.position;

        if (biteSound != null)
            AudioSource.PlayClipAtPoint(biteSound, wielder.transform.position);

        canBite = false;
        isBiting = true;
        biteTimer = 0f;
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
            case Utils.Enums.ObjectLayers.Player:
                ProcessPlayerHit();
                break;

            case Utils.Enums.ObjectLayers.Wall:
                Debug.Log("Hit wall");
                break;

            default:
                // Layer not relevant
                break;
        }
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
        logic.GameOver(biteRef);
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
