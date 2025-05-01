using UnityEngine;

/* This script must be a component of each different weapon */
public class SecondaryFinder : MonoBehaviour
{
    private WeaponManager playerManager;
    private ISecondary secondary;
    private GameObject gameObjectRef;

    // Better to use awake since it is called before Start
    void Awake()
    {
        this.playerManager = GameObject.FindGameObjectWithTag(Utils.Const.WEAPON_MANAGER_TAG).GetComponent<WeaponManager>();
        this.secondary = GetComponentInParent<ISecondary>(); // in parent, the concrete script of the gun which implements ISecondary must be present
        this.gameObjectRef = transform.parent.gameObject;
    }

    // In Player rigidbody -> continuous detections must be set for detecing mouse interactively
    // use stay instead of enter for this goal
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (secondary == null)
        {
            return;
        }

        var obj = collision.gameObject;

        if(obj.layer == (int)Utils.Enums.ObjectLayers.Player)
        {
            if (!Input.GetKeyDown(KeyCode.Q))
            {
                return;
            }
            Debug.Log("You got a secondary! " + gameObject.name);
            playerManager.LoadNewSecondary(secondary, this.gameObjectRef, obj);
            HandleSecondaryPickup();
        }
    }

    private void HandleSecondaryPickup()
    {
        // Destroy(this.gameObjectRef);
        gameObjectRef.SetActive(false);
    }
}
