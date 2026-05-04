using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TryAgainButton : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        gameObject.GetComponent<Button>().onClick.AddListener(() => TryAgain()); 
    }
    private async void TryAgain()
    {
       await AppInitializer.Initializer();
    }

    
}
