using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Common;
using Assets.Scripts.Menu;
using TMPro;
using UnityEngine;

public class PointsRecap : MonoBehaviour
{
    private SwitchScene switcher;
    private int points = 3000; // 3000 is for mocking, it will be overwritten in the start
    [SerializeField] private AudioSource source;
    [SerializeField] private AudioClip machineSound;
    [SerializeField] TMP_Text pointsText;
    private bool animFinished;
    private float animationTotalTime = 0; // in seconds

    void Start()
    {
        switcher = SwitchScene.Instance;

         points = switcher.GetCurrentSavedData();
         StartMainMenu obj = StartMainMenu.Instance;
         Repository repo = obj.GetRepository();
         obj.GetCursorChangerScript().ChangeToDefaultCursor();

         repo.InsertData(Repository.Tables.Stats.ToString(), new Dictionary<string, object>{
                     { "PlayerName", System.Environment.MachineName },
                     { "Score",points },
                     {"Time", ((Time.time - obj.GetStartPlayTime())/60).ToString()}
         });

        SwitchScene.Instance.StopMainMusic();
        animationTotalTime = machineSound.length;
        StartCoroutine(PlaySound());
        StartCoroutine(SlotAnimation());
    }

    private IEnumerator PlaySound()
    {
        while (!animFinished)
        {
            source.PlayOneShot(machineSound);
            while (source.isPlaying)
            {
                yield return null;
            }
        }
    }

    private IEnumerator SlotAnimation()
    {
        float elapsed = 0f;
        int lastDisplayed = 0;
        while (elapsed < animationTotalTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / animationTotalTime);
            int current = Mathf.FloorToInt(Mathf.Lerp(0, points, t));
            if (current != lastDisplayed)
            {
                pointsText.text = current.ToString();
                lastDisplayed = current;
            }
            yield return null;
        }
        pointsText.text = points.ToString();
        animFinished = true;
    }
}
