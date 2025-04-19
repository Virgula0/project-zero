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

    [SerializeField] SpriteRenderer playerSpriteRenderer;
    [SerializeField] Canvas ui;
    [SerializeField] AudioSource audioSrc;

    void Start()
    {
        this.defaultPlayerSprite = playerSpriteRenderer.sprite;
        this.uiManager = ui.GetComponent<UIManager>();
        ResizePlayerCollider();
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

        if (currentLoadedWeapon != null){ 
            UnloadCurrentGun();
        }

        // must be done whatever a new gun gets loaded
        currentLoadedWeapon = weapon;

        // we're allowed to shoot at te beginning 
        timer = float.PositiveInfinity;
        currentLoadedWeapon.Setup(shooter);
        audioSrc.PlayOneShot(currentLoadedWeapon.GetEquipSfx());
        playerSpriteRenderer.sprite = weapon.GetEquippedSprite();

        ResizePlayerCollider();

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
        ResizePlayerCollider();
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

        if (Input.GetKeyDown(KeyCode.R) && currentLoadedWeapon.GetNumberOfReloads() > 0 
            && currentLoadedWeapon.GetAmmoCount() < currentLoadedWeapon.GetMegCap() && !isReloading)
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
            currentLoadedWeapon.GetAmmoCount() > 0 && !isReloading)
        {
            timer = 0;
            currentLoadedWeapon.Shoot();
            audioSrc.PlayOneShot(currentLoadedWeapon.GetShotSfx());
            uiManager.UpdateBullets(currentLoadedWeapon.GetAmmoCount());
        }
    }

    public void ResizePlayerCollider(){
        BoxCollider2D playerCollider = gameObject.GetComponentInParent<BoxCollider2D>();
        Vector2 spriteSize = playerSpriteRenderer.sprite.bounds.size;
        //Vector3 spriteScale = gameObject.transform.parent.localScale;
        Vector3 spriteScale = GameObject.FindGameObjectWithTag("PlayerTag").GetComponentInChildren<SpriteRenderer>().transform.localScale;
        Vector2 scaledSize = new Vector2(spriteSize.x * spriteScale.x, spriteSize.y * spriteScale.y); // Multiply the sprite size by the parent’s scale
        playerCollider.size = scaledSize;
    }

    IEnumerator WaitForSfxToEnd(){

        while(audioSrc.isPlaying){
            yield return null;
        }

        isReloading = false;
    }
}
