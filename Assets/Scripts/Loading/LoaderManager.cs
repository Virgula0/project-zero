using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoaderManager : MonoBehaviour
{
    [SerializeField] private GameObject vicaGameObject;
    private loadingtext script;
    public static LoaderManager Instance { get; private set; }

    public void DestroyThis()
    {
        if (Instance == null || gameObject == null)
        {
            return;
        }

        Destroy(gameObject);
    }

    private IEnumerator SwitchAndWait()
    {
        yield return StartCoroutine(ShowLoadingProgress());
        ActivateNextScene();
    }

    private IEnumerator Start()
    {
        if (Instance == null)
        {
            DontDestroyOnLoad(gameObject);
            Instance = this;
        }
        script = vicaGameObject.GetComponent<loadingtext>();
        yield return SwitchAndWait();
    }

    public void ActivateNextScene()
    {
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
