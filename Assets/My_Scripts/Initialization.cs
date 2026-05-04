using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class ServerResponse
{
    public short user_id; // Assuming the server returns a UserID in the response
}
public class Initialization : MonoBehaviour
{
    [SerializeField] private ServerConfig serverConfig; // Reference to the ServerConfig ScriptableObject
    [SerializeField] private SharedData sharedData; // Reference to the SharedData ScriptableObject
    private Coroutine initConversation;
    private void Awake()
    {
        initConversation = StartCoroutine(InitConversation());
    }

    private IEnumerator InitConversation()
    {
        // Initialize the conversation with the server
        string url = serverConfig.GetFullVRAssistantInitUrl();
        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error initializing conversation: " + www.error);
        }
        else
        {
            Debug.Log("Conversation initialized successfully: " + www.downloadHandler.text);
            var jsonResponse = JsonUtility.FromJson<ServerResponse>(www.downloadHandler.text);
            sharedData.UserID = jsonResponse.user_id; // Assuming the server returns a UserID in the response
            Debug.Log("User ID set to: " + sharedData.UserID);
        }
        if (www.isDone)
        {
            Debug.Log("Initialization request completed successfully.");
        }
        else
        {
            Debug.LogWarning("Initialization request did not complete successfully.");
        }
        initConversation = null; // Reset the coroutine reference after completion
    }


}
