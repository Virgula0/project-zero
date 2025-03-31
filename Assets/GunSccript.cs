using UnityEngine;

public class GunSccript : MonoBehaviour
{
    [SerializeField] private float fireRate;
    [SerializeField] private int magCap;
    [SerializeField] private float bulletSpeed;
    [SerializeField] private float spread;

    private int ammoCount;
    private float timer = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        this.ammoCount = this.magCap;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.R)){
            Debug.Log("DENTRO R");
            Reload();
        }
        if(Input.GetMouseButton(0) && timer <= 0){
            Shoot();
        }
        timer -= Time.deltaTime;
    }

    private void Shoot(){
        if(ammoCount < 1){
            Debug.Log("YOU NEED TO RELOAD! PRESS R!");
            return;
        }
        ammoCount -= 1;
        Debug.Log("BAM!");
        Debug.Log(ammoCount + "/" + magCap);

    }

    private void Reload(){
        timer = fireRate;
        this.ammoCount = this.magCap;
    }
}
