using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject bulletsText;

    [SerializeField] private GameObject reloadsText;

    [SerializeField] private TextMeshPro pointsText;

    [SerializeField] private TextMeshPro levelText;

    //TODO: add variables for Primary and Secondary weapons sprites.

    public void UpdateBullets(int numBullets){
        bulletsText.GetComponent<TextMeshProUGUI>().text = numBullets.ToString();
    }

    public void UpdateReloads(int numReloads){
        reloadsText.GetComponent<TextMeshProUGUI>().text = numReloads.ToString();
    }
}
