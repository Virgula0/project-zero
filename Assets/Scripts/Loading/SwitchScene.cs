using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class SwitchScene : MonoBehaviour
{
    private int currentScene;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private IEnumerator Start()
    {
        this.currentScene = SceneManager.GetActiveScene().buildIndex;
        // wait 3 seconds and then switch to the next scene, just for testing
        // in the real game once you proceed to the next room you load another scene
        yield return new WaitForSeconds(3); 

        SceneManager.LoadScene(++currentScene);
    }
}
