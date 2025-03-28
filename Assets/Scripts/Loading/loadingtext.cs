using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class loadingtext : MonoBehaviour {

    private RectTransform rectComponent;
    private Image imageComp;

    public float speed = 200f;
    public Text text;
    public Text textNormal;
    private const float FullFill = 1f;
    private const float ResetFill = 0f;

    // Use this for initialization
    void Start () {
        rectComponent = GetComponent<RectTransform>();
        imageComp = rectComponent.GetComponent<Image>();
        imageComp.fillAmount = 0.0f;
    }
        
    void Update()
    {
        // Only update if the fill amount is not full
        if (imageComp.fillAmount != FullFill)
        {
            // Increase fill amount based on deltaTime and speed
            imageComp.fillAmount += Time.deltaTime * speed;
            
            // Convert fill amount to percentage
            int fillPercentage = (int)(imageComp.fillAmount * 100);
            
            // Update the text based on the current fill percentage
            if (fillPercentage > 0 && fillPercentage <= 33)
            {
                textNormal.text = "Loading...";
            }
            // Additional conditions for other ranges can be added here if needed

            text.text = fillPercentage + "%";
        }
        else
        {
            // Reset fill amount and text when full
            imageComp.fillAmount = ResetFill;
            text.text = "0%";
        }
    }

}
