using UnityEngine;

public class MockScript : MonoBehaviour
{
    [SerializeField] Canvas ui;

    private int mockAmmoCount = 10;

    private int mockReloadsCount = 4;

    private float mockTimer = 0f;

    private float mockFireRate = 0.5f;

    private bool mockMessageSpawned = false;



    private UIManager uIManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        this.uIManager = ui.GetComponent<UIManager>();
        Debug.Log(uIManager);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R) && mockReloadsCount > 0)
        {
            MockReload();
            uIManager.UpdateReloads(mockReloadsCount);
            uIManager.UpdateBullets(mockAmmoCount);
        }
        // if left button is pressed, let an user to leave the weapon
        if (Input.GetMouseButton((int)Utils.Enums.MouseButtons.RightButton))
        {
            MockUnloadCurrentGun();
            uIManager.UpdateBullets(0);
            uIManager.UpdateReloads(0);
            return;
        }

        if (Input.GetMouseButton((int)Utils.Enums.MouseButtons.LeftButton) &&
            mockTimer >= mockFireRate &&
            mockAmmoCount > 0)
        {
            mockTimer = 0;
            MockShoot();
            uIManager.UpdateBullets(mockAmmoCount);
        }
        else if (mockAmmoCount <= 0 && !mockMessageSpawned)
        { // for debugging purposes, this else statement can be removed later
            Debug.Log("No ammo, need to reload, press R!");
            mockMessageSpawned = true; // avoid spam on console
        }

        mockTimer += Time.deltaTime;
    }

    private void MockShoot(){
        this.mockAmmoCount -= 1;
    }

    private void MockReload(){
        this.mockAmmoCount = 10;
        this.mockReloadsCount -= 1;
    }

    private void MockUnloadCurrentGun()
    {
        mockTimer = 0;
    }

}
