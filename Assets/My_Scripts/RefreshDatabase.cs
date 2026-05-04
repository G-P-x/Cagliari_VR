using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RefreshDatabase : MonoBehaviour
{
    private InteractableUnityEventWrapper refreshButton;
    private TextMeshPro txtButton;
    private ShowGapInfo showGapInfo;
    void Start()
    {
        refreshButton = GetComponent<InteractableUnityEventWrapper>();
        txtButton = GetComponentInChildren<TextMeshPro>();
        showGapInfo = GameObject.Find("ShowDataAndHuman").GetComponent<ShowGapInfo>();
    }
    public void Refresh()
    {
        StartCoroutine(RefreshDatabaseCoroutine());
        refreshButton.enabled = false;
        txtButton.text = "....";
    }
    private IEnumerator RefreshDatabaseCoroutine()
    {
        // play animation

        yield return AppInitializer.Initializer();
        refreshButton.enabled = true;
        txtButton.text = "AGGIORNA";
        showGapInfo.ShowInfo();
        // stop animation
    }
}
