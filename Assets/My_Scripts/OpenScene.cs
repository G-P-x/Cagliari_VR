using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OpenScene : MonoBehaviour
{
    public void LoadGAP1()
    {
        SceneManager.LoadScene("GAP1", LoadSceneMode.Single);
    }

    public void LoadGAPAsync()
    {
        StartCoroutine(LoadSceneAsync(gameObject.name));
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        
        // Set the loaded scene as the active scene
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
    }
    public void OpenSceneButton() {
        // SceneManager.LoadScene("LoadingScene");
        SceneManager.LoadSceneAsync(gameObject.name, LoadSceneMode.Single);       
        }
}

       

