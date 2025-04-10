using System;
using System.Collections;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    private IGun currentLoadedWeapon;
    private float timer; // timer counts the timer elapsed from the last shot, in seconds
    private UIManager uiManager;
    private Sprite defaultPlayerSprite;

    [SerializeField] SpriteRenderer playerSpriteRenderer;
    [SerializeField] Canvas ui;
    [SerializeField] AudioSource audioSrc;

    void Start()
    {
        this.defaultPlayerSprite = playerSpriteRenderer.sprite;
        this.uiManager = ui.GetComponent<UIManager>();
    }


    // this will be invoked externally
    public void LoadNewGun(IGun weapon, GameObject shooter)
    {
        if (weapon == null)
        {
            throw new NullReferenceException("GUN LOAD CANNOT BE NULL, THE PASSED REFERENCE TO WEAPON MANAGER IS NULL");
        }

        if (playerSpriteRenderer == null)
        {
            throw new NullReferenceException("PLAYER SPRITE RENDERER CANNOT BE NULL, THE PASSED REFERENCE TO THE PLAYER SPRITE RENDERER IS NULL");
        }
        // must be done whatever a new gun gets loaded
        currentLoadedWeapon = weapon;

        // we're allowed to shoot at te beginning 
        timer = float.PositiveInfinity;
        currentLoadedWeapon.Setup(shooter);
        audioSrc.PlayOneShot(currentLoadedWeapon.GetEquipSfx());
        playerSpriteRenderer.sprite = weapon.GetEquippedSprite();
        uiManager.UpdateWeaponIcon(currentLoadedWeapon.GetStaticWeaponSprite());
        uiManager.UpdateBullets(currentLoadedWeapon.GetAmmoCount());
        uiManager.UpdateReloads(currentLoadedWeapon.GetNumberOfReloads());
    }

    private void UnloadCurrentGun()
    {
        if (currentLoadedWeapon == null)
        {
            throw new NullReferenceException("GUN CANNOT BE DELOADED IF NO ONE HAS BEEN LOADED");
        }

        Debug.Log("Weapon deloaded");
        audioSrc.PlayOneShot(currentLoadedWeapon.GetEquipSfx());
        currentLoadedWeapon = null;
        timer = 0;
        playerSpriteRenderer.sprite = defaultPlayerSprite;
        uiManager.UpdateBullets(0);
        uiManager.UpdateReloads(0);
        uiManager.UpdateWeaponIcon(null);
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

        // if left button is pressed, let an user to leave the weapon
        if (Input.GetMouseButton((int)Utils.Enums.MouseButtons.RightButton))
        {
            UnloadCurrentGun();
            return;
        }

        if (Input.GetKeyDown(KeyCode.R) && currentLoadedWeapon.GetNumberOfReloads() > 0)
        {
            currentLoadedWeapon.Reload();
            uiManager.UpdateReloads(currentLoadedWeapon.GetNumberOfReloads());
            uiManager.UpdateBullets(currentLoadedWeapon.GetAmmoCount());
            audioSrc.PlayOneShot(currentLoadedWeapon.GetReloadSfx()); 
            return;
        }

        if (Input.GetMouseButton((int)Utils.Enums.MouseButtons.LeftButton) &&
            timer >= currentLoadedWeapon.GetFireRate() &&
            currentLoadedWeapon.GetAmmoCount() > 0 && !audioSrc.isPlaying)
        {
            timer = 0;
            currentLoadedWeapon.Shoot();
            audioSrc.PlayOneShot(currentLoadedWeapon.GetShotSfx());
            uiManager.UpdateBullets(currentLoadedWeapon.GetAmmoCount());
        }
    }
}
