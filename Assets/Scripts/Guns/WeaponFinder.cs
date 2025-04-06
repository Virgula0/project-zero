using UnityEngine;

/* This script must be a component of each different weapon */
public class WeaponFinder : MonoBehaviour
{
    private WeaponManager playerManager;
    private IGun weapon;
    private GameObject gameObjectRef;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        this.playerManager = GameObject.FindGameObjectWithTag(Utils.Const.WEAPON_MANAGER_TAG).GetComponent<WeaponManager>();
        this.weapon = GetComponentInParent<IGun>(); // in parent, the concrete script of the gun which implements Igun must be present
        this.gameObjectRef = transform.parent.gameObject;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Collision detected with: " + collision.gameObject.name);
        switch (collision.gameObject.layer)
        {
            case (int)Utils.Enums.ObjectLayers.Player:
                Debug.Log("You got a weapon! " + gameObject.name);
                playerManager.LoadNewGun(weapon, collision.gameObject);
                Destroy(this.gameObjectRef);
                break;
            case (int)Utils.Enums.ObjectLayers.Enemy:
                // get the manager of the enemy which callided with the gun
                EnemyWeaponManager manager = collision.gameObject.transform.parent.GetComponentInChildren<EnemyWeaponManager>();
                Debug.Log("Enemy got a weapon! " + gameObject.name);
                manager.LoadNewGun(weapon, collision.gameObject);
                Destroy(this.gameObjectRef);
                break;
        }
    }
}
