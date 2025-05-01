using UnityEngine;

public class TeleportScript : MonoBehaviour, ISecondary
{
    private readonly float fireRate = 0.5f; // in seconds, 0.5 seconds between each shot
    private readonly int chargesCap = 3;
    private int currentCharges;
    private GameObject palyerRef;
    private Camera playerCameraRef;
    
    [SerializeField] private Sprite staticSecSprite;
    [SerializeField] private AudioClip shotSound;
    [SerializeField] private AudioClip readySound;
    [SerializeField] private AudioClip equipSound;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        palyerRef = GameObject.FindGameObjectWithTag(Utils.Const.PLAYER_TAG);
        playerCameraRef = Camera.main;
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
        return;
    }

    public void Setup(GameObject player)
    {
        this.palyerRef = player;
        this.currentCharges = this.chargesCap;
    }

    public void Shoot()
    {
        Vector2 mousePosition = playerCameraRef.ScreenToWorldPoint(Input.mousePosition);
        Debug.Log("CAM POS: " + mousePosition);
        palyerRef.GetComponent<Rigidbody2D>().position = mousePosition;
        currentCharges -= 1;
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
