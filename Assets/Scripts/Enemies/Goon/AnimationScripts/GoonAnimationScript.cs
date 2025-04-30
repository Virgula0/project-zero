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

    public void SetGoonDeadSprite(IGun weapon)
    {
        if (animatorRef.GetCurrentAnimatorStateInfo(0).IsName(Utils.Animations.ENEMY_SWORD_ATTACK))
        {
            animatorRef.enabled = false;
        }

        Sprite[] chosenSprites = weapon is IRanged ? goonGunDeathSprites :
                                weapon is IMelee ? goonBladeDeathSprites : null;

        if (chosenSprites == null)
        {
            Debug.LogError("Chosen sprites is null. The weapon seems to not have an expected identity");
            return;
        }

        int randomInt = Random.Range(0, chosenSprites.Length);
        spriteRenderer.sprite = chosenSprites[randomInt];
    }
}
