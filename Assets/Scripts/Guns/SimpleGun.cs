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
    private GameObject playerObject;

    public void Setup()
    {
        this.ammoCount = this.magCap;
    }

    void Start(){
        if (equippedSprite == null){
            throw new NullReferenceException("EQUIPPED SPRITE FOR WEAPON" + this.ToString() + " IS NULL");
        }

        if (bulletPrefab == null){
            throw new NullReferenceException("BULLET PREFAB FOR WEAPON " + this.ToString() + " IS NULL");
        }

        playerObject = GameObject.FindGameObjectWithTag(Utils.Const.PLAYER_TAG);

        if (playerObject == null){
            throw new NullReferenceException("PLAYER OBJECT IS NULL IN CONCRETE GUN COMPONENT");
        }
    }

    public void Shoot(){
        ammoCount -= 1;
        GameObject bullet = Instantiate(this.bulletPrefab, playerObject.transform.position, Quaternion.identity); // shoot passing the player position!
        SingleBulletScript bulletScript = bullet.GetComponent<SingleBulletScript>();
        bulletScript.Initialize(playerObject.layer); // Pass the shooter's layer to avoid autocollision, this is useful to use the same script for the enemies too and avoids the autocollision with the player when the shooter is the player and not an enemy.
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
