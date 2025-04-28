using UnityEngine;

public class GoonAnimationScript : MonoBehaviour
{
    [SerializeField] private Sprite[] goonGunDeathSprites;
    [SerializeField] private Sprite[] goonBladeDeathSprites;
    public void OnAttackAnimationEnd()
    {
        // Disable animator
        gameObject.GetComponentInChildren<Animator>().enabled = false;
    }

    public void SetGoonDeadSprite(int weaponType){
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
