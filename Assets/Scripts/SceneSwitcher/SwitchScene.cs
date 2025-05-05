using UnityEngine;

public class SwitchScene : MonoBehaviour
{
    private SavedData saver;
    public static SwitchScene Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            // Another instance already exists, destroy this one
            Destroy(gameObject);
            return;
        }

        // Set this as the singleton instance
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }


    void Start()
    {
        saver = GetComponentInChildren<SavedData>();
    }

    public void SaveAndGoNext()
    {
        saver.Save();
        LoaderManager.Instance.ActivateNextScene();
    }

    public int GetCurrentSavedData()
    {
        return saver.GetSavedPoints();
    }

    public bool UpdateLogicReference(LogicManager logic)
    {
        if (logic == null || saver == null)
        {
            return false;
        }

        return saver.SetLogicManager(logic);
    }
}
