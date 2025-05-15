using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections;

public class EnemyWeaponManager : MonoBehaviour
{
    [Tooltip("Drag gun prefab here for already equipped enemy")]
    [SerializeField] private GameObject weaponTemplatePrefab;
    [SerializeField] AudioSource audioSrc;
    [SerializeField] SpriteRenderer enemySpriteRenderer;

    private IPrimary currentLoadedWeapon; // an enemy could have a gun at the beginning. If it does not have it it will start to search one.
    private float timer; // timer counts the timer elapsed from the last shot, in seconds
    private Sprite defaultEnemySprite;
    private bool isEnemyAlerted = false;
    private bool needsToFindAWeapon = false;
    private List<Type> weaponTypesThatCanBeEquipped;
    private bool isReloading = false;
    private bool needsToPLayOnLoad = true; // this avoid to play the equip sound when scene start on already equipped weapons
    private Rigidbody2D enemyBody;
    private bool isPlayerBehindAWall = false;
    private int totalShotsDelivered;
    private IEnemy enemyRef;
    private Rigidbody2D playerBody;
    private float initialDetectionTime = 0;
    private float unhideTimestamp = -Mathf.Infinity;


    void Start()
    {
        enemyBody = transform.parent.GetComponentInChildren<Rigidbody2D>();
        enemyRef = transform.parent.GetComponentInChildren<IEnemy>();
        playerBody = GameObject.FindGameObjectWithTag(Utils.Const.PLAYER_TAG).GetComponent<Rigidbody2D>();
        defaultEnemySprite = enemySpriteRenderer.sprite; // current must be the normal one
        // if prefab is not null enemy will spawn with an already equipped weapon
        if (weaponTemplatePrefab != null)
        {
            needsToPLayOnLoad = false;
            // 1) Find the prefab’s MonoBehaviour that implements IGun
            var templateMono = weaponTemplatePrefab
                .GetComponents<MonoBehaviour>()
                .FirstOrDefault(mb => mb is IPrimary);

            if (templateMono == null)
                throw new InvalidOperationException(
                    $"Prefab {weaponTemplatePrefab.name} has no component implementing IPrimary");

            // 2) Add a new empty component of that exact type to the enemy
            var compType = templateMono.GetType();
            var newMono = (MonoBehaviour)gameObject.AddComponent(compType);

            // 3) Copy *all* serialized data via JsonUtility
            string json = JsonUtility.ToJson(templateMono);
            JsonUtility.FromJsonOverwrite(json, newMono);

            // 4) Cast back to IGun and finish setup
            var newGun = newMono as IPrimary;
            if (newGun == null)
                throw new InvalidCastException($"Added component {compType.Name} doesn’t implement IGun?");

            LoadNewGun(newGun, enemyBody.gameObject);
        }
        ResizeEnemyCollider();
    }

    public IPrimary GetCurrentLoadedWeapon()
    {
        return currentLoadedWeapon;
    }


    // Update is called once per frame
    void Update()
    {
        if (isReloading)
        {
            return;
        }

        if (currentLoadedWeapon == null)
        {
            needsToFindAWeapon = true; // the enemy needs a gun!
            return;
        }

        timer += Time.deltaTime;

        if (Time.time - initialDetectionTime <= 0.5f) // the player has 0.5 seconds after the detection
        {
            return;
        }

        if (!isEnemyAlerted || isPlayerBehindAWall)
        {
            return;
        }

        if (Time.time - unhideTimestamp <= 0.5f) // if the player ws behind an obstacle the enemy has been repositioned and we give 0.5f of seconds
        {
            return;
        }

        if (currentLoadedWeapon.GetNumberOfReloads() < 1 && currentLoadedWeapon.GetAmmoCount() < 1)
        {
            UnloadCurrentGun();
            return;
        }

        if (currentLoadedWeapon is IRanged &&
            currentLoadedWeapon.GetNumberOfReloads() > 0 && currentLoadedWeapon.GetAmmoCount() < 1)
        {
            currentLoadedWeapon.Reload();
            audioSrc.PlayOneShot(currentLoadedWeapon.GetReloadSfx());
            isReloading = true;
            StartCoroutine(WaitForSfxToEnd());
            return;
        }

        if (currentLoadedWeapon is IMelee melee &&
            Vector2.Distance(enemyBody.position, playerBody.position) > melee.MinDistanceForSwing())
        {
            // we need to check if we're close enough for swinging
            return;
        }

        int beforeShootAmmo = currentLoadedWeapon.GetAmmoCount();
        if (timer >= currentLoadedWeapon.GetFireRate() && beforeShootAmmo > 0)
        {
            timer = 0;
            currentLoadedWeapon.Shoot();
            totalShotsDelivered += Mathf.Max(0, beforeShootAmmo - currentLoadedWeapon.GetAmmoCount()); // Mathf.Max avoid negative values
            audioSrc.PlayOneShot(currentLoadedWeapon.GetShotSfx());
            return;
        }
    }


