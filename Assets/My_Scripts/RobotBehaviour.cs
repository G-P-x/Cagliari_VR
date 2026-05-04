using UnityEngine;
using System.Collections; // For Coroutines
using Meta.WitAi.TTS.Utilities; 

public class RobotBehaviour : MonoBehaviour
{
    public Transform playerHead; // Assign your VR camera/head transform here
    public float spawnDistanceBehindPlayer = 2.5f; // Further back for more dramatic entrance
    public float spawnHeightOffset = 0.5f; // Slightly above head
    public float targetDistanceInFront = 1.2f; // Comfortable viewing distance
    public float flightDuration = 1.0f; // How long the flight takes in seconds
    public float curveHeight = 0.75f; // How high it arcs during the flight
    public float leanAngle = 20f; // Max lean angle when turning (in degrees)
    public float rotationLerpSpeed = 10f; // How quickly the robot rotates to face its target
    public AudioSource flightSound; // Assign your flight sound here (optional)

    public Animator robotAnimator; // Assign your Animator component here

    public TTSSpeaker ttsSpeaker; // Optional: TTS Speaker for voice interaction

    // NEW: Offset rotation for models with non-standard forward axes
    // If your visual 'forward' is transform.right, you might need Quaternion.Euler(0, -90, 0)
    // If your visual 'forward' is transform.left, you might need Quaternion.Euler(0, 90, 0)
    // If your visual 'forward' is transform.up, you might need Quaternion.Euler(-90, 0, 0)
    // Adjust this based on your model's actual orientation relative to its local axes.
    private Quaternion modelForwardCorrection = Quaternion.Euler(0, 90, 0); // Assuming visual forward is local RIGHT

    private Coroutine currentFlightRoutine; // To manage stopping previous flights
    private Coroutine currentSoundFadeOutRoutine; // To manage sound fade out
    private Coroutine currentTTSRoutine; // To manage TTS playback

    public void CallRobot()
    {
        if (playerHead == null)
        {
            Debug.LogError("Player Head Transform is not assigned to MiniRobotFlight script! Please assign it in the Inspector.");
            return;
        }

        // Stop any ongoing flight routine to prevent conflicts
        if (currentFlightRoutine != null)
        {
            StopCoroutine(currentFlightRoutine);
        }

        // Deactivate first in case it was already active and being manipulated by another process
        gameObject.SetActive(false);

        // 1. Calculate Initial Spawn Position and Rotation
        // Spawn slightly off-center to create the "turning into view" effect
        // Example: Spawning to the player's right side initially
        Vector3 spawnOffsetDirection = playerHead.right;

        Vector3 initialSpawnPos = playerHead.position - playerHead.forward * spawnDistanceBehindPlayer
                                + playerHead.up * spawnHeightOffset
                                + spawnOffsetDirection * (spawnDistanceBehindPlayer / 3f); // Offset to the side

        transform.position = initialSpawnPos;
        // Start facing generally forward, or slightly towards the player to begin the arc
        // This is the initial "look direction" before the flight starts
        Quaternion initialLookRotation = Quaternion.LookRotation(playerHead.forward + (playerHead.position - initialSpawnPos).normalized);
        transform.rotation = initialLookRotation * modelForwardCorrection; // Apply correction here

        // 2. Calculate Target Position (in front of player)
        Vector3 targetPos = playerHead.position + playerHead.forward * targetDistanceInFront; // Modified previously

        // Make sure the robot is active before starting animation/movement
        gameObject.SetActive(true);
        // 3. Play flight sound if assigned
        if (flightSound != null)
        {
            flightSound.Play();
            flightSound.volume = 1f; // Set volume for flight sound
        }

        // Start the flight sequence
        currentFlightRoutine = StartCoroutine(FlyAndAnimateRoutine(initialSpawnPos, targetPos));
    }

