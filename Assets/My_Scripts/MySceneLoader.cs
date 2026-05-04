using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MySceneLoader : MonoBehaviour
{
    [SerializeField] private float delay = 1.2f;
    [SerializeField] private Animator transition;
    // [SerializeField] private string sceneName;
    
    /// <summary>
    /// Load the scene running a close animation before
    /// </summary>
    /// <param name="sceneName"></param>
    public void LoadNextScene(string sceneName)
    {
        StartCoroutine(LoadSceneAfterDelay(sceneName));
    }

    IEnumerator LoadSceneAfterDelay(string sceneName)
    {
        // Play animation
        transition.SetTrigger("CloseScene");
        // Wait until the animation is finished
        yield return new WaitForSeconds(delay);
        // Load next scene
        SceneManager.LoadScene(sceneName);
    }
}
