using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Common;
using Assets.Scripts.Menu;
using TMPro;
using UnityEngine;

public class PointsRecap : MonoBehaviour
{
    private SwitchScene switcher;
    private int points;
    [SerializeField] private AudioSource source;
    [SerializeField] private AudioClip machineSound;
    [SerializeField] TMP_Text pointsText;
    private bool animFinished;

    void Start()
    {
        switcher = GameObject.FindGameObjectWithTag(Utils.Const.SCENE_SWITCHER_TAG).GetComponent<SwitchScene>();
        points = switcher.GetCurrentSavedData();
        var obj = GameObject.FindGameObjectWithTag(Utils.Const.MAIN_MENU_OBJ).GetComponent<StartMainMenu>();
        Repository repo = obj.GetRepository();

        repo.InsertData(Repository.Tables.Stats.ToString(), new Dictionary<string, object>{
                    { "PlayerName", "test" },
                    { "Score",points }
        });

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
        int current = 0;
        while (current < points)
        {
            current += 10;
            pointsText.text = current.ToString();
            yield return new WaitForSeconds(0.001f);
        }
        pointsText.text = points.ToString();
        animFinished = true;
    }
}
