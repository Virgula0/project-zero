// This interface is the abstraction method provided for implementing weapons
// Ideally, each concrete item that implements it is a wapon.
// Another component will be responsable for calling this methods and let them actually work in the scene
using Unity.VisualScripting;

public interface IGun
{
    void Setup();
    void Shoot();
    void Reload();
    int GetAmmoCount();
    int GetNumberOfReloads();
    float GetFireRate();
    int GetMegCap();
}