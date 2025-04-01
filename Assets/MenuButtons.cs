using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class MenuButtons : MonoBehaviour
{
    [SerializeField] private AudioSource positiveClick;
    [SerializeField] private AudioSource onHoverEffect;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        this.positiveClick = GetComponents<AudioSource>()[0];
        this.onHoverEffect = GetComponents<AudioSource>()[1];
    }

    private void ClickSound(){
        this.positiveClick.Play();
    }

    public void OnHoverEffect(){
        this.onHoverEffect.Play();
    }

    public void ClickStart(){
        ClickSound();
    }

    public void ClickOptions(){
        ClickSound();
    }

    public void ClickLeaderBoard(){
        ClickSound();
    }

    public void ClickExit(){
        ClickSound();
        Application.Quit();
    }
}
