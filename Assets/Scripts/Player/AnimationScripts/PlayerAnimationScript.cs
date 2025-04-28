using UnityEngine;

public class PlayerAnimationScript : MonoBehaviour
{
    [SerializeField] private Sprite[] playerGunDeadSprites;
    [SerializeField] private Sprite[] playerBladeDeadSprites;
    public void OnAttackAnimationEnd()
    {
        // Disable animator
        gameObject.GetComponentInChildren<Animator>().enabled = false;
    }

    public void SetPlayerDeadSprite(int weaponType){
        if(weaponType == 0){
            int randomInt = Random.Range(0, playerGunDeadSprites.Length);
            gameObject.GetComponent<SpriteRenderer>().sprite = playerGunDeadSprites[randomInt]; 
        }
        if(weaponType == 1){
            int randomInt = Random.Range(0, playerBladeDeadSprites.Length);
            gameObject.GetComponent<SpriteRenderer>().sprite = playerBladeDeadSprites[randomInt];
        }
    }
}
