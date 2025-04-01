using System;
using UnityEngine;

public class GunScript : MonoBehaviour , IGun
{
    [SerializeField] private float fireRate = 0.5f; // in seconds, 0.5 seconds between each shot
    [SerializeField] private int magCap = 10;
    [SerializeField] private int numberOfReloads = 5; // total bullets available can be seen as numberOfReloads*magCap

    private float bulletSpeed;
    private float spread;
    
    private int ammoCount;
    private float timer; // timer counts the timer elapsed from the last shot, in seconds
    private bool messageSpawned = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        this.timer = this.fireRate;
        this.ammoCount = this.magCap;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.R) && this.numberOfReloads > 0){
            Reload();
        }

        if(Input.GetMouseButton(0) && (timer >= fireRate) && ammoCount > 0){
            Shoot();
        }else if (ammoCount <= 0 && !messageSpawned){ // for debugging purposes, this else statement can be removed later
            Debug.Log("No ammo, need to reload, press R!");
            messageSpawned = true; // avoid spam on console
        }
        
        timer += Time.deltaTime;
    }

    public void Shoot(){
        timer = 0;
        ammoCount -= 1;
        Debug.Log("BAM!");
        Debug.Log(ammoCount + "/" + magCap);
    }

    public void Reload(){
        if (this.ammoCount == this.magCap){
            Debug.Log("Already loaded");
            return;
        }
        Debug.Log("Reloaded");
        Debug.Log($"Number of reloads available -> {--this.numberOfReloads}");
        messageSpawned = false;
        this.ammoCount = this.magCap;
    }

}
