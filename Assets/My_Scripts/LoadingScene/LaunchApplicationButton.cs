using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Add a callback to SceneLoader to load the home scene passing the right scene name
/// </summary>
public class LaunchApplicationButton : MonoBehaviour
{
    /// <summary>
    /// SceneLoader object in the scene (MySceneLoader component is attached to it)
    /// </summary>
    [SerializeField] private MySceneLoader sceneLoader;
    /// <summary>
    /// Home scene
    /// </summary>
    [SerializeField] private string homeScene = "Home_1";
    // Start is called before the first frame update
    void Start()
    {
        gameObject.GetComponent<Button>().onClick.AddListener(() => sceneLoader.LoadNextScene(homeScene));
    }
}
