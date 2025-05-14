using System;
using System.Collections;
using UnityEngine;

// This class is associated directly to prefabs and defines the entire "GunType" behaviour
public class SimpleGun : MonoBehaviour, IPrimary, IRestricted, IThrowable, IRanged
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
    [SerializeField] private AudioClip throwSound;
    [SerializeField] private GameObject throwablePrefab;
    [SerializeField] private bool reservedToPlayer;
    private bool isGoingToBePickedUp = false;
    private GameObject shooterObject;
    private bool awakeExecuted = false;
    private GameObject throwable;

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
        bulletScript.Initialize(shooterObject, this);
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
        --this.numberOfReloads;
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

    public IEnumerator SaveStatus(IPrimary other)
    {
        while (!awakeExecuted)
        {
            yield return null;
        }
        this.numberOfReloads = other.GetNumberOfReloads();
        this.ammoCount = other.GetAmmoCount();
    }

    public bool IsGoingToBePickedUp()
    {
        return isGoingToBePickedUp;
    }

    public void SetIsGoingToBePickedUp(bool status)
    {
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

    public void ThrowWhereMousePoints()
    {
        this.throwable = Instantiate(throwablePrefab, shooterObject.transform.position, Quaternion.identity);
        ThrowableScript throwableScript = this.throwable.GetComponent<ThrowableScript>();
        throwableScript.Initialize(staticWeaponSprite);
    }

    public IEnumerator PlayThrowSfx(AudioSource audioSrc)
    {
        // cache the clip length so you don’t re-query it every loop
        float clipLength = throwSound.length;
        // as long as the throwable still exists, keep playing
        while (throwable != null)
        {
            audioSrc.PlayOneShot(throwSound);
            yield return new WaitForSeconds(clipLength);
        }
    }

    public bool IsEquippableByPlayerOnly()
    {
        return reservedToPlayer;
    }
}
