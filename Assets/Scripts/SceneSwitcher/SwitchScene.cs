using UnityEngine;
using UnityEngine.SceneManagement;

public class SwitchScene : MonoBehaviour
{
    private SavedData saver;
    public static SwitchScene Instance { get; private set; }
    [SerializeField] private AudioSource backgroundSource;

    void Awake()
    {
        if (SceneManager.GetActiveScene().buildIndex == 2
            && Instance != null
            && !Instance.backgroundSource.isPlaying) // if first game scene run the audio
        {
            PlayMainMusic();
        }

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

    public void StopMainMusic()
    {
        backgroundSource.Stop();
    }

    private void PlayMainMusic()
    {
        backgroundSource.volume = 0.2f;
        Instance.backgroundSource.Play();
    }

    void Start()
    {
        saver = GetComponentInChildren<SavedData>();
    }

    public void SaveAndGoNext()
    {
        saver.Save();
        LoaderManager.Instance.DestroyThis();
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
