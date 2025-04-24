using System;
using System.Collections;
using UnityEngine;

public class SimpleSword : MonoBehaviour, IGun, IMelee
{
    private readonly float fireRate = 1f; // 1 hit per second
    private readonly int magCap = int.MaxValue;
    private int numberOfReloads = int.MaxValue;
    private int ammoCount;
    private Animator anim;

    [SerializeField] private GameObject swingPrefab;
    [SerializeField] private SpriteRenderer staticWeaponSprite;
    [SerializeField] private Sprite equippedSprite;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private AudioClip swingSound;
    [SerializeField] private AudioClip equipSound;

    private GameObject wielder;
    private bool isGoingToBePickedUp = false;
    private SwardScript currentInitScript;
    private GameObject currentInitPrefab; 

    public void Setup(GameObject player)
    {
        wielder = player;
        currentInitPrefab = Instantiate(swingPrefab, wielder.transform.position, Quaternion.identity);
        currentInitScript = currentInitPrefab.GetComponent<SwardScript>();
        currentInitScript.Initialize(wielder, swingSound);
        anim = player.GetComponentInChildren<Animator>();
    }

    void Awake()
    {
        ammoCount = magCap;
    }

    void Start()
    {
        if (equippedSprite == null)
            throw new NullReferenceException("EQUIPPED SPRITE FOR WEAPON " + this.ToString() + " IS NULL");
    }

    public void Shoot()
    {   
        if (!currentInitScript.GetCanSwing()) return;
        anim.enabled = true;
        
        anim.Play("PlayerSwordAttackAnimation", 0, 0f);
        currentInitScript.Swing();
    }


    public void Reload(){}
    public int GetNumberOfReloads() => numberOfReloads;
    public float GetFireRate() => fireRate;
    public int GetMegCap() => magCap;
    public int GetAmmoCount() => ammoCount;
    public Sprite GetEquippedSprite() => equippedSprite;
    public Sprite GetStaticWeaponSprite() => staticWeaponSprite.sprite;
    public AudioClip GetShotSfx() => swingSound;
    public AudioClip GetReloadSfx() => null;
    public AudioClip GetEquipSfx() => equipSound;

    public IEnumerator SaveStatus(IGun other)
    {
        yield break;
    }

    public bool IsGoingToBePickedUp() => isGoingToBePickedUp;
    public void SetIsGoingToBePickedUp(bool status) => isGoingToBePickedUp = status;

    public void PostSetup()
    {
        Destroy(currentInitPrefab);
    }
}
