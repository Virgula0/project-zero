using System;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    private IGun currentLoadedWeapon;
    private bool messageSpawned = false;
    private float timer; // timer counts the timer elapsed from the last shot, in seconds

    private readonly float WeaponDeloadTimeInSeconds = 10; //deload weapon after N seconds if no ammo 

    // this will be invoked externally
    public void LoadNewGun(IGun weapon)
    {
        if (weapon == null)
        {
            throw new NullReferenceException("GUN LOAD CANNOT BE NULL, THE PASSED REFERENCE TO WEAPON MANAGER IS NULL");
        }

        // must be done whatever a new gun gets loaded
        currentLoadedWeapon = weapon;

        // we're allowed to shoot at te beginning 
        timer = float.PositiveInfinity;
        currentLoadedWeapon.Setup();
    }

    private void UnloadCurrentGun()
    {
        if (currentLoadedWeapon == null)
        {
            throw new NullReferenceException("GUN CANNOT BE DELOADED IF NO ONE HAS BEEN LOADED");
        }

        Debug.Log("Weapon deloaded");
        currentLoadedWeapon = null;
        timer = 0;
    }

    // Update is called once per frame
    void Update()
    {
        // we do nothing if we do not have a loaded weapon already
        if (currentLoadedWeapon == null)
        {
            return;
        }

        // if after WeaponDeloadTimeInSeconds seconds there is a weapon equipped but without any bullet we deload it 
        if (timer >= WeaponDeloadTimeInSeconds && 
            currentLoadedWeapon.GetNumberOfReloads() * currentLoadedWeapon.GetMegCap() <= 0)
        {
            UnloadCurrentGun();
            return;
        }

        // if left button is pressed, let an user to leave the weapon
        if (Input.GetMouseButton((int)Utils.Enums.MouseButtons.RightButton))
        {
            UnloadCurrentGun();
            return;
        }

        if (Input.GetKeyDown(KeyCode.R) && currentLoadedWeapon.GetNumberOfReloads() > 0)
        {
            currentLoadedWeapon.Reload();
        }

        if (Input.GetMouseButton((int)Utils.Enums.MouseButtons.LeftButton) &&
            timer >= currentLoadedWeapon.GetFireRate() &&
            currentLoadedWeapon.GetAmmoCount() > 0)
        {
            timer = 0;
            currentLoadedWeapon.Shoot();
        }
        else if (currentLoadedWeapon.GetAmmoCount() <= 0 && !messageSpawned)
        { // for debugging purposes, this else statement can be removed later
            Debug.Log("No ammo, need to reload, press R!");
            messageSpawned = true; // avoid spam on console
        }

        timer += Time.deltaTime;
    }
}
