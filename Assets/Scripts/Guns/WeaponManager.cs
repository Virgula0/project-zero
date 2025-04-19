using System;
using System.Collections;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    private IGun currentLoadedWeapon;
    private float timer; // timer counts the timer elapsed from the last shot, in seconds
    private UIManager uiManager;
    private Sprite defaultPlayerSprite;
    private bool isReloading = false;
    private Rigidbody2D playerBody;
    private WeaponSpawner spawner;
    private GameObject gunPrefab;
    private CursorChanger cursorChanger;

    [SerializeField] SpriteRenderer playerSpriteRenderer;
    [SerializeField] Canvas ui;
    [SerializeField] AudioSource audioSrc;

    private float forwardSpawnGunPrefabOffset = 3f;
    private float upOffsetSpawnGunPrefab = 1f;

    void Start()
    {
        this.cursorChanger = GameObject.FindGameObjectWithTag(Utils.Const.CURSOR_CHANGER_TAG).GetComponent<CursorChanger>();
        this.playerBody = GetComponentInParent<Rigidbody2D>();
        this.defaultPlayerSprite = playerSpriteRenderer.sprite;
        this.uiManager = ui.GetComponent<UIManager>();
        this.spawner = GameObject.FindGameObjectWithTag(Utils.Const.WEAPON_SPAWNER_TAG).GetComponent<WeaponSpawner>();
    }

    // this will be invoked externally
    public void LoadNewGun(IGun weapon, GameObject shooter, GameObject prefab)
    {
        if (weapon == null)
        {
            throw new NullReferenceException("GUN LOAD CANNOT BE NULL, THE PASSED REFERENCE TO WEAPON MANAGER IS NULL");
        }

        if (playerSpriteRenderer == null)
        {
            throw new NullReferenceException("PLAYER SPRITE RENDERER CANNOT BE NULL, THE PASSED REFERENCE TO THE PLAYER SPRITE RENDERER IS NULL");
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
        playerSpriteRenderer.sprite = weapon.GetEquippedSprite();
        uiManager.UpdateWeaponIcon(currentLoadedWeapon.GetStaticWeaponSprite());
        uiManager.UpdateBullets(currentLoadedWeapon.GetAmmoCount());
        uiManager.UpdateReloads(currentLoadedWeapon.GetNumberOfReloads());
        currentLoadedWeapon.SetIsGoingToBePickedUp(false);
    }

    private void UnloadCurrentGun()
    {
        if (currentLoadedWeapon == null)
        {
            throw new NullReferenceException("GUN CANNOT BE DELOADED IF NO ONE HAS BEEN LOADED");
        }
        Debug.Log("Weapon deloaded");
        audioSrc.PlayOneShot(currentLoadedWeapon.GetEquipSfx());

        if (currentLoadedWeapon.GetAmmoCount() > 0 || currentLoadedWeapon.GetNumberOfReloads() > 0)
        {
            RecreatePrefab();
        }

        currentLoadedWeapon = null;
        cursorChanger.ChangeToDefaultCursor();
        Destroy(this.gunPrefab);
        timer = 0;
        playerSpriteRenderer.sprite = defaultPlayerSprite;
        uiManager.UpdateBullets(0);
        uiManager.UpdateReloads(0);
        uiManager.UpdateWeaponIcon(null);
    }

    private void RecreatePrefab()
    {
        Vector3 mouseWorld3D = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mouseWorld2D = new Vector2(mouseWorld3D.x, mouseWorld3D.y);
        Vector2 origin = playerBody.position;
        Vector2 forward = (mouseWorld2D - origin).normalized;
        Vector2 up = new Vector2(-forward.y, forward.x); // rotate forward by 90° CCW
        Vector2 spawnPos = origin + forward * forwardSpawnGunPrefabOffset + up * upOffsetSpawnGunPrefab;
        GameObject newPrefab = Instantiate(gunPrefab, spawnPos, Quaternion.identity);
        StartCoroutine(newPrefab.GetComponent<IGun>().SaveStatus(currentLoadedWeapon)); // will save the status after awaked, that's why a coroutine
        newPrefab.SetActive(true);
        spawner.AddAvailableGunOnTheGroundPosition(spawnPos, currentLoadedWeapon);
    }

    // Update is called once per frame
    void Update()
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
        if (Input.GetMouseButton((int)Utils.Enums.MouseButtons.RightButton))
        {
            UnloadCurrentGun();
            return;
        }

        if (Input.GetKeyDown(KeyCode.R) && currentLoadedWeapon.GetNumberOfReloads() > 0
            && currentLoadedWeapon.GetAmmoCount() < currentLoadedWeapon.GetMegCap())
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
        }
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
