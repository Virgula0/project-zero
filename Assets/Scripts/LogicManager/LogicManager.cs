using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

// Used for managing logic, game over, sprites to be changed, point logic
public class LogicManager : MonoBehaviour
{
    private int totalPoints = 0;
    private PlayerScript playerReference;
    private bool isTheRoomClear = false;
    [SerializeField] private CanvasGroup fadeCanvasGroup; // add a canvas UI with black screen image
    [SerializeField] private GameObject playerUI;
    [SerializeField] private GameObject gameOverPrefab;
    private const string ScreenOverlay = "ScreenOverlay";
    private CursorChanger cursorChanger;
    private UIManager ui;
    private Animator playerAnimatorRef;
    private PlayerAnimationScript playerspriteRef;

    void Start()
    {
        this.playerReference = GameObject.FindGameObjectWithTag(Utils.Const.PLAYER_TAG).GetComponent<PlayerScript>();
        this.ui = GameObject.FindGameObjectWithTag(Utils.Const.UI_MANAGER_TAG).GetComponent<UIManager>();
        this.cursorChanger = GameObject.FindGameObjectWithTag(Utils.Const.CURSOR_CHANGER_TAG).GetComponent<CursorChanger>();
        this.playerspriteRef = GameObject.FindGameObjectWithTag(Utils.Const.PLAYER_TAG).GetComponentInChildren<PlayerAnimationScript>();
        playerAnimatorRef = GameObject.FindGameObjectWithTag(Utils.Const.PLAYER_TAG).GetComponentInChildren<Animator>();

        if (SwitchScene.Instance != null)
        {
            if (SwitchScene.Instance.UpdateLogicReference(this))
            {
                int pointsSaved = SwitchScene.Instance.GetCurrentSavedData();

                if (this.totalPoints < pointsSaved)
                {
                    this.totalPoints = pointsSaved;
                }
            }
        }

        ui.UpdatePoints(this.totalPoints);
    }

    void Update()
    {
        // only poll until we know the room is clear
        if (!isTheRoomClear)
            CheckAllEnemiesDead();
        CheckRestartGame();
    }

    public bool IsTheRoomClear()
    {
        return isTheRoomClear;
    }

    public void GameOver(IPrimary weapon)
    {
        if (playerReference.IsGodMode())
        {
            Debug.LogWarning("Unable to game over, god mode is activated");
            return;
        }
        // manages game over
        cursorChanger.ChangeToDefaultCursor();
        playerUI.SetActive(false);
        gameOverPrefab.SetActive(true);
        playerReference.SetIsPlayerAlive(false);

        playerReference.PlayDeathSound();
        playerspriteRef.SetPlayerDeadSprite(weapon);
    }

    public void GameOver()
    {
        if (playerReference.IsGodMode())
        {
            Debug.LogWarning("Unable to game over, god mode is activated");
            return;
        }
        // manages game over
        cursorChanger.ChangeToDefaultCursor();
        playerUI.SetActive(false);
        gameOverPrefab.SetActive(true);
        playerReference.SetIsPlayerAlive(false);
        
        playerReference.PlayDeathSound();
        //playerAnimatorRef.enabled = false;
        playerspriteRef.SetPlayerDeadSprite();
    }

    public void ReloadSceneWithFade()
    {
        // Make sure the black overlay starts fully transparent:
        fadeCanvasGroup.alpha = 0f;
        gameOverPrefab.transform.Find(ScreenOverlay).gameObject.SetActive(false);
        StartCoroutine(FadeWhileLoading());
    }

    public void AddEnemyKilledPoints(IPoints points)
    {
        if (points == null)
        {
            return;
        }
        totalPoints += CalculateScore(points);
        ui.UpdatePoints(totalPoints);
    }

    public int GetTotalPoints()
    {
        return totalPoints;
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
        var enemies = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
                          .OfType<IEnemy>();

        // if any enemy is still alive
        foreach (var e in enemies)
            if (!e.IsEnemyDead())
                return;

        // nobody left standing
        isTheRoomClear = true;
    }

    /* 
     * FORMULA:
      *  More shots → higher score
      *  The term w₁·S grows linearly with shots delivered.
      *
      *  Less chase time → higher score
      *  The term w₂·(1/(T+ε)) is largest when T is small, and falls off as T increases.
      *
      *  Plus base points
      *  Simply add existing GetBasePoints().
      * in other words:
      *
      * - the less is the TotalChasedTime, the higher will be the point
      * - the higher are the total shots delivered, the higher will be the point
      * - sum the base points to the calculation done on the first 2
    */
    private int CalculateScore(IPoints points, float weightShots = 5f, float weightChase = 100f, float epsilon = 0.1f)
    {
        int P0 = points.GetBasePoints();
        int S = points.GetTotalShotsDelivered();
        float T = points.GetTotalChasedTime();

        float shotsScore = weightShots * S;
        float chaseScore = weightChase / (T + epsilon);

        return P0 + Mathf.RoundToInt(shotsScore + chaseScore);
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
