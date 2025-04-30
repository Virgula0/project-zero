using NUnit.Framework;
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

    public void SetPlayerDeadSprite(int weaponType){
        if(animatorRef.GetCurrentAnimatorStateInfo(0).IsName(Utils.Animations.PLAYER_SWORD_ATTACK)){
                animatorRef.enabled = false;
        }
        if(weaponType == 0){
            int randomInt = Random.Range(0, playerGunDeadSprites.Length);
            spriteRendererRef.sprite = playerGunDeadSprites[randomInt]; 
        }
        if(weaponType == 1){
            int randomInt = Random.Range(0, playerBladeDeadSprites.Length);
            spriteRendererRef.sprite = playerBladeDeadSprites[randomInt];
        }
    }

    public void SetDefaultSprite(){
        spriteRendererRef.sprite = playerDefaultSprite;
    }

    public void SetEquippedWeponSprite(Sprite newSprite){
        spriteRendererRef.sprite = newSprite;
        idleSwordSprite = newSprite;
    }

    public Vector2 GetSpriteSize(){
        return spriteRendererRef.sprite.bounds.size;
    }

    public Vector3 GetSpriteScale(){
        return spriteRendererRef.transform.localScale;
    }
}
