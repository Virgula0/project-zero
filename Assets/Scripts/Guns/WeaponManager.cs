using System;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    private IGun currentLoadedWeapon;
    private bool messageSpawned = false;
    private float timer; // timer counts the timer elapsed from the last shot, in seconds

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // this will be invoked externally
    public void LoadNewGun(IGun weapon){
        if (weapon == null){
            throw new NullReferenceException("GUN LOAD CANNOT BE NULL, THE PASSED REFERENCE TO WEAPON MANAGER IS NULL");
        }

        // must be done whatever a new gun gets loaded
        this.currentLoadedWeapon = weapon;
        
        // we're allowed to shoot at te beginning 
        this.timer = float.PositiveInfinity; 
        this.currentLoadedWeapon.Setup();
    }

    // this will be invoked externally
    public void UnloadCurrentGun(){
        if (this.currentLoadedWeapon == null){
            throw new NullReferenceException("GUN CANNOT BE DELOADED IF NO ONE HAS BEEN LOADED");
        }

        this.currentLoadedWeapon = null;
        this.timer = 0;
    }

    // Update is called once per frame
    void Update()
    {
         // we do nothing if we do not have a loaded weapon already
        if (this.currentLoadedWeapon == null){
            return;
        }

        if (Input.GetKeyDown(KeyCode.R) && this.currentLoadedWeapon.GetNumberOfReloads() > 0){
            this.currentLoadedWeapon.Reload();
        }

        if (Input.GetMouseButton(0) && 
            (this.timer >= this.currentLoadedWeapon.GetFireRate()) && 
            this.currentLoadedWeapon.GetAmmoCount() > 0)
        {
            this.timer = 0;
            this.currentLoadedWeapon.Shoot();
        }else if (this.currentLoadedWeapon.GetAmmoCount() <= 0 && !messageSpawned){ // for debugging purposes, this else statement can be removed later
            Debug.Log("No ammo, need to reload, press R!");
            messageSpawned = true; // avoid spam on console
        }
        
        timer += Time.deltaTime;
    }
}
