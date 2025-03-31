using UnityEngine;
using UnityEngine.UI;

public class loadingtext : MonoBehaviour {

    private RectTransform rectComponent;
    private Image imageComp;

    [SerializeField]
    private Text text;

    // Use this for initialization
    void Start () {
        rectComponent = GetComponent<RectTransform>();
        imageComp = rectComponent.GetComponent<Image>();
        imageComp.fillAmount = 0.0f;
    }
        
    public void UpdateLoadingProgress(float progress)
    {
        // Increase fill amount based on deltaTime and speed
        imageComp.fillAmount =  progress;
        text.text = (int)(progress * 100) + "%";
    }
}
