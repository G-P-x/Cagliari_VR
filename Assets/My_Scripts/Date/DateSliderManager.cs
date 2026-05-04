using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UserInput;

/// <summary>
/// Set min and max for the date sliders. This script also retrives the dates from the database.
/// </summary>
public class DateSliderManager : MonoBehaviour
{
    public GameObject minDateObject; // Reference to the GameObject containing the min date slider
    public GameObject maxDateObject; // Reference to the GameObject containing the max date slider
    private string year = "2024"; // Default year for the date sliders, can be set from the menu
    private DateSlider minDate;
    private DateSlider maxDate;
    private InputFormat inputFormat = new(); // Instance of InputFormat to handle date formatting
    private const int minDay = 0; // Minimum day value for the date sliders
    private const int maxDay = 366; // Maximum day value for the date sliders 

    /// <summary>
    /// Array of date strings to be used in the sliders for test, must be substituted with the dates from the database.
    /// </summary>
    private readonly string[] datesArray = new string[maxDay]; // Array to hold date strings in "dd/MM" format for a leap year (366 days)};

    private void Start()
    {

        minDate = minDateObject.GetComponent<DateSlider>(); // Get the DateSlider component from the min date GameObject
        maxDate = maxDateObject.GetComponent<DateSlider>(); // Get the DateSlider component from
        StartCoroutine(Initialize()); // Start the initialization coroutine

    }
    private IEnumerator Initialize()
    {
        yield return null; // Wait for the next frame to ensure the GameObjects are completed their Start method

        if (minDate != null && maxDate != null)
        {
            int min = 0;
            int max = 366;
            DateTime startDate = new(2024, 1, 1); // leas year as example
            for (int i = min; i < max; i++)
            {
                datesArray[i] = startDate.AddDays(i).ToString($"dd{inputFormat.dateSeparator}MM"); // Popola l'array con le date in formato "dd/MM"
                if (i % 6 == 0)
                {
                    yield return null; // Yield every 6 iterations to avoid blocking the main thread
                    // This way it should take less than 1 second to complete the initialization without freezing the UI
                }
            }
            // 0 -> 01/01
            // 1 -> 02/01 ...
            // 365 -> 31/12
            minDate.InitializeSlider(this); // Initialize the min date slider with this manager
            maxDate.InitializeSlider(this); // Initialize the max date slider with this manager
            minDate.SetMinMaxValues(min, max - 1); // Set min and max values for minDate slider
            maxDate.SetMinMaxValues(min, max - 1); // Set min and max values for maxDate slider

            minDate.SetValue(this, min, datesArray[min]); // Set the initial value for the min date slider
            maxDate.SetValue(this, min, datesArray[min]); // Set the initial value for the max date slider
        }
        else
        {
            Debug.LogError("Min or Max Date GameObject is not assigned in the inspector.");
            gameObject.SetActive(false); // Disable the GameObject if minDate or maxDate is not assigned
            yield break; // Exit the coroutine if minDate or maxDate is not assigned
        }

    }
    /// <summary>
    /// Called when a year is selected from the menu.
    /// This method retrieves the data from the database and sets the min and max values for the sliders.
    /// It also sets the initial values for the min and max date sliders based on the selected year.
    /// The minDate slider will be set to the first date in the datesArray, and the maxDate slider will be set to the second date in the datesArray.
    /// If the minDate or maxDate GameObject is not assigned in the inspector, an error will be logged.
    /// </summary>
    /// <param name="year"></param>
    public void OnSelectedYear(string year)
    {
        if (minDate != null && maxDate != null)
        {
            this.year = year; // Set the selected year
            minDate.SetValue(this, minDay, datesArray[minDay]); // Set the initial value for the min date slider
            maxDate.SetValue(this, minDay, datesArray[minDay]); // Set the initial value for the max date slider
        }
        else
        {
            Debug.LogError("Min or Max Date GameObject is not assigned in the inspector.");
        }
    }

    /// <summary>
    /// Notifies to the DateSliderManager when a date slider value changes ensuring consistency between min and max date sliders.
    /// This method is called when the user interacts with the date sliders.
    /// </summary>
    public void Notify(DateSlider dateSlider, int value)
    {
        if (dateSlider == null || value < 0)
        {
            Debug.LogError("DateSlider is null in Notify method or value is less than 0.");
            return; // Exit if the dateSlider is null
        }
        if (dateSlider == minDate)
        {
            Debug.Log("Min Date Slider notified.");
            if (value > maxDate.GetValue())
            {
                maxDate.SetValue(this, value, datesArray[value]); // Set the max date slider to the next value
            }
            minDate.UpdateDateText(datesArray[value]); // Update the text for the min date slider
        }
        else if (dateSlider == maxDate)
        {
            Debug.Log("Max Date Slider notified.");
            if (value < minDate.GetValue())
            {
                minDate.SetValue(this, value, datesArray[value]); // Set the min date slider to the previous value
            }
            maxDate.UpdateDateText(datesArray[value]); // Update the text for the max date slider
        }
        else
        {
            // If the dateSlider is neither minDate nor maxDate, log an error and exit
            Debug.LogError("Received notification from an unrecognized DateSlider.");
            return;
        }
    }

    /// <summary>
    /// Retrieves the two dates based on the values of min and max slider.
    /// </summary>
    /// <returns></returns>
    public string[] GetDatesRange()
    {
        if (minDate == null || maxDate == null)
        {
            Debug.LogError("Min or Max Date Slider is not initialized.");
            return null; // Exit if minDate or maxDate is not initialized
        }

        int minValue = minDate.GetValue(); // Get the current value of the min date slider
        int maxValue = maxDate.GetValue(); // Get the current value of the max date slider

        if (minValue > maxValue)
        {
            Debug.LogWarning("Min date value is greater than Max date value. Returning empty range.");
            return new string[0]; // Return an empty array if min date is greater than max date
        }

        string fromDate = datesArray[minValue] + inputFormat.dateSeparator + year; // Get the date string for the min date
        string toDate = datesArray[maxValue] + inputFormat.dateSeparator + year; // Get the date string for the max date

        return new[] { fromDate, toDate }; // Return the date range as an array
    }
}