    private IEnumerator FlyAndAnimateRoutine(Vector3 startPos, Vector3 endPos)
    {
        float timer = 0f;
        Vector3 previousPosition = startPos; // To calculate velocity for leaning

        // Calculate control point for the arc (Bezier curve)
        Vector3 controlPoint = (startPos + endPos) / 2f + Vector3.up * curveHeight;

        while (timer < flightDuration)
        {
            float normalizedTime = timer / flightDuration;

            // --- Movement (Arcing Path using Bezier) ---
            transform.position = CalculateQuadraticBezierPoint(startPos, controlPoint, endPos, normalizedTime);

            // --- Rotation (Facing Player + Leaning) ---
            Vector3 lookDirection = playerHead.position - transform.position;
            
            if (lookDirection != Vector3.zero)
            {
                // Calculate the base rotation to look at the player's head
                Quaternion targetLookRotation = Quaternion.LookRotation(lookDirection.normalized);

                // Calculate lean based on horizontal velocity (for "lowering to the side")
                Vector3 currentVelocity = (transform.position - previousPosition) / Time.deltaTime;
                Vector3 horizontalVelocity = new Vector3(currentVelocity.x, 0, currentVelocity.z); 
                
                float leanFactor = 0f;
                if (horizontalVelocity.magnitude > 0.01f) 
                {
                    // For leaning, we still want to use the robot's CURRENT local right/forward
                    // The lean applies *after* the modelForwardCorrection is applied to the base rotation.
                    // Project robot's current horizontal right vector onto the horizontal plane
                    Vector3 robotCurrentRightHorizontal = Vector3.ProjectOnPlane(transform.right, Vector3.up).normalized;
                    leanFactor = Vector3.Dot(horizontalVelocity.normalized, -robotCurrentRightHorizontal); 
                }
                
                Quaternion leanRotation = Quaternion.Euler(0, 0, leanFactor * leanAngle); 

                // Combine the base look rotation (corrected for model's forward) and the lean rotation
                // Apply modelForwardCorrection to the base look rotation FIRST, then apply lean.
                Quaternion finalTargetRotation = targetLookRotation * modelForwardCorrection * leanRotation;

                transform.rotation = Quaternion.Slerp(transform.rotation, finalTargetRotation, Time.deltaTime * rotationLerpSpeed); 
            }

            // --- Animation Trigger ---
            // if (robotAnimator != null)
            // {
            //     // robotAnimator.SetBool("IsFlying", true); # not implemented
            // }

            previousPosition = transform.position; 
            timer += Time.deltaTime;
            yield return null; 
        }

        // Ensure it's exactly at the target at the end
        transform.position = endPos;
        // Final look at player's head without lean for settled state
        if (playerHead != null)
        {
            Quaternion finalSettledLookRotation = Quaternion.LookRotation(playerHead.position - transform.position);
            transform.rotation = finalSettledLookRotation * modelForwardCorrection; // Apply correction to final pose
        }

        // --- Final Animation State ---

        if (robotAnimator != null)
        {
            // robotAnimator.SetBool("IsFlying", false); 
            robotAnimator.SetTrigger("Idle");
        }
        // --- Flight Sound ---
        currentSoundFadeOutRoutine = StartCoroutine(SoundFadeOut(flightSound, 1.0f, 0.1f)); // Fade out sound
        Debug.Log("Robot arrived and settled! Ready for interaction.");
        // --- TTS Interaction ---
        if (ttsSpeaker != null)
        {
            // Stop any previous TTS playback
            if (currentTTSRoutine != null)
            {
                StopCoroutine(currentTTSRoutine);
            }
            // Start a new TTS playback
            string introductionText = "Ciao, sono la tua guida virtuale! Posso aiutarti a esplorare questo mondo virtuale. Se hai bisogno di aiuto, chiedi pure!";
            currentTTSRoutine = StartCoroutine(TTSRoutine(introductionText));
            while (ttsSpeaker.IsSpeaking) // Wait until TTS is not speaking
            {
                yield return null; // Wait for the next frame
            }
        }
        currentFlightRoutine = null; 
    }

    

    private IEnumerator TTSRoutine(string text)
    {
        if (ttsSpeaker == null) yield break; // Exit if no TTS speaker assigned
        ttsSpeaker.Speak(text); // Start speaking the text
        while (ttsSpeaker.IsSpeaking) // Wait until TTS is not speaking
        {
            yield return null;
        }
        currentTTSRoutine = null; // Reset current TTS routine
    }

    private IEnumerator SoundFadeOut(AudioSource audioSource, float fadeDuration, float targetVolume = 0.1f)
    {
        if (audioSource == null || !audioSource.isPlaying) yield break; // Exit if no audio source or not playing
        float startVolume = audioSource.volume;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, targetVolume, elapsedTime / fadeDuration);
            yield return null;
        }
        audioSource.volume = targetVolume; // Ensure it ends at the target volume
        currentSoundFadeOutRoutine = null; // Reset current sound fade out routine
    }
    private Vector3 CalculateQuadraticBezierPoint(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        Vector3 p = uu * p0;
        p += 2 * u * t * p1;
        p += tt * p2;
        return p;
    }
}