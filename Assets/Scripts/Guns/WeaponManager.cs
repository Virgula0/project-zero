using System;
using System.Collections;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    private IPrimary currentLoadedWeapon;
    private float timer; // timer counts the timer elapsed from the last shot, in seconds
    private float secondaryTimer;
    private UIManager uiManager;
    private bool isReloading = false;
    private bool canShoot = true;
    private Rigidbody2D playerBody;
    private WeaponSpawner spawner;
    private GameObject gunPrefab;
    private CursorChanger cursorChanger;
    PlayerAnimationScript playerAnimCtrl;
    [SerializeField] AudioSource audioSrc;
    private PlayerScript playerScript;
    private float loadTime = 0;
    private BoxCollider2D playerCollider;
    private bool isThrown = false;
    private ISecondary currentLoadedSecondary;
    private GameObject secondaryPrefab;

    IEnumerator Start()
    {
        this.cursorChanger = GameObject.FindGameObjectWithTag(Utils.Const.CURSOR_CHANGER_TAG).GetComponent<CursorChanger>();
        this.playerBody = GetComponentInParent<Rigidbody2D>();
        this.playerAnimCtrl = transform.parent.GetComponentInChildren<PlayerAnimationScript>();

        this.uiManager = GameObject.FindGameObjectWithTag(Utils.Const.UI_MANAGER_TAG).GetComponent<UIManager>();
        this.spawner = GameObject.FindGameObjectWithTag(Utils.Const.WEAPON_SPAWNER_TAG).GetComponent<WeaponSpawner>();
        this.playerScript = GameObject.FindGameObjectWithTag(Utils.Const.PLAYER_TAG).GetComponent<PlayerScript>();
        playerCollider = gameObject.GetComponentInParent<BoxCollider2D>();

        while (!playerAnimCtrl.IsAnimationScriptReady()) // let's wait for the script animation to be ready first
        {
            yield return null;
        }

        playerAnimCtrl.SetDefaultSprite();
        ResizePlayerCollider();
    }

    // this will be invoked externally
    public void LoadNewGun(IPrimary weapon, GameObject shooter, GameObject prefab)
    {
        if (weapon == null)
        {
            throw new NullReferenceException("GUN LOAD CANNOT BE NULL, THE PASSED REFERENCE TO WEAPON MANAGER IS NULL");
        }

        if (playerAnimCtrl == null)
        {
            throw new NullReferenceException("PLAYER ANIMATION SCRIPT CANNOT BE NULL, THE REFERENCE TO THE PLAYER ANIMATION SCRIPT IS NULL");
        }

        if (currentLoadedWeapon != null)
        {
            UnloadCurrentGun();
        }

        // must be done whatever a new gun gets loaded
        this.gunPrefab = prefab;
        currentLoadedWeapon = weapon;

        // we're allowed to shoot at te beginning 
        timer = float.PositiveInfinity;
        cursorChanger.ChangeToTargetCursor();
        currentLoadedWeapon.Setup(shooter);
        audioSrc.PlayOneShot(currentLoadedWeapon.GetEquipSfx());

        playerAnimCtrl.SetEquippedWeponSprite(weapon.GetEquippedSprite());
        ResizePlayerCollider();

        uiManager.UpdateWeaponIcon(currentLoadedWeapon.GetStaticWeaponSprite());
        uiManager.UpdateBullets(currentLoadedWeapon.GetAmmoCount());
        uiManager.UpdateReloads(currentLoadedWeapon.GetNumberOfReloads());
        currentLoadedWeapon.SetIsGoingToBePickedUp(false);
        loadTime = Time.time;
    }

    public void LoadNewSecondary(ISecondary secondary, GameObject prefab, GameObject shooter)
    {
        if (secondary == null)
        {
            throw new NullReferenceException("SECONDARY LOAD CANNOT BE NULL, THE PASSED REFERENCE TO WEAPON MANAGER IS NULL");
        }

        if (playerAnimCtrl == null)
        {
            throw new NullReferenceException("PLAYER ANIMATION SCRIPT CANNOT BE NULL, THE REFERENCE TO THE PLAYER ANIMATION SCRIPT IS NULL");
        }

        // must be done whatever a new gun gets loaded
        this.secondaryPrefab = prefab;
        currentLoadedSecondary = secondary;

        // we're allowed to shoot at te beginning 
        secondaryTimer = float.PositiveInfinity;
        currentLoadedSecondary.Setup(shooter);
        audioSrc.PlayOneShot(currentLoadedSecondary.GetEquipSfx());

        uiManager.UpdateSecondaryIcon(currentLoadedSecondary.GetStaticWeaponSprite());
        uiManager.UpdateCharges(currentLoadedSecondary.GetAmmoCount());
        currentLoadedSecondary.SetIsGoingToBePickedUp(false);
        loadTime = Time.time;
    }

    private void UnloadCurrentSecondary()
    {
        if (currentLoadedSecondary == null)
        {
            throw new NullReferenceException("GUN CANNOT BE DELOADED IF NO ONE HAS BEEN LOADED");
        }

        if (Time.time - loadTime <= 0.3f)
        {
            return; // not enough time has passed since equipping
        }

        Debug.Log("Secondary deloaded");
        audioSrc.PlayOneShot(currentLoadedSecondary.GetEquipSfx());

        currentLoadedSecondary.PostSetup();
        Destroy(this.secondaryPrefab);
        secondaryTimer = 0;
        currentLoadedSecondary = null;

        uiManager.UpdateCharges(0);
        uiManager.UpdateSecondaryIcon(null);
    }

    private void UnloadCurrentGun()
    {
        if (currentLoadedWeapon == null)
        {
            throw new NullReferenceException("GUN CANNOT BE DELOADED IF NO ONE HAS BEEN LOADED");
        }

        if (Time.time - loadTime <= 0.3f || currentLoadedWeapon.IsGoingToBePickedUp())
        {
            return; // not enough time has passed since equipping
        }

        Debug.Log("Weapon deloaded");
        audioSrc.PlayOneShot(currentLoadedWeapon.GetEquipSfx());

        if (currentLoadedWeapon.GetAmmoCount() > 0 || currentLoadedWeapon.GetNumberOfReloads() > 0)
        {
            RecreatePrefab();
        }

        currentLoadedWeapon.PostSetup();
        cursorChanger.ChangeToDefaultCursor();
        Destroy(this.gunPrefab);
        timer = 0;
        currentLoadedWeapon = null;

        playerAnimCtrl.SetDefaultSprite();
        ResizePlayerCollider();

        uiManager.UpdateBullets(0);
        uiManager.UpdateReloads(0);
        uiManager.UpdateWeaponIcon(null);
    }

    private void RecreatePrefab()
    {
        Vector2 origin = playerBody.position;
        GameObject newPrefab = Instantiate(gunPrefab, origin, Quaternion.identity);
        StartCoroutine(newPrefab.GetComponent<IPrimary>().SaveStatus(currentLoadedWeapon)); // will save the status after awaked, that's why a coroutine
        newPrefab.SetActive(true);
        spawner.AddAvailableGunOnTheGroundPosition(origin, currentLoadedWeapon);
    }

    // Update is called once per frame
    void Update()
    {
        if (playerScript == null)
        {
            return;
        }

        if (!playerScript.IsPlayerAlive())
        {
            return;
        }

        if (!canShoot)
        {
            return;
        }

        ManagePrimary();
        ManageSecondary();
    }

    private void ManageSecondary()
    {
        if (currentLoadedSecondary == null)
        {
            return;
        }

        secondaryTimer += Time.deltaTime;

        if (currentLoadedSecondary.GetAmmoCount() < 1)
        {
            UnloadCurrentSecondary();
            return;
        }

        if (Input.GetKeyDown(KeyCode.Q) && secondaryTimer >= currentLoadedSecondary.GetFireRate()
        && currentLoadedSecondary.GetAmmoCount() > 0)
        {
            if (currentLoadedWeapon != null)
            {
                playerAnimCtrl.SetPlayerLastSprite(currentLoadedWeapon.GetEquippedSprite());
            }
            secondaryTimer = 0;
            currentLoadedSecondary.Shoot();
            audioSrc.PlayOneShot(currentLoadedSecondary.GetShotSfx());

            uiManager.UpdateCharges(currentLoadedSecondary.GetAmmoCount());
            return;
        }
    }

    private void ManagePrimary()
    {
        // we do nothing if we do not have a loaded weapon already
        if (currentLoadedWeapon == null)
        {
            return;
        }

        timer += Time.deltaTime;

        if (isReloading)
        {
            return;
        }

        // if left button is pressed, let an user to leave the weapon
        if (Input.GetMouseButton((int)Utils.Enums.MouseButtons.RightButton) | isThrown)
        {
            UnloadCurrentGun();
            isThrown = false;
            return;
        }

        if (Input.GetKeyDown(KeyCode.R) && currentLoadedWeapon.GetNumberOfReloads() > 0
            && currentLoadedWeapon.GetAmmoCount() < currentLoadedWeapon.GetMegCap() &&
            currentLoadedWeapon is IRanged)
        {
            currentLoadedWeapon.Reload();
            uiManager.UpdateReloads(currentLoadedWeapon.GetNumberOfReloads());
            uiManager.UpdateBullets(currentLoadedWeapon.GetAmmoCount());
            audioSrc.PlayOneShot(currentLoadedWeapon.GetReloadSfx());
            isReloading = true;
            StartCoroutine(WaitForSfxToEnd());
            return;
        }

        if (Input.GetMouseButton((int)Utils.Enums.MouseButtons.LeftButton) &&
            timer >= currentLoadedWeapon.GetFireRate() &&
            currentLoadedWeapon.GetAmmoCount() > 0)
        {
            timer = 0;
            currentLoadedWeapon.Shoot();
            audioSrc.PlayOneShot(currentLoadedWeapon.GetShotSfx());
            uiManager.UpdateBullets(currentLoadedWeapon.GetAmmoCount());
            return;
        }

        if (Input.GetMouseButton((int)Utils.Enums.MouseButtons.LeftButton) &&
            currentLoadedWeapon is IThrowable throwable &&
            currentLoadedWeapon.GetAmmoCount() < 1 && currentLoadedWeapon.GetNumberOfReloads() < 1 &&
            timer >= currentLoadedWeapon.GetFireRate())
        {
            timer = 0;
            throwable.ThrowWhereMousePoints();
            StartCoroutine(throwable.PlayThrowSfx(audioSrc));
            isThrown = true;
            return;
        }
    }

    public IPrimary GetCurrentLoadedWeapon()
    {
        return currentLoadedWeapon;
    }

    public bool GetCanShoot()
    {
        return canShoot;
    }

    public void SetCanShoot(bool newBool)
    {
        canShoot = newBool;
    }

    public ISecondary GetCurrentLoadedSecondary()
    {
        return currentLoadedSecondary;
    }

    public void ResizePlayerCollider()
    {
        Vector2 spriteSize = playerAnimCtrl.GetSpriteSize();
        Vector3 spriteScale = playerAnimCtrl.GetSpriteScale();
        Vector2 scaledSize = new Vector2(spriteSize.x * spriteScale.x, spriteSize.y * spriteScale.y); // Multiply the sprite size by the parent’s scale
        playerCollider.size = scaledSize;
    }

    IEnumerator WaitForSfxToEnd()
    {

        while (audioSrc.isPlaying)
        {
            yield return null;
        }

        isReloading = false;
    }
}
