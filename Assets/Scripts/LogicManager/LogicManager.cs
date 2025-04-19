using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

// Used for managing logic, game over, sprites to be changed, point logic
public class LogicManager : MonoBehaviour
{
    private int totalPoints;
    private PlayerScript playerReference;
    private bool isTheRoomClear = false;
    [SerializeField] private CanvasGroup fadeCanvasGroup; // add a canvas UI with black screen image
    [SerializeField] private GameObject playerUI;
    [SerializeField] private GameObject gameOverPrefab;
    private const string ScreenOverlay = "ScreenOverlay";

    void Start()
    {
        this.playerReference = GameObject.FindGameObjectWithTag(Utils.Const.PLAYER_TAG).GetComponent<PlayerScript>();
    }

    private void CheckRestartGame()
    {
        // Reloading the scene create new instances for each game object within the scene itself
        if (!this.playerReference.IsPlayerAlive() && Input.GetKeyDown(KeyCode.R) && gameOverPrefab.activeSelf)
        {
            ReloadSceneWithFade(); // reload the current scene
        }
    }

    private void CheckAllEnemiesDead()
    {

        IEnemy[] enemRef = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).
                OfType<IEnemy>().
                ToArray();

        foreach (var enemy in enemRef)
        {
            if (!enemy.IsEnemyDead())
            {
                return;
            }
        }

        this.isTheRoomClear = true;
    }


    void Update()
    {
        // check actively if all enemies in the room are dead
        CheckAllEnemiesDead();
        CheckRestartGame();
    }

    public bool IsTheRoomClear()
    {
        return isTheRoomClear;
    }

    public void GameOver()
    {
        // manages game over
        playerUI.SetActive(false);
        gameOverPrefab.SetActive(true);
        playerReference.SetIsPlayerAlive(false);
    }


    public void ReloadSceneWithFade()
    {
        // Make sure the black overlay starts fully transparent:
        fadeCanvasGroup.alpha = 0f;
        gameOverPrefab.transform.parent.Find(ScreenOverlay).gameObject.SetActive(false);        
        StartCoroutine(FadeWhileLoading());
    }

    private IEnumerator FadeWhileLoading()
    {
        // Begin loading the current scene in the background:
        string sceneName = SceneManager.GetActiveScene().name;
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(sceneName);
        loadOp.allowSceneActivation = false;

        // As long as Unity reports < 0.9 progress, it's still loading:
        // (Unity holds at 0.9 until you allowSceneActivation = true)
        while (loadOp.progress < 0.9f)
        {
            // Map the 0 → 0.9 load progress to a 0 → 1 fade progress:
            float normalized = Mathf.InverseLerp(0f, 0.9f, loadOp.progress);
            fadeCanvasGroup.alpha = normalized;

            yield return null;  // wait a frame
        }

        // Ensure we're fully faded out:
        fadeCanvasGroup.alpha = 1f;

        // Unpause time (in case you had Time.timeScale = 0 for GameOver):
        Time.timeScale = 1f;

        // Finally, let Unity swap to the new scene:
        loadOp.allowSceneActivation = true;
    }
}
