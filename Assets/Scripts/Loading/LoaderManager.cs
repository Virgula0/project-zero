using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections.Generic;

public class DirectorySceneLoader : MonoBehaviour
{
    private string firstSceneName;  // The actual first game scene

    [SerializeField]
    private GameObject vicaGameObject;
    private loadingtext script;

    private IEnumerator Start()
    {
        script = vicaGameObject.GetComponent<loadingtext>();

        int sceneCount = SceneManager.sceneCountInBuildSettings;
        // We'll store only the "game" scenes, skipping the main menu
        // (which is presumably index 0 in your build settings)
        var sceneList = new List<AsyncOperation>();

        for (int i = 0; i < sceneCount; i++)
        {
            // Get scene path and name
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = Path.GetFileNameWithoutExtension(scenePath);

            // Skip the Main Menu scene
            if (i == 0 || i == 1) // skip MainMenuScene and LoadingScene
            {
                continue;
            }

            if (string.IsNullOrEmpty(firstSceneName))
            {
                // The first *game* scene we find becomes our firstSceneName
                firstSceneName = sceneName;
            }

            Debug.Log("Loading scene in background: " + sceneName);
            AsyncOperation asyncOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            asyncOp.allowSceneActivation = false;
            sceneList.Add(asyncOp);
        }

        // Wait until all "game" scenes reach 90% loaded
        bool allScenesLoaded = false;
        while (!allScenesLoaded)
        {
            allScenesLoaded = true;
            foreach (var op in sceneList)
            {
                script.UpdateLoadingProgress(op.progress);

                if (op.progress < 0.9f)
                {
                    allScenesLoaded = false;
                    break;
                }
            }
            yield return new WaitForSeconds(1);
        }

        Debug.Log("All scenes loaded. Activating the first game scene: " + firstSceneName);

        // Activate that first game scene
        AsyncOperation firstSceneOp = sceneList[0];
        firstSceneOp.allowSceneActivation = true;

        // Wait for the first scene to finish activating
        while (!firstSceneOp.isDone)
        {
            yield return null;
        }

        // Set that scene as the active scene
        Scene firstScene = SceneManager.GetSceneByName(firstSceneName);
        if (firstScene.IsValid())
        {
            if (!SceneManager.SetActiveScene(firstScene)){
                Debug.LogError(firstScene.name + "should be loaded but was not loaded when trying to set to active");
            }
            Debug.Log("First scene is now active: " + firstSceneName);
        }
        else
        {
            Debug.LogError("Failed to set the first scene as active.");
        }
    }
}
