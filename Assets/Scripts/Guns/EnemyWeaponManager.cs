using UnityEngine;
using System;

public class EnemyWeaponManager : MonoBehaviour
{
    [SerializeField] private IGun currentLoadedWeapon; // an enemy will have always a gun at the beginning
    private float timer; // timer counts the timer elapsed from the last shot, in seconds
    [SerializeField] SpriteRenderer enemySpriteRenderer;

    private Sprite defaultEnemySprite;
    private bool isEnemyAlerted = false;

    public void ChangeEnemyStatus(bool status){
        this.isEnemyAlerted = status;
    }

    // this will be invoked externally
    public void LoadNewGun(IGun weapon, GameObject shooter)
    {
        if (weapon == null)
        {
            throw new NullReferenceException("ENEMY LOAD CANNOT BE NULL, THE PASSED REFERENCE TO WEAPON MANAGER IS NULL");
        }

        Debug.Log("Enemy loaded a weapon");

        /*
        if(playerSpriteRenderer == null){
           throw new NullReferenceException("ENEMY SPRITE RENDERER CANNOT BE NULL, THE PASSED REFERENCE OF THE PLAYER SPRITE RENDERER IS NULL"); 
        }
        */

        // must be done whatever a new gun gets loaded
        currentLoadedWeapon = weapon;
        enemySpriteRenderer.sprite = weapon.GetEquippedSprite();     

        // we're allowed to shoot at te beginning 
        timer = float.PositiveInfinity;
        currentLoadedWeapon.Setup(shooter);
        Debug.Log("Should Resize");
        ResizeEnemyCollider();
    }

    private void UnloadCurrentGun()
    {
        if (currentLoadedWeapon == null)
        {
            throw new NullReferenceException("ENEMY GUN CANNOT BE DELOADED IF NO ONE HAS BEEN LOADED");
        }

        Debug.Log("Enemy deloaded a weapon");
        currentLoadedWeapon = null;
        timer = 0;
        enemySpriteRenderer.sprite = defaultEnemySprite;
        ResizeEnemyCollider();
    }

    void Start()
    {
        this.defaultEnemySprite = enemySpriteRenderer.sprite;
        ResizeEnemyCollider();
    }

    // Update is called once per frame
    void Update()
    {
        if (currentLoadedWeapon == null)
        {
            // TODO: Implement logic for finding a new gun
            return;
        }

        if (!isEnemyAlerted){
            timer = 0;
            return;
        }

        if (currentLoadedWeapon.GetNumberOfReloads() < 1 && currentLoadedWeapon.GetAmmoCount() < 1){
            UnloadCurrentGun();
            return;
        }

        if (currentLoadedWeapon.GetNumberOfReloads() > 0 && currentLoadedWeapon.GetAmmoCount() < 1)
        {
            currentLoadedWeapon.Reload();
            return;
        }

        if (timer >= currentLoadedWeapon.GetFireRate() && currentLoadedWeapon.GetAmmoCount() > 0)
        {
            timer = 0;
            currentLoadedWeapon.Shoot();
            return;
        }

        timer += Time.deltaTime;
    }

    public void ResizeEnemyCollider(){
        BoxCollider2D enemyCollider = gameObject.transform.parent.GetComponentInChildren<BoxCollider2D>();
        Vector2 spriteSize = enemySpriteRenderer.sprite.bounds.size;
        enemyCollider.size = spriteSize;
    }
}
