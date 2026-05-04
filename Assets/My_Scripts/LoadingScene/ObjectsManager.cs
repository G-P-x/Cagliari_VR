using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjectsManager : MonoBehaviour
{
    [SerializeField] private GameObject launchApplication;
    [SerializeField] private GameObject tryAgain, loading;
    private Button launchApplicationButton, tryAgainButton;
    private CanvasGroup launchApplicationCanvas, tryAgainCanvas;
    // Start is called before the first frame update
    void Start()
    {
        launchApplicationButton = launchApplication.GetComponent<Button>();
        tryAgainButton = tryAgain.GetComponent<Button>();
        launchApplicationCanvas = launchApplication.GetComponent<CanvasGroup>();
        tryAgainCanvas = tryAgain.GetComponent<CanvasGroup>(); 

        // start with these buttons disabled and faded
        launchApplicationButton.interactable = false;
        tryAgainButton.interactable = false;
        launchApplicationCanvas.alpha = 0.2f;
        tryAgainCanvas.alpha = 0.2f;
    }

    public void EnableLaunchApplicationButton()
    {
        launchApplicationButton.interactable = true;
        launchApplicationCanvas.alpha = 1f;
    }

    public void EnableTryAgainButton()
    {
        tryAgainButton.interactable = true;
        tryAgainCanvas.alpha = 1f;
    }

    public void StopAndDisableLoading()
    {
        loading.GetComponent<Animator>().SetTrigger("Stop");
        loading.SetActive(false);
    }
}
