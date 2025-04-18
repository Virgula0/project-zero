using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections;

public class EnemyWeaponManager : MonoBehaviour
{
    [Tooltip("Drag gun prefab here for already equipped enemy")]
    [SerializeField] private GameObject weaponTemplatePrefab;
    [SerializeField] AudioSource audioSrc;
    [SerializeField] SpriteRenderer enemySpriteRenderer;

    private IGun currentLoadedWeapon; // an enemy could have a gun at the beginning. If it does not have it it will start to search one.
    private float timer; // timer counts the timer elapsed from the last shot, in seconds
    private Sprite defaultEnemySprite;
    private bool isEnemyAlerted = false;
    private bool needsToFindAWeapon = false;
    private List<Type> weaponTypesThatCanBeEquipped;
    private bool isReloading = false;
    private bool needsToPLayOnLoad = true; // this avoid to play the equip sound when scene start on already equipped weapons

    public void ChangeEnemyStatus(bool status)
    {
        this.isEnemyAlerted = status;
    }

    public bool NeedsToFindAWeapon()
    {
        return needsToFindAWeapon;
    }

    public void SetWeaponThatCanBeEquipped(List<Type> list)
    {
        this.weaponTypesThatCanBeEquipped = list;
    }

    public bool CanWeaponBeEquipped(object weapon)
    {
        if (weaponTypesThatCanBeEquipped == null || weapon == null)
        {
            return false;
        }

        Type weaponType = weapon.GetType();
        foreach (Type type in weaponTypesThatCanBeEquipped)
        {
            if (type.IsAssignableFrom(weaponType))
            {
                return true;
            }
        }

        return false;
    }

    // this will be invoked externally
    public void LoadNewGun(IGun weapon, GameObject shooter)
    {
        if (weapon == null)
        {
            throw new NullReferenceException("ENEMY LOAD CANNOT BE NULL, THE PASSED REFERENCE TO WEAPON MANAGER IS NULL");
        }

        /*
        if(playerSpriteRenderer == null){
           throw new NullReferenceException("ENEMY SPRITE RENDERER CANNOT BE NULL, THE PASSED REFERENCE OF THE PLAYER SPRITE RENDERER IS NULL"); 
        }
        */

        Debug.Log("Enemy loaded a weapon");
        // must be done whatever a new gun gets loaded
        currentLoadedWeapon = weapon;

        // we're allowed to shoot at te beginning 
        timer = float.PositiveInfinity;
        currentLoadedWeapon.Setup(shooter);
        needsToFindAWeapon = false; // enemy do not needs to find a weapon anymore
        if (needsToPLayOnLoad) {
            audioSrc.PlayOneShot(currentLoadedWeapon.GetEquipSfx());
        }
        needsToPLayOnLoad = true;
        // playerSpriteRenderer.sprite = weapon.GetEquippedSprite();
    }

    private void UnloadCurrentGun()
    {
        if (currentLoadedWeapon == null)
        {
            throw new NullReferenceException("ENEMY GUN CANNOT BE DELOADED IF NO ONE HAS BEEN LOADED");
        }

        Debug.Log("Enemy deloaded a weapon");
        audioSrc.PlayOneShot(currentLoadedWeapon.GetEquipSfx());
        currentLoadedWeapon = null;
        timer = 0;
        // playerSpriteRenderer.sprite = defaultPlayerSprite;
    }

    void Start()
    {
        // if prefab is not null enemy will spawn with an already equipped weapon
        if (weaponTemplatePrefab != null){
            // 1) Find the prefab’s MonoBehaviour that implements IGun
            var templateMono = weaponTemplatePrefab
                .GetComponents<MonoBehaviour>()
                .FirstOrDefault(mb => mb is IGun);

            if (templateMono == null)
                throw new InvalidOperationException(
                    $"Prefab {weaponTemplatePrefab.name} has no component implementing IGun");

            // 2) Add a new empty component of that exact type to the enemy
            var compType = templateMono.GetType();
            var newMono  = (MonoBehaviour)gameObject.AddComponent(compType);

            // 3) Copy *all* serialized data via JsonUtility
            string json = JsonUtility.ToJson(templateMono);
            JsonUtility.FromJsonOverwrite(json, newMono);

            // 4) Cast back to IGun and finish setup
            var newGun = newMono as IGun;
            if (newGun == null)
                throw new InvalidCastException($"Added component {compType.Name} doesn’t implement IGun?");

            GameObject enemyObj = transform.parent.gameObject.GetComponentInChildren<Rigidbody2D>().gameObject;
            LoadNewGun(newGun, enemyObj);

            needsToPLayOnLoad = false;
        }
        // this.defaultPlayerSprite = playerSpriteRenderer.sprite;
    }

    // Update is called once per frame
    void Update()
    {

        if (isReloading)
        {
            return;
        }

        if (currentLoadedWeapon == null)
        {
            needsToFindAWeapon = true; // the enemy needs a gun!
            return;
        }

        if (!isEnemyAlerted)
        {
            timer = 0;
            return;
        }

        if (currentLoadedWeapon.GetNumberOfReloads() < 1 && currentLoadedWeapon.GetAmmoCount() < 1)
        {
            UnloadCurrentGun();
            return;
        }

        if (currentLoadedWeapon.GetNumberOfReloads() > 0 && currentLoadedWeapon.GetAmmoCount() < 1)
        {
            currentLoadedWeapon.Reload();
            audioSrc.PlayOneShot(currentLoadedWeapon.GetReloadSfx());
            isReloading = true;
            StartCoroutine(WaitForSfxToEnd());
            return;
        }

        if (timer >= currentLoadedWeapon.GetFireRate() && currentLoadedWeapon.GetAmmoCount() > 0)
        {
            timer = 0;
            currentLoadedWeapon.Shoot();
            audioSrc.PlayOneShot(currentLoadedWeapon.GetShotSfx());
            return;
        }

        timer += Time.deltaTime;
    }

    IEnumerator WaitForSfxToEnd(){

        while(audioSrc.isPlaying){
            yield return null;
        }

        isReloading = false;
    }
}
