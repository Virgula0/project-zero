using System;
using System.Collections;
using UnityEngine;

public class SimpleSword : MonoBehaviour, IGun
{
    private readonly float fireRate         = 1f;
    private readonly int   magCap           = int.MaxValue;
    private int            numberOfReloads  = int.MaxValue;
    private int            ammoCount;

    [SerializeField] private SpriteRenderer staticWeaponSprite;
    [SerializeField] private Sprite         equippedSprite;
    [SerializeField] private Transform      hitOrigin;
    [SerializeField] private float          coneAngle         = 45f;
    [SerializeField] private float          coneRange         = 2f;
    [SerializeField] private LayerMask      enemyLayer;
    [SerializeField] private AudioClip      swingSound;
    [SerializeField] private AudioClip      reloadSound;
    [SerializeField] private AudioClip      equipSound;

    private GameObject     wielder;
    private bool           canSwing          = true;
    private bool           awakeExecuted     = false;
    private bool           isGoingToBePickedUp = false;
    private Collider2D[]   hits              = new Collider2D[1]; // 1 hit at time by the collider

    public void Setup(GameObject player)
    {
        wielder = player;
    }

    void Awake()
    {
        ammoCount     = magCap;
        awakeExecuted = true;
    }

    void Start()
    {
        if (equippedSprite == null)
            throw new NullReferenceException("EQUIPPED SPRITE FOR WEAPON " + this.ToString() + " IS NULL");
    }

    public void Shoot()
    {
        if (!canSwing) return;
        StartCoroutine(SwingCoroutine());
    }

    private IEnumerator SwingCoroutine()
    {
        canSwing = false;

        if (swingSound != null)
            AudioSource.PlayClipAtPoint(swingSound, wielder.transform.position);

        yield return new WaitForSeconds(0.1f);

        // OverlapCircleNonAlloc does not allocates memory each time as done by OverlapCircleAll instead
        int count = Physics2D.OverlapCircleNonAlloc(
            (Vector2)hitOrigin.position,
            coneRange,
            hits,
            enemyLayer
        );

        for (int i = 0; i < count; i++)
        {
            Vector2 toTarget = (Vector2)hits[i].transform.position - (Vector2)hitOrigin.position;
            if (Vector2.Angle(hitOrigin.right, toTarget) <= coneAngle * 0.5f)
            {
                /*
                var dmg = hits[i].GetComponent<IDamageable>();
                if (dmg != null)
                    dmg.TakeDamage(damage);
                */
            }
            hits[i] = null;
        }

        yield return new WaitForSeconds(1f / fireRate - 0.1f);
        canSwing = true;
    }

    public void Reload()
    {
        ammoCount = magCap;
        Debug.Log("Sword reload (noop)");
    }

    public int GetNumberOfReloads()       => numberOfReloads;
    public float GetFireRate()            => fireRate;
    public int GetMegCap()                => magCap;
    public int GetAmmoCount()             => ammoCount;
    public Sprite GetEquippedSprite()     => equippedSprite;
    public Sprite GetStaticWeaponSprite() => staticWeaponSprite.sprite;
    public AudioClip GetShotSfx()         => swingSound;
    public AudioClip GetReloadSfx()       => reloadSound;
    public AudioClip GetEquipSfx()        => equipSound;

    public IEnumerator SaveStatus(IGun other)
    {
        while (!awakeExecuted)
            yield return null;
    }

    public bool IsGoingToBePickedUp()              => isGoingToBePickedUp;
    public void SetIsGoingToBePickedUp(bool status) => isGoingToBePickedUp = status;
}
