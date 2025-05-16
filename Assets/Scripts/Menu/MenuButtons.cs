using Assets.Scripts.Menu;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButtons : MonoBehaviour
{
    [SerializeField] private AudioClip positiveClick;
    [SerializeField] private AudioClip onHoverEffect;
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject optionSubmenu;
    [SerializeField] private GameObject leaderboardSubmenu;
    [SerializeField] private GameObject mainTitle;
    [SerializeField] AudioSource normalAudioSrc;
    [SerializeField] AudioSource hoverAudioSrc;
    private const string loadingScene = Utils.Const.LOAD_MENU_SCENE;

    public void ClickSound()
    {
        normalAudioSrc.PlayOneShot(positiveClick);
    }

    private void ClickSoundAndSwitch(GameObject toSwitch)
    {
        ClickSound(); // Wait for the clip to finish playing
        gameObject.SetActive(false); // Now disable the current menu
        toSwitch.SetActive(true);    // Activate the target menu
    }

    private void SwitchSubMenu(GameObject toSwitch)
    {
        ClickSoundAndSwitch(toSwitch);
    }

    public void OnHoverEffect()
    {
        hoverAudioSrc.PlayOneShot(onHoverEffect);
    }

    public void ClickStart()
    {
        if (SwitchScene.Instance != null)
        {
            SwitchScene.Instance.ResetSavedData();
        }
        ClickSound();
        StartMainMenu.Instance.BeginPlayTime();
        SceneManager.LoadScene(loadingScene);
    }

    public void ClickOptions()
    {
        SwitchSubMenu(optionSubmenu);
    }

    public void ClickLeaderBoard()
    {
        SwitchSubMenu(leaderboardSubmenu);
        mainTitle.SetActive(false);
    }

    public void ClickBack()
    {
        SwitchSubMenu(mainMenu);
        mainTitle.SetActive(true);
    }

    public void LoadMainMenuScene()
    {
        ClickSound();
        SceneManager.LoadScene(Utils.Const.MAIN_MENU_SCENE);
    }

    public void ClickExit()
    {
        ClickSound();
        Application.Quit();
    }
}
