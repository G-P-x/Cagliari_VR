using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SetToggleButton : MonoBehaviour
{
    public DateSliderManager dateSlider; // Reference to the DateSliderManager
    private Toggle toggleButton; // Reference to the Toggle component
    private TextMeshProUGUI toggleText; // Optional: Reference to the TextMeshPro component for the toggle text
    // Start is called before the first frame update
    void Start()
    {
        toggleButton = GetComponent<Toggle>(); // Get the Toggle component from the GameObject
        toggleText = GetComponentInChildren<TextMeshProUGUI>(); // Get the TextMeshPro component from the children of the GameObject
        if (toggleButton == null)
        {
            Debug.LogError("Toggle component not found on the GameObject. Please ensure this script is attached to a GameObject with a Toggle component.");
            gameObject.SetActive(false); // Disable the GameObject if the Toggle component is not found
            return; // Exit if the Toggle component is not found
        }
        toggleText.text = gameObject.name.Trim(); // Set the text of the Toggle to the name of the GameObject, trimmed of whitespace
        if (dateSlider == null)
        {
            Debug.LogError("DateSliderManager reference is not assigned. Please assign it in the inspector.");
            gameObject.SetActive(false); // Disable the GameObject if the DateSliderManager is not assigned
            return; // Exit if the DateSliderManager is not assigned
        }
        toggleButton.isOn = false; // Set the Toggle to off by default
        toggleButton.onValueChanged.AddListener(delegate { WhenToggleOn(); }); // Add a listener to the Toggle's value change event
        // toggleButton.onValueChanged.AddListener((value)=>WhenToggleOn()); // Notify the DateSliderManager when the Toggle is turned on
    }
    
    private void WhenToggleOn()
    {
        if(toggleButton.isOn)
        {
            dateSlider.OnSelectedYear(gameObject.name.Trim());
        }
        
    }

}
