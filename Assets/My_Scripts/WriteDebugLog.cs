using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WriteDebugLog : MonoBehaviour
{
    public TextMeshProUGUI logText; // Assign this in the inspector
    
    private string logMessages = "";
    public string stringFilter = "";

    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        if (stringFilter == "")
        {
            // we don't want to filter the logs
            logMessages += logString + "\n";
            logText.text += logMessages;
            return;
        }

        if (logString.Contains( stringFilter ))
        {
            logMessages += logString + "\n";
            logText.text += logMessages;
        }    
    }
}

    
