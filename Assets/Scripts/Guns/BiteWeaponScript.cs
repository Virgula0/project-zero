using System.Collections;
using UnityEngine;

public class BiteWeaponScript : MonoBehaviour, IPrimary, IMelee
{
    private readonly float fireRate = 1f; // 1 hit per second
    private readonly int magCap = int.MaxValue;
    private int numberOfReloads = int.MaxValue;
    private int ammoCount;
    
    [SerializeField] private float minDistanceForSwing = 2f;
    [SerializeField] private GameObject bitePrefab;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private AudioClip biteSound;

    private GameObject wielder;
    private BiteScript currentInitScript;
    private GameObject currentInitPrefab; 

    public void Setup(GameObject player)
    {
        wielder = player;
        currentInitPrefab = Instantiate(bitePrefab, wielder.transform.position, Quaternion.identity);
        currentInitScript = currentInitPrefab.GetComponent<BiteScript>();
        currentInitScript.Initialize(wielder, biteSound, this);
    }

    void Awake()
    {
        ammoCount = magCap;
    }

    public void Shoot()
    {   
        currentInitScript.Bite();
    }

    public void Reload(){}
    public int GetNumberOfReloads() => numberOfReloads;
    public float GetFireRate() => fireRate;
    public int GetMegCap() => magCap;
    public int GetAmmoCount() => ammoCount;
    public Sprite GetEquippedSprite() => null;
    public Sprite GetStaticWeaponSprite() => null;
    public AudioClip GetShotSfx() => biteSound;
    public AudioClip GetReloadSfx() => null;
    public AudioClip GetEquipSfx() => null;

    public IEnumerator SaveStatus(IPrimary other)
    {
        yield break;
    }

    //public bool IsGoingToBePickedUp() => isGoingToBePickedUp;
    //public void SetIsGoingToBePickedUp(bool status) => isGoingToBePickedUp = status;

    public void PostSetup()
    {
        Destroy(currentInitPrefab);
    }

    public Sprite GetGoonEquippedSprite()
    {
        return null;
    }

    public float MinDistanceForSwing()
    {
        return minDistanceForSwing;
    }

    public bool IsGoingToBePickedUp()
    {
        return false;
    }

    public void SetIsGoingToBePickedUp(bool status)
    {
        return;
    }
}
