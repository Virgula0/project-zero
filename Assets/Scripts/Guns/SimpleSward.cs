using System;
using System.Collections;
using UnityEngine;

public class SimpleSword : MonoBehaviour, IPrimary, IMelee
{
    private readonly float fireRate = 1f; // 1 hit per second
    private readonly int magCap = int.MaxValue;
    private int numberOfReloads = int.MaxValue;
    private int ammoCount;
    private Animator playerAnim;
    private Animator goonAnim;
    
    [SerializeField] private float minDistanceForSwing = 2f;
    [SerializeField] private GameObject swingPrefab;
    [SerializeField] private SpriteRenderer staticWeaponSprite;
    [SerializeField] private Sprite equippedSprite;
    [SerializeField] private Sprite goonEquippedSprite;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private AudioClip swingSound;
    [SerializeField] private AudioClip equipSound;
    [SerializeField] private AudioClip parrySound;

    private GameObject wielder;
    private bool isGoingToBePickedUp = false;
    private SwordScript currentInitScript;
    private GameObject currentInitPrefab; 

    public void Setup(GameObject player)
    {
        wielder = player;
        currentInitPrefab = Instantiate(swingPrefab, wielder.transform.position, Quaternion.identity);
        currentInitScript = currentInitPrefab.GetComponent<SwordScript>();
        currentInitScript.Initialize(wielder, swingSound, parrySound, this);
        playerAnim = player.GetComponentInChildren<Animator>();
        if (wielder.layer != (int)Utils.Enums.ObjectLayers.Player){
            goonAnim = wielder.GetComponentInChildren<Animator>();
        }
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

        if (wielder.layer == (int)Utils.Enums.ObjectLayers.Player){
            playerAnim.enabled = true;

            playerAnim.Play(Utils.Animations.PLAYER_SWORD_ATTACK, 0, 0f);
        }else{
            goonAnim.enabled = true;

            goonAnim.Play(Utils.Animations.ENEMY_SWORD_ATTACK, 0, 0f);
        }
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

    public IEnumerator SaveStatus(IPrimary other)
    {
        yield break;
    }

    public bool IsGoingToBePickedUp() => isGoingToBePickedUp;
    public void SetIsGoingToBePickedUp(bool status) => isGoingToBePickedUp = status;

    public void PostSetup()
    {
        Destroy(currentInitPrefab);
    }

    public Sprite GetGoonEquippedSprite()
    {
        return this.goonEquippedSprite;
    }

    public float MinDistanceForSwing()
    {
        return minDistanceForSwing;
    }
}
