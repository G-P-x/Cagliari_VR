using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UserInput;
using System;
using TMPro;
using Meta.WitAi.Utilities;


#if UNITY_EDITOR
using UnityEditor;
#endif


/// <summary>
/// Manages the date menu, retrieves user input from date sliders, and interacts with the SpawnManager to change gap colors.
/// This script is responsible for retrieving the date range selected by the user and updating the gaps in the scene based on that date range.
/// It uses the DateSliderManager to get the date range and the SpawnManager to get the gaps in the scene.
/// The gaps are then updated based on the data retrieved from the database.
/// </summary>
public class DateMenuManager : MonoBehaviour
{
    public DateSliderManager dateSliderManager; // Reference to the DateSliderManager in Date Menu
    public SpawnManager spawnManager; // Reference to the SpawnManager in Round Map to change the color of the gap
    public Button ConfirmButton; // Button to confirm the date selection
    public Image colorMapBackgroundImage; // Background image for the date menu
    // public Color intialColor;
    // public Color finalColor; // Color for the color map background image

    [Tooltip("Text components to display the minimum and maximum average accesses in the gaps related to the color map. 0: min, 1: max")]
    public TextMeshProUGUI[] textBar; // Text component to display the minimum and maximum average accesses in the gaps related to the color map
    private DataUsage dataFromDb = new();
    private InputFormat inputFormat = new(); // Instance of InputFormat to handle date formatting

    /// <summary>
    /// Dictionary to hold gap names and their corresponding GameObjects.
    /// </summary>
    private Dictionary<string, GameObject> gaps; // Dictionary to hold gap names and their corresponding GameObjects
    private List<Dictionary<string, object>> gapsData; // List to hold data for each gap
    private string[] dates = new string[2]; // Array to hold the start and end dates for the gaps
    private Coroutine retrieveDataCoroutine; // Coroutine to retrieve data from the database
    private Coroutine setGapsColorCoroutine; // Coroutine to set the gaps color based on the retrieved data
    private void Start()
    {
        ConfirmButton.onClick.AddListener(delegate { RetriveUserInputAndGapsInScene(); }); // Add listener to the Confirm button to retrieve user input and gaps in the scene
        dates = new string[2]; // Initialize the dates array with two elements
        gapsData = new List<Dictionary<string, object>>(); // Initialize the list to hold data for each gap
    }
    public void RetriveUserInputAndGapsInScene()
    {
        dates = inputFormat.FormatDate(dateSliderManager.GetDatesRange()); // Format the dates using InputFormat
        gaps = spawnManager.GetGapInScene(); // Get the gap name from the SpawnManager
        retrieveDataCoroutine = StartCoroutine(RetriveDataFromDb()); // Start the coroutine to retrieve data from the database
        return; // This method can be used to retrieve user input from the date sliders or other UI elements
    }

