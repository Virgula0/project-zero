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
    private Sprite playerLastSprite;

    void Start()
    {
        animatorRef = GetComponent<Animator>();
        spriteRendererRef = GetComponent<SpriteRenderer>();
        isAnimationScriptReady = true;
        playerCameraRef = Camera.main;
        playerRef = GameObject.FindGameObjectWithTag(Utils.Const.PLAYER_TAG);
        legsScriptRef = transform.parent.GetComponentInChildren<PlayerLegsAnimationScript>();
    }

    public bool IsAnimationScriptReady() => isAnimationScriptReady;

    public void OnAttackAnimationEnd()
    {
        // Disable animator
        gameObject.GetComponentInChildren<Animator>().enabled = false;
        SetEquippedWeponSprite(idleSwordSprite);
    }

    public void OnTeleportInAnimationEnd(){
        legsScriptRef.SetIsTeleporting(true);
        Vector2 mousePosition = playerCameraRef.ScreenToWorldPoint(Input.mousePosition);
        playerRef.GetComponent<Rigidbody2D>().position = mousePosition;
        animatorRef.SetTrigger("teleport_end");
    }

    public void OnTeleportOutEnd(){
        if(playerLastSprite != null){
            spriteRendererRef.sprite = playerLastSprite;
            playerLastSprite = null;
        }
        animatorRef.enabled = false;
        legsScriptRef.SetIsTeleporting(false);
    }

    public void SetPlayerDeadSprite(IGun weapon)
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

    public void SetPlayerDeadSpriteAmbient()
    {
        if (animatorRef.GetCurrentAnimatorStateInfo(0).IsName(Utils.Animations.PLAYER_SWORD_ATTACK))
        {
            animatorRef.enabled = false;
        }

        spriteRendererRef.sprite = playerGunDeadSprites[0];
    }

    public void SetPlayerLastSprite(Sprite lastSprite){
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
