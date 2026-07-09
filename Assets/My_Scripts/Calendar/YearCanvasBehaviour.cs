using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

/// <summary>
/// This class is attached to the YearCanvas GameObject.
/// </summary>
public class YearCanvasBehaviour : MonoBehaviour
{
    public GameObject yearToggle;
    private GameObject content;
    private GameObject yearPanel;
    public Date date;
    // private TextMeshProUGUI yearText;
    private List<GameObject> years = new List<GameObject>();
    private TextMeshPro yearPanel_text;
    private string[] availableYears = new string[] { "2021", "2022", "2023", "2024", "2025" };
    // Start is called before the first frame update
    void Start()
    {
        content = GameObject.Find("Content");
        yearPanel = GameObject.Find("YearPanel");
        yearPanel_text = yearPanel.GetComponentInChildren<TextMeshPro>();
        Assert.IsNotNull(content, "Content not found");
        Assert.IsNotNull(yearPanel, "YearPanel not found");  
        InstanciateNewYear();  
        // gameObject.SetActive(false);

        // l'oggetto deve essere attivo per poter accedere ai suoi componenti ed iniziliazzarli bene
        // altrimenti crea problemi con OnEnable alla prima attivazione           
    }

    void OnEnable()
    {
        Assert.IsNotNull(date, "Date not found in OnEnable method");
        int selectedYear = date.FirstYear;
        Debug.Log("[OnEnable] Selected Year: " + selectedYear);
        foreach (GameObject year in years)
        {
            if(selectedYear > int.Parse(year.name))
            {
                year.GetComponent<Toggle>().enabled = false;
            }
            else
            {
                year.GetComponent<Toggle>().enabled = true;
            }
        }
    }
    public void InstanciateNewYear()
    {
        years.Clear();
        foreach (string year in availableYears)
        {
            GameObject newYear = Instantiate(yearToggle, yearToggle.transform.position, Quaternion.identity);
            newYear.name = year;
            try
            {
                newYear.GetComponentInChildren<TextMeshProUGUI>().text = year;
                // YearBoxBehaviour yearBox = newYear.AddComponent<YearBoxBehaviour>();
                newYear.GetComponent<Toggle>().onValueChanged.AddListener(delegate { newYear.GetComponent<YearBoxBehaviour>().OnToggle(); });
            }
            catch
            {
                Debug.LogError("[Error] Component not found");
            }
            newYear.transform.SetParent(content.transform, false);
            years.Add(newYear);
        }
    }
    /// <summary>
    /// This method is called when a year is selected and isOn false->true, called from YearBoxBehaviour
    /// </summary>
    /// <param name="yearSelected"></param>
    public void OnToggleValueChanged(GameObject yearSelected)
    {
        Debug.Log("[STEP] 2: OnToggleValueChanged");
        foreach (GameObject year in years)
        {
            // deactivate all year toggles
            year.GetComponent<Toggle>().isOn = false;            
        }
        // write the selected year text in the year text field
        yearPanel_text.text = yearSelected.GetComponentInChildren<TextMeshProUGUI>().text;
        // close the year canvas
        gameObject.SetActive(false);
        yearPanel.GetComponent<DatePanelsBehaviour>().OpenDays(null, yearPanel_text.text);
    }
}
