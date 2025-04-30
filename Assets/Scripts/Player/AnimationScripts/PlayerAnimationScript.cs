using UnityEngine;

public class PlayerAnimationScript : MonoBehaviour
{
    [SerializeField] private Sprite[] playerGunDeadSprites;
    [SerializeField] private Sprite[] playerBladeDeadSprites;
    [SerializeField] private Sprite playerDefaultSprite;

    private Animator animatorRef;
    private SpriteRenderer spriteRendererRef;
    private Sprite idleSwordSprite;
    private bool isAnimationScriptReady = false;

    void Start()
    {
        animatorRef = GetComponent<Animator>();
        spriteRendererRef = GetComponent<SpriteRenderer>();
        isAnimationScriptReady = true;
    }

    public bool IsAnimationScriptReady() => isAnimationScriptReady;

    public void OnAttackAnimationEnd()
    {
        // Disable animator
        gameObject.GetComponentInChildren<Animator>().enabled = false;
        SetEquippedWeponSprite(idleSwordSprite);
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
