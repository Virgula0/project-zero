using UnityEngine;
using TMPro;
using System.Text;

public class OptionMenu : MonoBehaviour
{
    private TMP_Dropdown dropdown;

    void Start()
    {
        dropdown = GetComponent<TMP_Dropdown>();
    }

    public void ChangeResolution()
    {
        string selectedOption = dropdown.options[dropdown.value].text;
        // Split by 'x'
        string[] dimensions = selectedOption.Trim().Split('x'); //×

        Debug.Log(dimensions.ToString());

        if (dimensions.Length == 2 &&
            int.TryParse(dimensions[0], out int width) &&
            int.TryParse(dimensions[1], out int height))
        {
            // Apply the new resolution
            Screen.SetResolution(width, height, Screen.fullScreen);
            Debug.Log($"Resolution set to: {width} x {height}");
        }
        else
        {
            Debug.LogWarning("Invalid resolution format!");
        }
    }
}
