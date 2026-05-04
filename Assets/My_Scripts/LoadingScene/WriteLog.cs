using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Attaccato all'oggetto che permette di visualizzare i log in tempo reale nel canvas
/// </summary>
public class WriteLog : MonoBehaviour
{
    private TextMeshProUGUI logText; // Assign this in the inspector
    private static readonly UserInput.LogFormat logFormat = new();
    /// <summary>
    /// Riferimento all'oggetto che gestisce enable/disable degli oggetti in scena (ObjectsManager attaccato)
    /// </summary>
    [SerializeField] private GameObject sceneObjectsManager;
    [SerializeField] private string log = logFormat.databaseLog;
    [SerializeField] private string logError = logFormat.databaseErrorLog;
    [SerializeField] private string logReady  = logFormat.databaseReadyLog;
    private ObjectsManager manager;
    void Start()
    {
        logText = GetComponent<TextMeshProUGUI>();
        logText.text = "";
        Application.logMessageReceived += HandleLog;
        manager = sceneObjectsManager.GetComponent<ObjectsManager>();
        
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        if (logString.Contains( log ))
        {
            logText.text += logString + "\n";
            return;
        }
        
        if (logString.Contains( logReady ))
        {
            logText.text += logString + "\n";
            if(sceneObjectsManager == null) return; // It means that the scene is not the loading scene
            manager.EnableLaunchApplicationButton();
            manager.StopAndDisableLoading();
            return;
        }

        if (logString.Contains( logError ))
        {
            sceneObjectsManager.GetComponent<ObjectsManager>().EnableTryAgainButton();
            logText.text += logString + "\n" + "Controlla lo stato della tua connessione e/o riprova\n" + "se l'applicazione è comunque pronta la puoi utilizzare usando l'ultimo database scaricato";
            // both buttons should be enabled
            manager.EnableLaunchApplicationButton();
            manager.EnableTryAgainButton();
            return;
        }
    }
}
