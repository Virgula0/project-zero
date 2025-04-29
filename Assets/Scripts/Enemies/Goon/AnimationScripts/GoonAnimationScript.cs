using UnityEngine;

public class GoonAnimationScript : MonoBehaviour
{
    [SerializeField] private Sprite[] goonGunDeathSprites;
    [SerializeField] private Sprite[] goonBladeDeathSprites;

    private Animator animatorRef;

    void Start()
    {
        animatorRef = GetComponent<Animator>();
    }
    public void OnAttackAnimationEnd()
    {
        // Disable animator
        gameObject.GetComponentInChildren<Animator>().enabled = false;
    }

    public void SetGoonDeadSprite(int weaponType){
        if(animatorRef.GetCurrentAnimatorStateInfo(0).IsName(Utils.Animations.ENEMY_SWORD_ATTACK)){
                animatorRef.enabled = false;
        }
        if (weaponType == 0){
            int randomInt = UnityEngine.Random.Range(0, goonGunDeathSprites.Length);
            transform.parent.GetComponentInChildren<SpriteRenderer>().sprite = goonGunDeathSprites[randomInt]; 
        }
        if (weaponType == 1){
            int randomInt = UnityEngine.Random.Range(0, goonBladeDeathSprites.Length);
            transform.parent.GetComponentInChildren<SpriteRenderer>().sprite = goonBladeDeathSprites[randomInt];
        }
    }
}
