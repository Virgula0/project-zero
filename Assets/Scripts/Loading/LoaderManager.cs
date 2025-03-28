using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DirectorySceneLoader : MonoBehaviour
{
    private string sceneDirectory = "Assets/Scenes/Level1";  // Directory to search for scenes

    private AsyncOperation[] asyncOperations;
    private string firstSceneName;

    [SerializeField]
    private GameObject vicaGameObject;
    private loadingtext script;

    private IEnumerator Start()
    {
        // get initialized script component for upadting text
        script = vicaGameObject.GetComponent<loadingtext>();
        
        // Get all scene files in the specified directory
        string[] sceneFiles = Directory.GetFiles(sceneDirectory, "*.unity", SearchOption.TopDirectoryOnly);
        
        if (sceneFiles.Length == 0)
        {
            Debug.LogError("No scenes found in directory: " + sceneDirectory);
            yield break;
        }

        asyncOperations = new AsyncOperation[sceneFiles.Length];
        firstSceneName = Path.GetFileNameWithoutExtension(sceneFiles[0]);

        // Load all scenes additively but don't activate them
        for (int i = 0; i < sceneFiles.Length; i++)
        {
            string sceneName = Path.GetFileNameWithoutExtension(sceneFiles[i]);
            Debug.Log("Loading scene in background: " + sceneName);

            AsyncOperation asyncOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            asyncOp.allowSceneActivation = false;  // Prevent immediate activation
            asyncOperations[i] = asyncOp;
        }

        // Wait for all scenes to load up to 90% (Unity keeps them at 0.9 until activation)
        bool allScenesLoaded = false;
        while (!allScenesLoaded)
        {
            allScenesLoaded = true; // Assume all scenes are ready

            foreach (var op in asyncOperations)
            {
                script.UpdateLoadingProgress((int)Math.Round(op.progress*100)); // upload progress on the graphics

                if (op.progress < 0.9f)
                {
                    allScenesLoaded = false; // If any scene is not yet ready, keep waiting
                    break;
                }
            }

            yield return null;  // Update status each frame
        }

        Debug.Log("All scenes loaded. Activating the first scene: " + firstSceneName);

        // Activate only the first scene
        AsyncOperation firstSceneOp = asyncOperations[0];
        firstSceneOp.allowSceneActivation = true;

        // Wait for it to fully activate
        while (!firstSceneOp.isDone)
        {
            yield return null;
        }

        // Set the first scene as the active scene
        Scene firstScene = SceneManager.GetSceneByName(firstSceneName);
        if (firstScene.IsValid())
        {
            SceneManager.SetActiveScene(firstScene);
            Debug.Log("First scene is now active: " + firstSceneName);
        }
        else
        {
            Debug.LogError("Failed to set the first scene as active.");
        }
    }
}
