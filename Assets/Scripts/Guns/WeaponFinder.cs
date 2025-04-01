using UnityEngine;

public class WeaponFinder : MonoBehaviour
{
    private WeaponManager mng;
    private IGun weapon;
    private GameObject gameObjectRef;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        this.mng = GameObject.FindGameObjectWithTag("WeaponManager").GetComponent<WeaponManager>();
        this.weapon = GetComponentInParent<SimpleGun>();
        this.gameObjectRef = transform.parent.gameObject;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Collision detected with: " + collision.gameObject.name);
        if (collision.gameObject.layer == (int)Utils.Enums.ObjectLayers.Player)
        {
            Debug.Log("You got a weapon!");
            mng.LoadNewGun(weapon);
            Destroy(this.gameObjectRef);
        }
    }
}
