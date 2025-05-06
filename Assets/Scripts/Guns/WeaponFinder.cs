using UnityEngine;

/* This script must be a component of each different weapon */
public class WeaponFinder : MonoBehaviour
{
    private WeaponManager playerManager;
    private IGun weapon;
    private GameObject gameObjectRef;
    private WeaponSpawner spawner;

    // Better to use awake since it is called before Start
    void Awake()
    {
        this.playerManager = GameObject.FindGameObjectWithTag(Utils.Const.WEAPON_MANAGER_TAG).GetComponent<WeaponManager>();
        this.weapon = GetComponentInParent<IGun>(); // in parent, the concrete script of the gun which implements Igun must be present
        this.gameObjectRef = transform.parent.gameObject;
        this.spawner = GameObject.FindGameObjectWithTag(Utils.Const.WEAPON_SPAWNER_TAG).GetComponent<WeaponSpawner>();
    }

    // In Player rigidbody -> continuous detections must be set for detecing mouse interactively
    // use stay instead of enter for this goal
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (weapon == null)
        {
            return;
        }

        if (weapon.IsGoingToBePickedUp())
        {
            return;
        }

        var obj = collision.gameObject;

        switch (obj.layer)
        {
            case (int)Utils.Enums.ObjectLayers.Player:
                if (!Input.GetMouseButton((int)Utils.Enums.MouseButtons.RightButton))
                {
                    return;
                }
                weapon.SetIsGoingToBePickedUp(true);
                Debug.Log("You got a weapon! " + gameObject.name);
                playerManager.LoadNewGun(weapon, obj, this.gameObjectRef);
                HandleWeaponPickup();
                break;
            case (int)Utils.Enums.ObjectLayers.Enemy:
                EnemyWeaponManager manager = obj.transform.parent.GetComponentInChildren<EnemyWeaponManager>();
                if (!manager.CanWeaponBeEquipped(weapon))
                {
                    Debug.Log("This enemy cannot equip this type of weapon or is dead");
                    break;
                }
                weapon.SetIsGoingToBePickedUp(true);
                Debug.Log("Enemy got a weapon! " + gameObject.name);
                manager.LoadNewGun(weapon, obj);
                HandleWeaponPickup();
                break;
        }
    }

    private void HandleWeaponPickup()
    {
        // Destroy(this.gameObjectRef);
        gameObjectRef.SetActive(false);
        if (weapon is IRestricted pick &&
            !pick.IsEquippableByPlayerOnly() &&
            !spawner.RemoveAGunFromTheGroundPosition(gameObject.transform.position))
        {
            Debug.LogWarning("An element should have been removed and it was not!");
        }
    }
}
