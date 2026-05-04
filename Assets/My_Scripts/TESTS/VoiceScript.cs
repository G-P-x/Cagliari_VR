using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Voice;

public class VoiceScript : MonoBehaviour
{
    public AppVoiceExperience voiceExperience;

    ///<summary>
    /// Initializes the voice experience and checks if it is assigned when
    /// left thumbs up gesture is detected.
    /// </summary>
    public void ActivateVoice()
    {
        if (voiceExperience != null)
        {
            voiceExperience.Activate();
        }
        else
        {
            Debug.LogWarning("Voice Experience is not assigned.");
        }
    }
}
