using UnityEngine;

public class LeaderBoardScript : MonoBehaviour
{
    [SerializeField] private GameObject row;          // The row prefab
    [SerializeField] private Transform contentParent; // Content object from the ScrollView

    void Start()
    {
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        // Create 10 mock rows
        for (int i = 0; i < 20; i++)
        {
            GameObject newRow = Instantiate(row, contentParent); // Sets Content as parent
            newRow.transform.localScale = Vector3.one; // Ensure scale is correct

            // newRow.GetComponentInChildren<Text>().text = $"Player {i}";
        }
    }
}
