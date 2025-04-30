using System;
using System.Collections;
using UnityEngine;

// This class is associated directly to prefabs and defines the entire "GunType" behaviour
public class SimpleGun : MonoBehaviour, IGun, IRanged
{
    private readonly float fireRate = 0.5f; // in seconds, 0.5 seconds between each shot
    private readonly int magCap = 10;
    private int numberOfReloads = 5; // total bullets available can be seen as numberOfReloads*magCap
    private int ammoCount;

    [SerializeField] private SpriteRenderer staticWeaponSprite;
    [SerializeField] private Sprite equippedSprite;
    [SerializeField] private Sprite goonEquippedSprite;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Sprite bulletSprite;
    [SerializeField] private AudioClip shotSound;
    [SerializeField] private AudioClip reloadSound;
    [SerializeField] private AudioClip equipSound;
    private bool isGoingToBePickedUp = false;
    private GameObject shooterObject;
    private bool awakeExecuted = false;

    public void Setup(GameObject player)
    {
        shooterObject = player;
    }

    void Awake()
    {
        // Needed because otherwise when executing SaveStatus the start will run after the save
        // the weird fact is that in the build the contrary happens
        this.ammoCount = this.magCap;
        awakeExecuted = true;
    }

    void Start()
    {
        if (equippedSprite == null)
        {
            throw new NullReferenceException("EQUIPPED SPRITE FOR WEAPON" + this.ToString() + " IS NULL");
        }

        if (bulletPrefab == null)
        {
            throw new NullReferenceException("BULLET PREFAB FOR WEAPON " + this.ToString() + " IS NULL");
        }

        bulletPrefab.GetComponentInChildren<SpriteRenderer>().sprite = bulletSprite;
    }

    public void Shoot()
    {
        ammoCount -= 1;
        GameObject bullet = Instantiate(bulletPrefab, shooterObject.transform.position, Quaternion.identity);
        SingleBulletScript bulletScript = bullet.GetComponent<SingleBulletScript>();
        bulletScript.Initialize(shooterObject);
        Debug.Log("BAM!");
        Debug.Log(ammoCount + "/" + magCap);
    }

    public void Reload()
    {
        if (this.ammoCount == this.magCap)
        {
            Debug.Log("Already loaded");
            return;
        }
        Debug.Log("Reloaded");
        Debug.Log($"Number of reloads available -> {--this.numberOfReloads}");
        this.ammoCount = this.magCap;
    }

    public int GetNumberOfReloads()
    {
        return this.numberOfReloads;
    }

    public float GetFireRate()
    {
        return this.fireRate;
    }

    public int GetMegCap()
    {
        return this.magCap;
    }

    public int GetAmmoCount()
    {
        return this.ammoCount;
    }

    public Sprite GetEquippedSprite()
    {
        return this.equippedSprite;
    }

    public Sprite GetStaticWeaponSprite()
    {
        // this method returns the sprite of grounded weapon so that it can be used as an icon in the ui.
        // return this.gameObject.GetComponentsInChildren<SpriteRenderer>()[1].sprite;
        return this.staticWeaponSprite.sprite;
    }

    public AudioClip GetShotSfx()
    {
        return this.shotSound;
    }

    public AudioClip GetReloadSfx()
    {
        return this.reloadSound;
    }

    public AudioClip GetEquipSfx()
    {
        return this.equipSound;
    }

    public IEnumerator SaveStatus(IGun other)
    {
        while (!awakeExecuted){
            yield return null;
        }
        this.numberOfReloads = other.GetNumberOfReloads();
        this.ammoCount = other.GetAmmoCount();
    }

    public bool IsGoingToBePickedUp() {
        return isGoingToBePickedUp;
    }

    public void SetIsGoingToBePickedUp(bool status){
        this.isGoingToBePickedUp = status;
    }

    public void PostSetup()
    {
        return;
    }

    public Sprite GetGoonEquippedSprite()
    {
        return this.goonEquippedSprite;
    }
}
