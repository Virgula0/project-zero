using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject bulletsText;

    [SerializeField] private GameObject reloadsText;

    [SerializeField] private GameObject pointsText;

    [SerializeField] private GameObject levelText;
    
    [SerializeField] private GameObject chargesText;

    [SerializeField] private Image weaponIcon;

    [SerializeField] private Image secondaryIcon;

    void Start()
    {
       SetLevelText(); 
    }

    public void UpdateBullets(int numBullets){
        bulletsText.GetComponent<TextMeshProUGUI>().text = numBullets.ToString();
    }

    public void UpdateReloads(int numReloads){
        reloadsText.GetComponent<TextMeshProUGUI>().text = numReloads.ToString();
    }

    public void UpdateCharges(int numCharges){
        chargesText.GetComponent<TextMeshProUGUI>().text = "CHARGES: " + numCharges.ToString();
    }

    public void UpdatePoints(int points){
        pointsText.GetComponent<TextMeshProUGUI>().text = points.ToString() + " pts";
    }

    public void UpdateWeaponIcon(Sprite newSprite){
        Color currentColor = weaponIcon.color;
        currentColor.a = 1f - weaponIcon.color.a;
        weaponIcon.color = currentColor;
        weaponIcon.sprite = newSprite;
    }

    //TODO: add method for updating secondary sprite.

    private void SetLevelText(){
        levelText.GetComponent<TextMeshProUGUI>().text = SceneManager.GetActiveScene().name;
    }
    
}
