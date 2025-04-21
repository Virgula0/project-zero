using System.Collections;
using UnityEngine;

public class SwardScript : MonoBehaviour
{
    [SerializeField] private float coneAngle = 45f;
    [SerializeField] private float coneRange = 2f;
    private bool isPlayer = false;
    private bool canSwing = true;
    private GameObject wielder;
    private AudioClip swingSound;

    private LayerMask enemyLayer;
    private float fireRate = 1f;
    private LogicManager logic;

    public void Initialize(GameObject wielder, AudioClip swingSound, LayerMask enemyLayer)
    {
        if (wielder.layer == (int)Utils.Enums.ObjectLayers.Player)
        {
            // gameObject.layer = (int)Utils.Enums.ObjectLayers.BulletByPlayer; // set a layer to be detected by enemies but we need to do this because it will detect only if it's by the player
            isPlayer = true;
        }
        this.wielder = wielder;
        this.swingSound = swingSound;
        this.enemyLayer = enemyLayer;
    }

    void Start()
    {
        this.logic = GameObject.FindGameObjectWithTag(Utils.Const.LOGIC_MANAGER_TAG).GetComponent<LogicManager>();
    }

    public bool GetCanSwing()
    {
        return canSwing;
    }

    public void Swing()
    {
        transform.position = wielder.transform.position; // update current position
        StartCoroutine(SwingCoroutine());
    }

    private IEnumerator SwingCoroutine()
    {
        canSwing = false;

        if (swingSound != null)
            AudioSource.PlayClipAtPoint(swingSound, wielder.transform.position);

        yield return new WaitForSeconds(0.1f);

        Transform hitOrigin = wielder.transform;

        // update the enemy layer adding the player layer too to let this to be equipped by enemies too
        Collider2D[] hits = Physics2D.OverlapCircleAll((Vector2)hitOrigin.position, coneRange, enemyLayer);

        for (int i = 0; i < hits.Length; i++)
        {
            Vector2 toTarget = (Vector2)hits[i].transform.position - (Vector2)hitOrigin.position;
            if (Vector2.Angle(hitOrigin.right, toTarget) <= coneAngle * 0.5f)
            {
                HandleHit(hits[i]);
            }
        }

        yield return new WaitForSeconds(1f / fireRate - 0.1f);
        canSwing = true;
    }


    private void HandleHit(Collider2D collider)
    {
        switch (collider.gameObject.layer)
        {
            case (int)Utils.Enums.ObjectLayers.Player:
                Debug.Log("Hit Player");
                this.logic.GameOver();
                break;
            case (int)Utils.Enums.ObjectLayers.Wall:
                Debug.Log("Hit Wall");
                break;
            case (int)Utils.Enums.ObjectLayers.Enemy:
                Debug.Log("Hit Enemy");
                if (collider.transform.parent.GetComponentInChildren<IEnemy>() is IEnemy enemy)
                {
                    if (!enemy.IsEnemyDead()) // avoid to do actions on already died enemy
                    {
                        // TODO: change the enemy sprite to a dead one, update IEnemy interface
                        enemy.SetIsEnemyDead(true);
                        logic.AddEnemyKilledPoints(enemy as IPoints);
                    }
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

        // Draw range circle
        Gizmos.DrawWireSphere(origin, coneRange);

        // Draw cone lines
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
