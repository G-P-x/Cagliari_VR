using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions; // for Regex, more efficient and robust than string.Split
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;
using Meta.WitAi.TTS.Utilities;

/// <summary>
/// SendTextToServer is a MonoBehaviour that handles sending text to a server and processing the response using Text-to-Speech (TTS).
/// It uses a ScriptableObject to manage server configuration and can handle long text by splitting it into smaller chunks.
/// </summary>
public class SendTextToServer : MonoBehaviour
{
    [SerializeField] private ServerConfig serverConfig; // Reference to the ServerConfig ScriptableObject
    [SerializeField] private SharedData sharedData; // Reference to the SharedData ScriptableObject
    [SerializeField] private string textToSend;
    [SerializeField] TTSSpeaker ttsSpeaker;
    private Coroutine send_data;
    private readonly Queue<string> textQueue = new Queue<string>(); // Queue to hold text for TTS, referece doesn't change, the content does
    private readonly int chunkSize = 140; // Size of each chunk for TTS
    private Coroutine currentTTSRoutine;

    public void AskInformation(string text)
    {
        if (send_data != null)
        {
            StopCoroutine(send_data); // Stop the previous request if it's still running
        }
        send_data = StartCoroutine(SendData(text));
    }

    private IEnumerator SendData(string text)
    {
        text ??= textToSend; // if (text == null) {text = textToSend;}

        string url = serverConfig.GetFullVRAssistantChatUrl(); // Use the method to get the full URL for authentication
        WWWForm form = new();
        form.AddField("user_id", sharedData.UserID.ToString()); // Add the user ID from SharedData
        form.AddField("text", text);


        using UnityWebRequest www = UnityWebRequest.Post(url, form);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error sending data: " + www.error);
            currentTTSRoutine = StartCoroutine(TTSRoutineLongText("Error sending data: " + www.error)); // Handle error by speaking the error message
            yield break; // Exit the coroutine if there is an error
        }
        else
        {
            if (www.downloadHandler.text == null || www.downloadHandler.text == "")
            {
                Debug.LogWarning("Received empty response from the server.");
                yield break; // Exit the coroutine if the response is empty
            }
            Debug.Log("Data sent successfully: " + www.downloadHandler.text); // Log the response from the server
                                                                 // Optionally, you can handle the response here
            if (currentTTSRoutine != null)
            {
                StopCoroutine(currentTTSRoutine); // Stop any ongoing TTS routine
            }

            currentTTSRoutine = StartCoroutine(TTSRoutineLongText(www.downloadHandler.text)); // Start a new TTS routine
        }

        if (www.isDone)
        {
            Debug.Log("Request completed successfully.");
        }
        else
        {
            Debug.LogWarning("Request did not complete successfully.");
        }
        send_data = null; // Reset the coroutine reference
    }

    /// <summary>
    /// Coroutine to handle TTS for long text (above 140 chars) by splitting it into chunks.
    /// /// </summary>
    /// <param name="text">The text to be spoken.</param>
    private IEnumerator TTSRoutineLongText(string text)
    {
        if (ttsSpeaker == null)
        {
            Debug.LogWarning("TTSSpeaker is not assigned. Cannot speak the text.");
            yield break; // Exit the coroutine if TTSSpeaker is not assigned
        }

        // Split the text into chunks of 140 characters without braking words
        
        void SplitTextIntoSmartChunks(string text)
        {
            /// <summary>
            /// Fill the queue with smart chunks conciously broken.
            /// /// </summary>
            /// <param name="text">The text to be spoken.</param>

            // Split the text into sentences first to avoid breaking words
            string[] sentences = Regex.Split(text, @"(?<=[.!?])\s+");  // Use regex to split by punctuation and whitespace, keeps punctuation at the end of sentences
            // Iterate through each sentence and add it to the current chunk if it fits within 140 characters
            foreach (string sentence in sentences)
            {                
                if (sentence.Length <= chunkSize)
                {
                    textQueue.Enqueue(sentence); // Add the sentence to the queue
                }
                else
                {
                    // If the sentence itself is longer than the chunk size, split it into smaller parts
                    string[] subSentences = Regex.Split(sentence, @"(?<=[,;:])\s+"); // Split by punctuation and whitespace
                    string phase = string.Empty; // Reset current phase
                    foreach (string subSentence in subSentences)
                    {
                        if (phase.Length + subSentence.Length <= chunkSize)
                        {
                            phase += subSentence; // Add the sub-sentence to the current phase
                        }
                        else
                        {
                            // if I don't go in this if, it means that the sub-sentence is too long
                            // such that phase = 0 + subSentence.Length > chunkSize
                            if (phase.Length > 0)
                            {
                                textQueue.Enqueue(phase); // Add the current phase to the queue
                                phase = string.Empty; // Reset current phase
                                continue; // Skip to the next sub-sentence
                            }
                            // if I'm here, the sub-sentence is still too long.
                            string[] words = subSentence.Split(' '); // Split the sub-sentence into words
                            string wordChunk = string.Empty; // Reset current chunk
                            foreach (string word in words)
                            {
                                if (wordChunk.Length + word.Length + 1 <= chunkSize) // +1 for space
                                {
                                    if (wordChunk.Length > 0)
                                    {
                                        wordChunk += " "; // Add space before the next word
                                    }
                                    wordChunk += word.Trim();
                                }
                                else
                                {
                                    if (wordChunk.Length > 0)
                                    {
                                        textQueue.Enqueue(wordChunk); // Add the current word chunk to the queue
                                    }
                                    wordChunk = word.Trim(); // Start a new chunk with the current word
                                }
                            }
                            if (wordChunk.Length > 0)
                            {
                                textQueue.Enqueue(wordChunk); // Add any remaining chunk to the queue
                            }
                        }
                    }
                }

            }
        }

        SplitTextIntoSmartChunks(text);
        string[] textChunks = textQueue.ToArray();
        if (textChunks.Length == 0)
        {
            Debug.LogWarning("No text chunks to speak.");
            yield break; // Exit the coroutine if there are no text chunks
        }
        StartCoroutine(ttsSpeaker.SpeakQueuedAsync(textChunks)); // Speak all chunks in the queue asynchronously
        while (ttsSpeaker.IsSpeaking) // Wait until TTS is not speaking
        {
            yield return null;
        }
        // reset queue after speaking
        textQueue.Clear(); // Clear the queue after speaking
        currentTTSRoutine = null; // Reset current TTS routine
    }
}
