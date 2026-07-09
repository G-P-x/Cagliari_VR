using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
public class MapGapBehaviour : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI gapText; // Text component to display the gap information
    private GameObject frameReference; // Reference to the frame object (Round Map)
    private Coroutine distanceCheckCoroutine;
    private const string frameTag = "Frame"; // Tag for the frame object
    private float distanceThreshold; // Example threshold for distance
    private MeshRenderer meshRenderer;
    private const float waitingTime = 0.5f; // Time to wait before checking distance again
    private Color lessThenMInColor = Color.cyan; // Color for low access
    private Color moreThenMaxColor = Color.red; // Color for high access
    private readonly float alphaValue = 0.6f; // Alpha value
    private HashSet<string> scenesInBuild = new(); // Set of scenes in the build
    // Start is called before the first frame update
    void Start()
    {
        frameReference = GameObject.FindGameObjectWithTag(frameTag);
        if (frameReference == null)
        {
            Debug.LogError("Frame object with tag 'Frame' not found in the scene.");
            return;
        }

        meshRenderer = gameObject.GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            Debug.LogError("MeshRenderer component not found on the gap object.");
            return;
        }
        lessThenMInColor.a = alphaValue; // Set alpha for low access color
        moreThenMaxColor.a = alphaValue; // Set alpha for high access color
        // cache material instance to avoid modifying shared material
        instanceMaterial = meshRenderer.material;
        // Fill the scenes set synchronously to avoid race conditions
        FillScenesInBuild();
        distanceCheckCoroutine = StartCoroutine(CheckDistance());
    }

    /// <summary>
    /// Checks the distance between the gap and the frame object.
    /// If the distance is less than the threshold, it shows the gap; otherwise, it hides it.
    /// This method runs in a coroutine to avoid checking every frame, which can be performance-intensive
    /// </summary>
    /// <returns></returns>
    private IEnumerator CheckDistance()
    {
        while (frameReference != null)
        {
            float distance = Vector3.Distance(transform.position, frameReference.transform.position);
            distanceThreshold = Mathf.Max(frameReference.transform.localScale.x, frameReference.transform.localScale.y, frameReference.transform.localScale.z) / 2f;
            // Adjust the distance threshold because the scale can vary
            if (distance < distanceThreshold) // Example threshold
            {
                meshRenderer.enabled = true; // Show the gap if within distance
            }
            else
            {
                meshRenderer.enabled = false; // Hide the gap if outside distance
            }

            yield return new WaitForSeconds(waitingTime); // Check every defined interval instead of every frame
        }
        distanceCheckCoroutine = null; // Stop the coroutine if the frameReference is destroyed
    }

    /// <summary>
    /// ClickVisual is called when the gap is clicked. 
    /// It is set to be called with a click event in the Unity Editor.
    /// </summary>
    public void ClickVisual()
    {
        // I'm going to check here if the gap scene is in the built scenes.
        // If not, I will change the color of the gap to a light gray to indicate that it is not implemented.
        // change the color of the gap when clicked
        // Ensure comparisons are case-insensitive and culture-invariant
        if (!scenesInBuild.Contains(gameObject.name.ToUpperInvariant()))
        {
            Color originalColor = instanceMaterial.color; // Store the original color
            // If the scene is not in the build, change the color to light gray
            Color lightGray = new Color(0.8f, 0.8f, 0.8f, alphaValue); // Light gray color with alpha
            instanceMaterial.color = lightGray;
            StartCoroutine(ResetGapColor(originalColor, lightGray)); // Start the coroutine to reset the color after a delay
            return; // Exit the method since the scene is not implemented
        }
        if (meshRenderer.isVisible)
        {
            Color newColor = Color.white; // Set the new color you want when clicked
            newColor.a = 0.6f; // Set alpha to make it semi-transparent
            instanceMaterial.color = newColor;

            // here I should add the logic to load the scene
        }
    }
    /// <summary>
    /// Change the color of the gap based on the average access value.
    /// This method is used to visually represent the average access of the gap.
    /// </summary>
    /// <param name="averageAccesses">The average accesses for the gap</param>
    /// <param name="minAccess">Use to compute the color gradient</param>
    /// <param name="maxAccess">Use to compute the color gradient</param>
    public void ChangeColorOnAverageAccess(float averageAccesses, float minAccess, float maxAccess)
    {
        if (meshRenderer != null && meshRenderer.material != null)
        {
            // Ensure the value is within the range
            float clampedValue = Mathf.Clamp(averageAccesses, minAccess, maxAccess);
            
            // Calculate normalized position (0 to 1) within the range
            float t = Mathf.InverseLerp(minAccess, maxAccess, clampedValue);
            
            // Create a smooth color gradient from cyan to red
            Color finalColor;
            Color orange = new (1f, 0.57f, 0); // Orange color
            Color violet = new (1f, 0, 1); // Violet color
            
            if (t <= 0.2f) // First 20% - cyan to green
            {
                float localT = t / 0.2f; // Normalize to 0-1 within this range
                finalColor = Color.Lerp(Color.cyan, Color.green, localT);
            }
            else if (t <= 0.4f) // Next 20% - green to yellow
            {
                float localT = (t - 0.2f) / 0.2f;
                finalColor = Color.Lerp(Color.green, Color.yellow, localT);
            }
            else if (t <= 0.6f) // Next 20% - yellow to orange
            {
                float localT = (t - 0.4f) / 0.2f;
                finalColor = Color.Lerp(Color.yellow, orange, localT);
            }
            else if (t <= 0.8f) // Next 20% - orange to red
            {
                float localT = (t - 0.6f) / 0.2f;
                finalColor = Color.Lerp(orange, Color.red, localT);
            }
            else // Final 20% - red (high values)
            {
                float localT = (t - 0.8f) / 0.2f;
                finalColor = Color.Lerp(Color.red, violet, localT);
            }
            
            // Apply alpha value
            finalColor.a = alphaValue;
            meshRenderer.material.color = finalColor;
            
            StartCoroutine(ReportPanelInfo(averageAccesses));
        }
    }
    private IEnumerator ReportPanelInfo(float averageAccess = 0f)
    {
        yield return new WaitForSeconds(0.1f); // Wait briefly before updating the text
        if (gapText != null)
        {
            gapText.text = $"Average accesses: {averageAccess:F2}"; // Set the gap name in the text component with formatted average access
        }
        else
        {
            Debug.LogWarning("Gap name text component is not assigned.");
        }
    }
    /// <summary>
    /// Fill the scenesInBuild HashSet synchronously to avoid timing issues.
    /// </summary>
    private void FillScenesInBuild()
    {
        scenesInBuild.Clear();
        int sceneCount = SceneManager.sceneCountInBuildSettings;
        for (int i = 0; i < sceneCount; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            scenesInBuild.Add(sceneName.ToUpperInvariant());
        }
    }
    private IEnumerator ResetGapColor(Color toRestore, Color newerColor)
    {
        const float holdBeforeFade = 0.5f; // seconds to hold the newer color
        const float duration = 0.5f; // fade duration in seconds
        yield return new WaitForSeconds(holdBeforeFade);
        float t = 0f;
        while (t < 1f)
        {
            instanceMaterial.color = Color.Lerp(newerColor, toRestore, t);
            t += Time.deltaTime / duration;
            yield return null;
        }
        instanceMaterial.color = toRestore;
        yield break; // Exit the coroutine
    }

    // cached material instance to avoid modifying the shared material
    private Material instanceMaterial;
}


