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

    void Update()
    {
        if (levelText.GetComponent<TextMeshProUGUI>().text != SceneManager.GetActiveScene().name)
        {
            SetLevelText();
            return;
        }
    }

    public void UpdateBullets(int numBullets)
    {
        if (numBullets == int.MaxValue)
        {
            bulletsText.GetComponent<TextMeshProUGUI>().text = "∞";
            return;
        }
        bulletsText.GetComponent<TextMeshProUGUI>().text = numBullets.ToString();
    }

    public void UpdateReloads(int numReloads)
    {
        if (numReloads == int.MaxValue)
        {
            reloadsText.GetComponent<TextMeshProUGUI>().text = "∞";
            return;
        }
        reloadsText.GetComponent<TextMeshProUGUI>().text = numReloads.ToString();
    }

    public void UpdateCharges(int numCharges)
    {
        if (numCharges == int.MaxValue)
        {
            chargesText.GetComponent<TextMeshProUGUI>().text = "∞";
        }
        chargesText.GetComponent<TextMeshProUGUI>().text = "CHARGES: " + numCharges.ToString();
    }

    public void UpdateSecondaryIcon(Sprite newSprite){
        Color currentColor = secondaryIcon.color;
        currentColor.a = 1f - secondaryIcon.color.a;
        secondaryIcon.color = currentColor;
        secondaryIcon.sprite = newSprite;
        secondaryIcon.preserveAspect = true; //prevent stretching of the image
    }

    public void UpdatePoints(int points)
    {
        pointsText.GetComponent<TextMeshProUGUI>().text = points.ToString() + " pts";
    }

    public void UpdateWeaponIcon(Sprite newSprite)
    {
        Color currentColor = weaponIcon.color;
        currentColor.a = 1f - weaponIcon.color.a;
        weaponIcon.color = currentColor;
        weaponIcon.sprite = newSprite;
        secondaryIcon.preserveAspect = true;
    }

    //TODO: add method for updating secondary sprite.

    private void SetLevelText()
    {
        levelText.GetComponent<TextMeshProUGUI>().text = SceneManager.GetActiveScene().name;
    }

}
