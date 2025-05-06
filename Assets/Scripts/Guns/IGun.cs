using System.Collections;
using UnityEngine;

// Handles initial setup of the weapon with the player
public interface IInitializableWeapon
{
    void Setup(GameObject player);
    void PostSetup();
}

// Anything that can fire projectiles
public interface IFireable
{
    void Shoot();
    // returns shots per second
    float GetFireRate();
}

// Anything that can reload and track ammunition
public interface IReloadable
{
    void Reload();
    int GetAmmoCount();
    int GetNumberOfReloads();
    int GetMegCap();
}

// Persistence of weapon state (ammo, upgrades, etc.)
public interface IStatusPersistable
{
    IEnumerator SaveStatus(IPrimary other);
}

// Enables “will-be-picked-up” flag on world weapons
public interface IPickupable
{
    bool IsGoingToBePickedUp();
    void SetIsGoingToBePickedUp(bool status);
}

// Supplies sprites for UI and world display
public interface IVisualWeapon
{
    Sprite GetEquippedSprite();
    Sprite GetGoonEquippedSprite();
    Sprite GetStaticWeaponSprite();
}

// Supplies audio clips for shooting, reloading, equipping
public interface IAudioWeapon
{
    AudioClip GetShotSfx();
    AudioClip GetReloadSfx();
    AudioClip GetEquipSfx();
}

public interface IThrowable {
    void ThrowWhereMousePoints();
    IEnumerator PlayThrowSfx(AudioSource src);
}

// Full‐feature gun interface combining all individual aspects
public interface IPrimary :
    IInitializableWeapon,
    IFireable,
    IReloadable,
    IStatusPersistable,
    IPickupable,
    IVisualWeapon,
    IAudioWeapon
{}

public interface ISecondary : 
    IInitializableWeapon,
    IFireable,
    IVisualWeapon,
    IAudioWeapon,
    IReloadable,
    IPickupable
{}