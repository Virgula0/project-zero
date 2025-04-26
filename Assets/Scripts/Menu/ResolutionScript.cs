using UnityEngine;
using TMPro;

public class OptionMenu : MonoBehaviour
{
    private TMP_Dropdown dropdown;
    private string currentRes;
    void Start()
    {
        dropdown = GetComponent<TMP_Dropdown>();
        currentRes = Screen.currentResolution.ToString().Trim().Split("@")[0].Replace(" ","");
        var options = dropdown.options;
        for (int i = 0; i < options.Count; i++)
        {
            string resInList = dropdown.options[i].text;
            if (Equals(resInList, currentRes))
            {
                Debug.Log("Default resolution set to " + currentRes);
                dropdown.value = i; // set index to current res
                break;
            }
        }
    }

    public void ChangeResolution()
    {
        string selectedOption = dropdown.options[dropdown.value].text;
        // Split by 'x'
        string[] dimensions = selectedOption.Trim().Split('x'); //×

        print(Screen.currentResolution);

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