    public int GetTotalShotsDelivered()
    {
        return totalShotsDelivered;
    }

    // this will be invoked externally
    public void LoadNewGun(IPrimary weapon, GameObject shooter)
    {
        if (weapon == null)
        {
            throw new NullReferenceException("ENEMY LOAD CANNOT BE NULL, THE PASSED REFERENCE TO WEAPON MANAGER IS NULL");
        }

        if (this.currentLoadedWeapon != null)
        {
            Debug.LogWarning("Enemy has already a weapon cannot load another one");
        }

        Debug.Log("Enemy loaded a weapon");
        // must be done whatever a new gun gets loaded
        currentLoadedWeapon = weapon;
        if (weapon.GetEquippedSprite() != null)
        {
            enemySpriteRenderer.sprite = weapon.GetEquippedSprite();
        }

        // we're allowed to shoot at te beginning 
        timer = float.PositiveInfinity;
        currentLoadedWeapon.Setup(shooter);

        if (weapon.GetGoonEquippedSprite() != null)
        {
            enemySpriteRenderer.sprite = weapon.GetGoonEquippedSprite();

            ResizeEnemyCollider();
        }

        needsToFindAWeapon = false; // enemy do not needs to find a weapon anymore
        if (needsToPLayOnLoad)
        {
            audioSrc.PlayOneShot(currentLoadedWeapon.GetEquipSfx());
        }
        needsToPLayOnLoad = true;
        currentLoadedWeapon.SetIsGoingToBePickedUp(false);

    }

    private void UnloadCurrentGun()
    {
        if (currentLoadedWeapon == null)
        {
            throw new NullReferenceException("ENEMY GUN CANNOT BE DELOADED IF NO ONE HAS BEEN LOADED");
        }

        Debug.Log("Enemy deloaded a weapon");
        audioSrc.PlayOneShot(currentLoadedWeapon.GetEquipSfx());
        currentLoadedWeapon.PostSetup();
        // Instantiate a new prefab on the ground if there are some ammo
        currentLoadedWeapon = null;
        timer = 0;
        enemySpriteRenderer.sprite = defaultEnemySprite;
        ResizeEnemyCollider();
    }

    public void ChangeEnemyStatus(bool status)
    {
        if (!status)
            this.initialDetectionTime = 0;

        if (status && this.initialDetectionTime == 0)
            this.initialDetectionTime = Time.time;

        this.isEnemyAlerted = status;
    }

    public bool NeedsToFindAWeapon()
    {
        return needsToFindAWeapon;
    }

    public void SetWeaponThatCanBeEquipped(List<Type> list)
    {
        this.weaponTypesThatCanBeEquipped = list;
    }

    public void SetIsPlayerBehindAWall(bool isHidden)
    {
        // Detect the *moment* they come out of cover
        if (isPlayerBehindAWall && !isHidden)
        {
            unhideTimestamp = Time.time;
        }

        isPlayerBehindAWall = isHidden;
    }

    public bool CanWeaponBeEquipped(IPrimary weapon)
    {
        if (weaponTypesThatCanBeEquipped == null || weapon == null)
        {
            return false;
        }

        if (enemyRef.IsEnemyDead())
        {
            weapon.SetIsGoingToBePickedUp(false);
            return false;
        }

        Type weaponType = weapon.GetType();
        foreach (Type type in weaponTypesThatCanBeEquipped)
        {
            if (type.IsAssignableFrom(weaponType))
            {
                return true;
            }
        }

        return false;
    }

    IEnumerator WaitForSfxToEnd()
    {
        while (audioSrc.isPlaying)
        {
            yield return null;
        }

        isReloading = false;
    }

    public void ResizeEnemyCollider()
    {
        BoxCollider2D enemyCollider = gameObject.transform.parent.GetComponentInChildren<BoxCollider2D>();
        Vector2 spriteSize = enemySpriteRenderer.sprite.bounds.size;
        enemyCollider.size = spriteSize;
    }
}
