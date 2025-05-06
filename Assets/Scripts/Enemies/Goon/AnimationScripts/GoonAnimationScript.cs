using UnityEngine;

public class GoonAnimationScript : MonoBehaviour
{
    [SerializeField] private Sprite[] goonGunDeathSprites;
    [SerializeField] private Sprite[] goonBladeDeathSprites;
    private SpriteRenderer spriteRenderer;
    private Animator animatorRef;

    void Start()
    {
        animatorRef = GetComponent<Animator>();
        spriteRenderer = transform.parent.GetComponentInChildren<SpriteRenderer>();
    }

    public void OnAttackAnimationEnd()
    {
        // Disable animator
        gameObject.GetComponentInChildren<Animator>().enabled = false;
    }

    public void SetGoonDeadSprite(IPrimary weapon)
    {
        if (animatorRef.GetCurrentAnimatorStateInfo(0).IsName(Utils.Animations.ENEMY_SWORD_ATTACK))
        {
            animatorRef.enabled = false;
        }

        // if weapon is null probably is because the object that hitted the enemy was not a bullet neither a swing
        // in this case, just choose a random from normal death sprites 
        
        Sprite[] chosenSprites = weapon is IRanged ? goonGunDeathSprites :
                               weapon is IMelee ? goonBladeDeathSprites : goonGunDeathSprites;

        int randomInt = Random.Range(0, chosenSprites.Length);
        spriteRenderer.sprite = chosenSprites[randomInt];
    }
}
