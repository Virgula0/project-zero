using UnityEngine;

public class SavedData : MonoBehaviour
{
    private LogicManager logic;
    private int currentTotalPoints;

    public bool SetLogicManager(LogicManager logic)
    {
        this.logic = logic;
        return true;
    }

    public void Save()
    {
        this.currentTotalPoints = logic.GetTotalPoints();
    }

    public int GetSavedPoints()
    {
        return currentTotalPoints;
    }
}
