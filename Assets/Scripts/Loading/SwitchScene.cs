using UnityEngine;

public class SwitchScene : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // wait 3 seconds and then switch to the next scene, just for testing
        // in the real game once you proceed to the next room you load another scene
        // yield return new WaitForSeconds(3); 

        // SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex+1);
    }
}
