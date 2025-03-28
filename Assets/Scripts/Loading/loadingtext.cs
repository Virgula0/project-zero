using UnityEngine;
using UnityEngine.UI;

public class loadingtext : MonoBehaviour {

    private RectTransform rectComponent;
    private Image imageComp;

    [SerializeField]
    private Text text;

    [SerializeField]
    private Text textNormal;
    private const float FullFill = 1f;
    private const float ResetFill = 0f;

    // Use this for initialization
    void Start () {
        rectComponent = GetComponent<RectTransform>();
        imageComp = rectComponent.GetComponent<Image>();
        imageComp.fillAmount = 0.0f;
    }
        
    public void UpdateLoadingProgress(int progress)
    {
        // Increase fill amount based on deltaTime and speed
        imageComp.fillAmount += Time.deltaTime * progress;
        text.text = progress + "%";
    }

}
