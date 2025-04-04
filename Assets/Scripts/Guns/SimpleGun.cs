using System;
using Unity.VisualScripting;
using UnityEngine;

public class SimpleGun : MonoBehaviour , IGun
{
    private readonly float fireRate = 0.5f; // in seconds, 0.5 seconds between each shot
    private readonly int magCap = 10;
    private int numberOfReloads = 5; // total bullets available can be seen as numberOfReloads*magCap
    private int ammoCount;

    [SerializeField] private Sprite equippedSprite;
    [SerializeField] private GameObject bulletPrefab;

    public void Setup()
    {
        if (equippedSprite == null){
            throw new NullReferenceException("EQUIPPED SPRITE FOR WEAPON" + this.ToString() + " IS NULL");
        }

        if (bulletPrefab == null){
            throw new NullReferenceException("BULLET PREFAB FOR WEAPON " + this.ToString() + " IS NULL");
        }
        
        this.ammoCount = this.magCap;
    }

    public void Shoot(){
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

    public Sprite GetEquippedSprite(){
        return this.equippedSprite;
    }
}
