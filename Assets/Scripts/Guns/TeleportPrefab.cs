using UnityEngine;

public class TeleportPrefab : MonoBehaviour, ISecondary
{
    private readonly float fireRate = 1f; // in seconds, 0.5 seconds between each shot
    private readonly int chargesCap = 3;
    private int currentCharges;
    private GameObject playerRef;
    private Animator playerAnimatorRef;

    [SerializeField] private Sprite staticSecSprite;
    [SerializeField] private AudioClip shotSound;
    [SerializeField] private AudioClip readySound;
    [SerializeField] private AudioClip equipSound;
    [SerializeField] private GameObject teleportScriptPrefab;

    private GameObject currentInitPrefab;
    private TeleportScript currentInitScript;
    private GameObject legsObj;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerRef = GameObject.FindGameObjectWithTag(Utils.Const.PLAYER_TAG);
        playerAnimatorRef = playerRef.GetComponentInChildren<Animator>();
    }

    public void Setup(GameObject player)
    {
        this.playerRef = player;
        this.currentCharges = this.chargesCap;
        this.playerAnimatorRef = player.GetComponentInChildren<Animator>();
        legsObj = playerRef.transform.GetChild(1).gameObject;
        currentInitPrefab = Instantiate(teleportScriptPrefab, playerRef.transform.position, Quaternion.identity);
        currentInitScript = currentInitPrefab.GetComponent<TeleportScript>();
        currentInitScript.Initialize(legsObj);
    }

    public void Shoot()
    {
        legsObj.SetActive(false);
        playerAnimatorRef.enabled = true;
        playerAnimatorRef.SetTrigger(Utils.Animations.Triggers.TELEPORTING);
        currentInitScript.Run(playerAnimatorRef);
        this.currentCharges -= 1;
    }

    public Sprite GetEquippedSprite()
    {
        return null;
    }

    public AudioClip GetEquipSfx()
    {
        return equipSound;
    }

    public float GetFireRate()
    {
        return fireRate;
    }

    public Sprite GetGoonEquippedSprite()
    {
        return null;
    }

    public AudioClip GetReloadSfx()
    {
        return null;
    }

    public AudioClip GetShotSfx()
    {
        return shotSound;
    }

    public Sprite GetStaticWeaponSprite()
    {
        return staticSecSprite;
    }

    public void PostSetup()
    {
        Destroy(currentInitPrefab);
    }

    public void Reload()
    {
        return;
    }

    public int GetAmmoCount()
    {
        return currentCharges;
    }

    public int GetNumberOfReloads()
    {
        return 0;
    }

    public int GetMegCap()
    {
        return 0;
    }
}
