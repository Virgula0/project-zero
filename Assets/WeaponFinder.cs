using UnityEngine;

public class WeaponFinder : MonoBehaviour
{
    private WeaponManager mng;
    private IGun weapon;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        this.mng = GameObject.FindGameObjectWithTag("WeaponManager").GetComponent<WeaponManager>();
        this.weapon = GetComponentInParent<GunScript>();
    }

    private void OnTriggerEnter2D(Collider2D collision){
        Debug.Log("oeeee - Collision detected with: " + collision.gameObject.name);
        if (collision.gameObject.layer == (int)Utils.Enums.ObjectLayers.Player){ 
            Debug.Log("You got a weapon!");
            mng.LoadNewGun(weapon);
        }
    }
}
