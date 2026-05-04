using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DateSlider : MonoBehaviour
{
    [Tooltip("Reference to the Slider component")]
    public Slider slider; // Reference to the Slider component
    [Tooltip("Reference to the TextMeshPro component to display the date")]
    public TextMeshProUGUI text; // Reference to the TextMeshPro component to display the date (optional, can be used instead of Text)
    public Button increaseButton; // Reference to the Button component for increasing the slider value
    public Button decreaseButton; // Reference to the Button component for decreasing the slider value
    private DateSliderManager manager; // Reference to the DateSliderManager
    private bool isInitialized = false; // Flag to check if the slider is initialized
    private bool isMinMaxSet = false; // Flag to check if min and max values are set


    void Start()
    {
        if (slider == null || text == null)
        {
            Debug.LogError("Slider or Text or the Button component is not assigned in the inspector.");
            gameObject.SetActive(false); // Disable the GameObject if the slider or text is not assigned
            return; // Exit if the slider or text is not assigned
        }
        slider.wholeNumbers = true; // Ensure the slider only allows whole numbers

    }
    /// <summary>
    /// Initialize the slider with a reference to the DateSliderManager.
    /// This method should be called before using the slider to ensure it is properly set up.
    /// </summary>
    /// <param name="manager"></param>
    public void InitializeSlider(DateSliderManager manager)
    {
        this.manager = manager; // Set the reference to the DateSliderManager
        slider.onValueChanged.AddListener(delegate { ChangeColor(); }); // Add a listener to the slider's value change event
        slider.onValueChanged.AddListener(delegate { NotifyToTheManager(); });
        // slider.onValueChanged.AddListener((value) => NotifyToTheManager()); // it can be also written like this
        // value must be put because the listener of a slider passes the value of the slider to the method      
        
        if (increaseButton != null)
        {
            increaseButton.onClick.AddListener(IncreaseValue); // Add a listener to the increase button
        }
        else
        {
            Debug.LogWarning("Increase Button is not assigned in the inspector.");
        }
        if (decreaseButton != null)
        {
            decreaseButton.onClick.AddListener(DecreaseValue); // Add a listener to the decrease button
        }
        else
        {
            Debug.LogWarning("Decrease Button is not assigned in the inspector.");
        }
        isInitialized = true; // Set the initialized flag to true
        return; // Exit the method after initializing the slider
        
    }
    public int GetValue()
    {
        return (int)slider.value; // Return the current value of the slider as an integer
    }
    /// <summary>
    /// Set the value of the slider and update the date text.   
    /// This method should be called after initializing the slider and setting the min and max values.
    /// </summary>
    public void SetValue(DateSliderManager manager, int value, string date)
    {
        if (this.manager != manager)
        {
            Debug.LogError("Manager reference does not match. Please initialize the slider with the correct manager.");
            return; // Exit if the manager reference does not match
        }
        if (value < slider.minValue || value > slider.maxValue)
        {
            Debug.LogWarning($"Value {value} is out of range. Setting to closest valid value.");
            slider.value = value < slider.minValue ? (int)slider.minValue : (int)slider.maxValue; // Clamp the value to the valid range
        }
        else
        {
            slider.value = value; // Set the slider value to the specified value
        }
        UpdateDateText(date); // Update the text to display the date
    }
    /// <summary>
    /// Set the minimum and maximum values for the slider.
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
    public void SetMinMaxValues(int min, int max)
    {
        slider.minValue = min; // Set the minimum value of the slider
        slider.maxValue = max; // Set the maximum value of the slider
        isMinMaxSet = true; // Set the isMinMax flag to true
        slider.value = min; // Set the initial value of the slider to the minimum value
        return; // Exit the method after setting min and max values

        // technically, I could put this in the InitializeSlider method
    }

    private void ChangeColor()
    {
        // This method can be used to change the color of the slider or text if needed
        // For now, it is empty, but you can implement color changes based on conditions
        return; // Exit the method
    }
    public void UpdateDateText(string date)
    {
        text.text = date; // Update the text component to display the date
        return;
    }

    public void IncreaseValue()
    {
        if (!isInitialized)
        {
            Debug.LogWarning("Slider is not initialized. Please call InitializeSlider first.");
            return; // Exit if the slider is not initialized
        }
        if (slider.value >= slider.maxValue)
        {
            Debug.LogWarning("Slider value is already at maximum. Cannot increase further.");
            return; // Exit if the slider value is already at maximum
        }
        else
        {
            slider.value++; // Increase the slider value by 1
            NotifyToTheManager(); // Notify the manager about the change
        }
    }

    public void DecreaseValue()
    {
        if (!isInitialized)
        {
            Debug.LogWarning("Slider is not initialized. Please call InitializeSlider first.");
            return; // Exit if the slider is not initialized
        }
        if (slider.value <= slider.minValue)
        {
            Debug.LogWarning("Slider value is already at minimum. Cannot decrease further.");
            return; // Exit if the slider value is already at minimum
        }
        else
        {
            slider.value--; // Decrease the slider value by 1
            NotifyToTheManager(); // Notify the manager about the change
        }
    }
    /// <summary>
    /// Notify the manager that the date has changed.
    /// This method is called when the slider value changes.
    /// </summary>
    private void NotifyToTheManager()
    {
        if (!isInitialized)
        {
            Debug.LogWarning("Slider is not initialized. Please call InitializeSlider first.");
            return; // Exit if the slider is not initialized
        }
        if (!isMinMaxSet)
        {
            // If min and max values are not set, it means that the user has not selected a year yet
            text.text = "Seleziona un anno"; // Update the text to prompt the user to select a year
            slider.value = slider.minValue; // Reset the slider value to the minimum value
            Debug.LogWarning("Min and Max values are not set. Please call SetMinMaxValues first.");
            return; // Exit if min and max values are not set
        }
        manager.Notify(this, (int)slider.value); // Notify the manager with the current slider value
        return; // Exit the method after notifying the manager
    }
}
