using UnityEngine;
using UnityEngine.UI;

public class Loading : MonoBehaviour
{
    private RectTransform rectComponent;
    private Image imageComp;
    public float rotateSpeed = 200f;
    public float openSpeed = .005f;
    public float closeSpeed = .01f;
    private const float MaxFillThreshold = 0.30f;
    private const float MinFillThreshold = 0.02f;
    private bool increasing = true;

    private void Start()
    {
        rectComponent = GetComponent<RectTransform>();
        imageComp = rectComponent.GetComponent<Image>();
    }

    private void Update()
    {
        rectComponent.Rotate(0f, 0f, rotateSpeed * Time.deltaTime);
        ChangeSize();
    }
    private void ChangeSize()
    {
        float currentFill = imageComp.fillAmount;

        // If we are increasing and haven't reached the maximum fill, increase the fill amount.
        if (increasing && currentFill < MaxFillThreshold)
        {
            imageComp.fillAmount += openSpeed;
            return;
        }
        
        // If increasing but the maximum threshold is reached, reverse direction.
        if (increasing && currentFill >= MaxFillThreshold)
        {
            increasing = false;
            return;
        }
        
        // If we are decreasing and above the minimum fill, decrease the fill amount.
        if (!increasing && currentFill > MinFillThreshold)
        {
            imageComp.fillAmount -= closeSpeed;
            return;
        }
        
        // If decreasing but the minimum threshold is reached, reverse direction.
        if (!increasing && currentFill <= MinFillThreshold)
        {
            increasing = true;
        }
    }
}