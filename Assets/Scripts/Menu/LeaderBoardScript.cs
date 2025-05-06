using System.Collections.Generic;
using Assets.Scripts.Menu;
using UnityEngine;
using Assets.Scripts.Common;
using TMPro;

public class LeaderBoardScript : MonoBehaviour
{
    [SerializeField] private GameObject row;          // The row prefab
    [SerializeField] private Transform contentParent; // Content object from the ScrollView
    private StartMainMenu menu;

    void Start()
    {

        if (row == null || contentParent == null)
        {
            return;
        }

        menu = StartMainMenu.Instance;

        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        List<Dictionary<string, object>> players = menu.GetRepository().ReadData(Repository.Tables.Stats.ToString());

        foreach (var player in players)
        {
            // Debug.Log($"ID: {player["ID"]}, Name: {player["PlayerName"]}, Age: {player["Score"]}, Time: {player["Time"]}");

            GameObject newRow = Instantiate(row, contentParent); // Sets Content as parent
            newRow.transform.localScale = Vector3.one; // Ensure scale is correct

            TMP_Text[] text = newRow.GetComponentsInChildren<TMP_Text>();
            text[0].text = $"{player["PlayerName"]}";
            text[1].text = $"{player["Score"]}";
            text[2].text = $"{player["Time"]} min.";
        }
    }
}
