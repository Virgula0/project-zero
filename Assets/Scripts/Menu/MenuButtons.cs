using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButtons : MonoBehaviour
{
    [SerializeField] private AudioSource positiveClick;
    [SerializeField] private AudioSource onHoverEffect;
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject optionSubmenu;
    [SerializeField] private GameObject leaderboardSubmenu;

    private const string loadingScene = "LoadingScene";

    private IEnumerator ClickSound(){
        positiveClick.Play();
        yield return new WaitForSeconds(positiveClick.clip.length); // Wait for the clip to finish
    }
    
    private IEnumerator ClickSoundAndSwitch(GameObject toSwitch)
    {
        yield return ClickSound(); // Wait for the clip to finish playing
        gameObject.SetActive(false); // Now disable the current menu
        toSwitch.SetActive(true);    // Activate the target menu
    }

    private void SwitchSubMenu(GameObject toSwitch)
    {
        StartCoroutine(ClickSoundAndSwitch(toSwitch));
    }

    public void OnHoverEffect(){
        onHoverEffect.Play();
    }

    public void ClickStart(){
        StartCoroutine(ClickSound());
        SceneManager.LoadScene(loadingScene);
    }

    public void ClickOptions(){
        SwitchSubMenu(optionSubmenu);
    }

    public void ClickLeaderBoard(){
        SwitchSubMenu(leaderboardSubmenu);
    }

    public void ClickBack(){
        SwitchSubMenu(mainMenu);
    }

    public void ClickExit(){
        StartCoroutine(ClickSound());
        Application.Quit();
    }
}
