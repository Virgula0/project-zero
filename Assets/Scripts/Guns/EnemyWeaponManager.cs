using UnityEngine;
using System;

public class EnemyWeaponManager : MonoBehaviour
{
    [SerializeField] private IGun currentLoadedWeapon; // an enemy will have always a gun at the beginning
    private float timer; // timer counts the timer elapsed from the last shot, in seconds
    [SerializeField] SpriteRenderer enemySpriteRenderer;

    private Sprite defaultEnemySprite;

    private bool isEnemyAlerted = false; 

    public void changeEnemyStatus(bool status){
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

        // we're allowed to shoot at te beginning 
        timer = float.PositiveInfinity;
        currentLoadedWeapon.Setup(shooter);
        // playerSpriteRenderer.sprite = weapon.GetEquippedSprite();
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
        // playerSpriteRenderer.sprite = defaultPlayerSprite;
    }

    void Start()
    {
        // this.defaultPlayerSprite = playerSpriteRenderer.sprite;
    }

    // Update is called once per frame
    void Update()
    {
        if (currentLoadedWeapon == null)
        {
            // TODO: Implement logic for finding a new gun
            return;
        }

        timer += Time.deltaTime;

        /* TODO: implement enemy weapon logic here */
        currentLoadedWeapon.Shoot();
        UnloadCurrentGun();
        return;

    }
}
