using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoaderManager : MonoBehaviour
{
    [SerializeField]
    private GameObject vicaGameObject;
    private loadingtext script;
    public static LoaderManager Instance { get; private set; }
    [SerializeField] private AudioSource backgroundSource;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void PlayMainMusic()
    {
        backgroundSource.Play();
    }

    public void StopMainMusic()
    {
        backgroundSource.Stop();
    }

    private IEnumerator Start()
    {
        script = vicaGameObject.GetComponent<loadingtext>();
        yield return StartCoroutine(ShowLoadingProgress());
        ActivateNextScene();
    }

    public void ActivateNextScene()
    {
        if (SceneManager.GetActiveScene().buildIndex + 1 == 2) { // if first game scene rnu the audio
            PlayMainMusic();
        }
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1, LoadSceneMode.Single);
    }

    private IEnumerator ShowLoadingProgress()
    {
        // e.g. simulate or track progress…
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 0.5f;
            script.UpdateLoadingProgress(t);
            yield return null;
        }
    }
}
