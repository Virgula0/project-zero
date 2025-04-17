using UnityEngine;
using System;
using System.Collections.Generic;

public class EnemyWeaponManager : MonoBehaviour
{
    [SerializeField] private IGun currentLoadedWeapon; // an enemy could have a gun at the beginning. If it does not have it it will start to search one.
    private float timer; // timer counts the timer elapsed from the last shot, in seconds
    [SerializeField] SpriteRenderer enemySpriteRenderer;
    private Sprite defaultEnemySprite;
    private bool isEnemyAlerted = false;
    private bool needsToFindAWeapon = false;
    private List<Type> weaponTypesThatCanBeEquipped;

    public void ChangeEnemyStatus(bool status)
    {
        this.isEnemyAlerted = status;
    }

    public bool NeedsToFindAWeapon()
    {
        return needsToFindAWeapon;
    }

    public void SetWeaponThatCanBeEquipped( List<Type> list){
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
        needsToFindAWeapon = false; // enemy do not needs to find a weapon anymore

        // we're allowed to shoot at te beginning 
        timer = float.PositiveInfinity;
        currentLoadedWeapon.Setup(shooter);
        // playerSpriteRenderer.sprite = weapon.GetEquippedSprite();
    }

    private void UnloadCurrentGun()
    {
        if (currentLoadedWeapon == null)
        {
            throw new NullReferenceException("ENEMY GUN CANNOT BE DELOADED IF NO ONE HAS BEEN LOADED");
        }

        Debug.Log("Enemy deloaded a weapon");
        currentLoadedWeapon = null;
        timer = 0;
        // playerSpriteRenderer.sprite = defaultPlayerSprite;
    }

    void Start()
    {
        // this.defaultPlayerSprite = playerSpriteRenderer.sprite;
    }

    // Update is called once per frame
    void Update()
    {
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
            return;
        }

        if (timer >= currentLoadedWeapon.GetFireRate() && currentLoadedWeapon.GetAmmoCount() > 0)
        {
            timer = 0;
            currentLoadedWeapon.Shoot();
            return;
        }

        timer += Time.deltaTime;
    }
}
