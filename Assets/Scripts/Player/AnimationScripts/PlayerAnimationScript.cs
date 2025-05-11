using UnityEngine;

public class PlayerAnimationScript : MonoBehaviour
{
    [SerializeField] private Sprite[] playerGunDeadSprites;
    [SerializeField] private Sprite[] playerBladeDeadSprites;
    [SerializeField] private Sprite playerDefaultSprite;

    private Animator animatorRef;
    private PlayerLegsAnimationScript legsScriptRef;
    private SpriteRenderer spriteRendererRef;
    private Sprite idleSwordSprite;
    private bool isAnimationScriptReady = false;
    private Camera playerCameraRef;
    private GameObject playerRef;
    private PlayerScript playerScript;
    private Sprite playerLastSprite;
    private Vector2 teleportTargetPosition;
    private WeaponManager weaponManagerRef;

    void Start()
    {
        animatorRef = GetComponent<Animator>();
        spriteRendererRef = GetComponent<SpriteRenderer>();
        playerCameraRef = Camera.main;
        playerRef = GameObject.FindGameObjectWithTag(Utils.Const.PLAYER_TAG);
        legsScriptRef = transform.parent.GetComponentInChildren<PlayerLegsAnimationScript>();
        playerScript = GameObject.FindGameObjectWithTag(Utils.Const.PLAYER_TAG).GetComponent<PlayerScript>();
        weaponManagerRef = transform.parent.GetComponentInChildren<WeaponManager>();
        isAnimationScriptReady = true;
    }

    public bool IsAnimationScriptReady() => isAnimationScriptReady;

    public void OnAttackAnimationEnd()
    {
        // Disable animator
        gameObject.GetComponentInChildren<Animator>().enabled = false;
        SetEquippedWeponSprite(idleSwordSprite);
    }

    public void OnTeleportInAnimationStart()
    {   
        legsScriptRef.SetIsTeleporting(true);
        weaponManagerRef.SetCanShoot(false);
        Utils.Functions.SetLayerRecursively(playerRef.gameObject, (int)Utils.Enums.ObjectLayers.Invulnerability);

        teleportTargetPosition = playerCameraRef.ScreenToWorldPoint(Input.mousePosition);
    }

    public void OnTeleportInAnimationEnd()
    {
        playerRef.GetComponent<Rigidbody2D>().position = teleportTargetPosition;
        animatorRef.SetTrigger("teleport_end");
    }

    public void OnTeleportOutEnd()
    {
        if(weaponManagerRef.GetCurrentLoadedWeapon() != null){
            if (playerLastSprite != null)
            {
                spriteRendererRef.sprite = playerLastSprite;
                playerLastSprite = null;
            }
        }

        animatorRef.enabled = false;

        if (!playerScript.IsPlayerAlive())
        {
            SetPlayerDeadSprite();
        }

        playerRef.layer = (int)Utils.Enums.ObjectLayers.Player;
        legsScriptRef.SetIsTeleporting(false);
        weaponManagerRef.SetCanShoot(true);
    }

    public void SetPlayerDeadSprite(IPrimary weapon)
    {
        if (animatorRef.GetCurrentAnimatorStateInfo(0).IsName(Utils.Animations.PLAYER_SWORD_ATTACK))
        {
            animatorRef.enabled = false;
        }

        Sprite[] chosenSprites = weapon is IRanged ? playerGunDeadSprites :
                        weapon is IMelee ? playerBladeDeadSprites : null;

        if (chosenSprites == null)
        {
            Debug.LogError("Chosen sprites is null. The weapon seems to not have an expected identity");
            return;
        }

        int randomInt = Random.Range(0, chosenSprites.Length);
        spriteRendererRef.sprite = chosenSprites[randomInt];
    }

    public void SetPlayerDeadSprite()
    {
        if (animatorRef.GetCurrentAnimatorStateInfo(0).IsName(Utils.Animations.PLAYER_SWORD_ATTACK))
        {
            animatorRef.enabled = false;
        }

        spriteRendererRef.sprite = playerGunDeadSprites[0];
    }

    public void SetPlayerLastSprite(Sprite lastSprite)
    {
        playerLastSprite = lastSprite;
    }

    public void SetDefaultSprite()
    {
        spriteRendererRef.sprite = playerDefaultSprite;
    }

    public void SetEquippedWeponSprite(Sprite newSprite)
    {
        spriteRendererRef.sprite = newSprite;
        idleSwordSprite = newSprite;
    }

    public Vector2 GetSpriteSize()
    {
        return spriteRendererRef.sprite.bounds.size;
    }

    public Vector3 GetSpriteScale()
    {
        return spriteRendererRef.transform.localScale;
    }
}
