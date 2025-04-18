// This interface is the abstraction method provided for implementing weapons
// Ideally, each concrete item that implements it is a wapon.
// Another component will be responsable for calling this methods and let them actually work in the scene
using UnityEngine;

public interface IGun
{
    void Setup(GameObject player);
    void Shoot();
    void Reload();
    int GetAmmoCount();
    int GetNumberOfReloads();
    int GetMegCap();
    float GetFireRate();
    void SaveStatus(IGun weapon);
    bool IsGoingToBePickedUp();
    void SetIsGoingToBePickedUp(bool status);
    Sprite GetEquippedSprite();
    Sprite GetStaticWeaponSprite();
    AudioClip GetShotSfx();
    AudioClip GetReloadSfx();
    AudioClip GetEquipSfx();
}