    [ContextMenu("Set ColorMapBackground Color")]
    void CreateColorMapBackground()
    {
# if UNITY_EDITOR
        if (Application.isPlaying)
        {
            Debug.LogError("This method should be called in Editor mode.");
            return; // Exit if not in Play mode
        }
        if (colorMapBackgroundImage == null)
        {
            Debug.LogError("Color Map Background Image is not assigned.");
            return; // Exit if the color map background image is not assigned
        }
        colorMapBackgroundImage.sprite = null; // Clear any existing sprite from the background image

        // create a gradient texture for the background image (horizontal gradient from black to white)
        int width = 100; // Width of the gradient texture
        int interval = width / 5; // Interval for color changes(20% of the width)
        int height = 20; // Height of the gradient texture
        Texture2D gradientTexture = new Texture2D(width, height); // Create
                                                                  // Create a horizontal gradient from green to red

        for (int x = 0; x < interval; x++)
        {
            float t = (float)x / (interval - 1); // Normalize x to [0, 1]
            Color _color = Color.Lerp(Color.cyan, Color.green, t); // Interpolate between green and blue based on x position
            for (int y = 0; y < height; y++)
            {
                gradientTexture.SetPixel(x, y, _color); // Set the pixel color in the texture
            }
        }
        for (int x = interval; x < 2 * interval; x++)
        {
            float t = (float)(x - interval) / (interval - 1); // Normalize x to [0, 1]
            Color _color = Color.Lerp(Color.green, Color.yellow, t); // Interpolate between blue and white based on x position
            for (int y = 0; y < height; y++)
            {
                gradientTexture.SetPixel(x, y, _color); // Set the pixel color in the texture
            }
        }
        for (int x = 2 * interval; x < 3 * interval; x++)
        {
            float t = (float)(x - 2 * interval) / (interval - 1); // Normalize x to [0, 1]
            Color _color = Color.Lerp(Color.yellow, new Color(1f, 0.57f, 0f), t); // Interpolate between white and red based on x position
            for (int y = 0; y < height; y++)
            {
                gradientTexture.SetPixel(x, y, _color); // Set the pixel color in the texture
            }
        }
        for (int x = 3 * interval; x < 4*interval; x++)
        {
            float t = (float)(x - 3 * interval) / (interval - 1); // Normalize x to [0, 1]
            Color _color = Color.Lerp(new Color(1f, 0.57f, 0f), Color.red, t); // Interpolate between red and black based on x position
            for (int y = 0; y < height; y++)
            {
                gradientTexture.SetPixel(x, y, _color); // Set the pixel color in the texture
            }
        }
        for (int x = 4 * interval; x < width; x++)
        {
            float t = (float)(x - 4 * interval) / (interval - 1); // Normalize x to [0, 1]
            Color _color = Color.Lerp(Color.red, new Color(1, 0, 1), t); // Interpolate between white and red based on x position
            for (int y = 0; y < height; y++)
            {
                gradientTexture.SetPixel(x, y, _color); // Set the remaining pixels to red
            }
        }
        gradientTexture.Apply(); // Apply the changes to the texture
        Sprite gradientSprite = Sprite.Create(gradientTexture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f)); // Create a sprite from the texture
        colorMapBackgroundImage.sprite = gradientSprite; // Assign the sprite to the background image
        EditorUtility.SetDirty(colorMapBackgroundImage); // Mark the background image as dirty to save changes in the editor
        Debug.Log("[Color] Color Map Background Image created and assigned."); // Log the creation and
#endif
    }
    private IEnumerator RetriveDataFromDb()
    {
        yield return null; // Wait for the next frame
        if (gaps == null || dates == null || dates.Length < 2)
        {
            Debug.LogError("Gaps or dates are not properly initialized.");
            yield break; // Exit the coroutine if gaps or dates are not properly initialized
        }
        int i = 0;
        gapsData = new List<Dictionary<string, object>>(gaps.Count); // Initialize the list to hold data for each gap
        Debug.Log($"[A] Retrieving data for {gaps.Count} gaps from {dates[0]} to {dates[1]}"); // Log the number of gaps and the date range being processed
        foreach (var gap in gaps)
        {
            var gapData = dataFromDb.GetGapAverageAffluenceInDateRange(gap.Key, dates[0], dates[1]); // Retrieve data for each gap by name and date range
            gapsData.Add(gapData); // Add the retrieved data to the list
            if (gapData == null || !gapData.ContainsKey("gap_name"))
            {
                Debug.Log($"[A] No data found for gap: {gap.Key} in the specified date range.");
            }
            else
            {
                Debug.Log($"[A] Data retrieved for gap: {gap.Key} - Average Accesses: {gapsData[i]["average_accesses"]}");
            }
            i++; // Increment the index for the next gap

            yield return new WaitForSeconds(0.1f); // Wait for a short time to avoid blocking the main thread

        }
        setGapsColorCoroutine = StartCoroutine(SetGapsColorAndColorMapLimits()); // Start the coroutine to set the gaps color based on the retrieved data
        Debug.Log("[A] Data retrieval and gap color setting completed."); // Log completion of data retrieval and gap color setting
        retrieveDataCoroutine = null; // Reset the coroutine reference to null after completion
        yield break; // Exit the coroutine after processing all gaps
    }
    /// <summary>
    /// Sets the color of gaps based on their average accesses and updates the color map limits.
    /// </summary>
    /// <returns></returns>
    private IEnumerator SetGapsColorAndColorMapLimits()
    {
        // find the smaller and larger average accesses in the gaps data
        float minAverageAccesses = 0f; // Initialize the minimum average accesses to 0, no one has entered the gap
        float maxAverageAccesses = 1350f; // based on the data in the database, the maximum dayly accesses is 450
        // foreach (var gapData in gapsData)
        // {
        //     if (gapData.ContainsKey("average_accesses"))
        //     {
        //         double averageAccesses = Convert.ToDouble(gapData["average_accesses"]);
        //         if (averageAccesses > maxAverageAccesses)
        //         {
        //             maxAverageAccesses = (float)averageAccesses; // Update the maximum average accesses found
        //         }
        //         if (minAverageAccesses == 0 || averageAccesses < minAverageAccesses)
        //         {
        //             minAverageAccesses = (float)averageAccesses; // Update the minimum average accesses found
        //         }
        //     }
        //     Debug.Log($"[A] Max Average Accesses: {maxAverageAccesses}"); // Log the maximum average accesses found
        //     Debug.Log($"[A] Min Average Accesses: {minAverageAccesses}"); // Log the minimum average accesses found
        //     yield return null; // Wait for the next frame before processing the gaps data
        // }
        foreach (var gapData in gapsData)
        {
            if (gapData.ContainsKey("gap_name") && gapData.ContainsKey("average_accesses"))
            {
                string gapName = gapData["gap_name"].ToString(); // Get the gap name
                double averageAccesses = Convert.ToDouble(gapData["average_accesses"]); // Get the average accesses for the gap
                if (gaps.ContainsKey(gapName))
                {
                    GameObject gapObject = gaps[gapName]; // Get the GameObject for the gap
                    if (gapObject != null)
                    {
                        // Change the color of the gap based on the average accesses
                        gapObject.GetComponent<MapGapBehaviour>().ChangeColorOnAverageAccess((float)averageAccesses, minAverageAccesses, maxAverageAccesses);
                        // Optionally, you can also log the gap name and its average accesses for debugging
                        Debug.Log($"[A] Gap: {gapName}, Average Accesses: {averageAccesses:2F}");
                    }
                    else
                    {
                        Debug.Log($"[A] Gap object for {gapName} not found in the scene.");
                    }
                }
            }
            yield return null; // Wait for the next frame to avoid blocking the main thread
        }
        if (textBar != null && textBar.Length == 2)
        {
            // Update the text bar with the minimum and maximum average accesses
            textBar[0].text = $"{minAverageAccesses:F2}"; // Set the minimum average accesses in the first text bar
            textBar[1].text = $"{maxAverageAccesses:F2}"; // Set the maximum average accesses in the second text bar
        }
        else
        {
            Debug.LogError("Text bar is not assigned or has insufficient elements.");
        }
    }
}
