using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction;
using UnityEngine;

public class EnterGapAnimBehaviour : MonoBehaviour
{
    private GameObject loadScene;
    // Start is called before the first frame update
    void Start()
    {
        // Find the SceneLoader object and get the MySceneLoader component
        loadScene = GameObject.FindGameObjectWithTag("SceneLoader");
        if (loadScene == null)
        {
            Debug.LogError("SceneLoader object not found");
            return;
        }
        // Add a listener to the WhenSelect event of the InteractableUnityEventWrapper component
        gameObject.GetComponent<InteractableUnityEventWrapper>().WhenSelect.AddListener(()=> loadScene.GetComponent<MySceneLoader>().LoadNextScene(gameObject.name));
    }

}
