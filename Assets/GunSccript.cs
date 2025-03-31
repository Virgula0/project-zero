using UnityEngine;

public class GunSccript : MonoBehaviour
{
    [SerializeField] private float fireRate = 0.5f; // in seconds, 0.5 seconds between each shot
    [SerializeField] private int magCap = 10;
    [SerializeField] private float bulletSpeed;
    [SerializeField] private float spread;

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
        if(Input.GetKeyDown(KeyCode.R)){
            Reload();
            return;
        }

        if(Input.GetMouseButton(0) && (timer >= fireRate) && ammoCount > 0){
            Shoot();
        }else if (ammoCount <= 0 && !messageSpawned){ // for debugging purposes, this else statement can be removed later
            Debug.Log("No ammo, need to reload, press R!");
            messageSpawned = true; // avoid spam on console
        }
        
        timer += Time.deltaTime;
    }

    private void Shoot(){
        timer = 0;
        ammoCount -= 1;
        Debug.Log("BAM!");
        Debug.Log(ammoCount + "/" + magCap);
    }

    private void Reload(){
        if (this.ammoCount == this.magCap){
            Debug.Log("Already loaded");
            return;
        }
        Debug.Log("Reloaded");
        messageSpawned = false;
        this.ammoCount = this.magCap;
    }
}
