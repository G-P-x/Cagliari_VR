using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction;
using UnityEngine;
using TMPro;

/// <summary>
/// This class is attached to the MonthPanel and YearPanel GameObjects.
/// </summary>
public class DatePanelsBehaviour : MonoBehaviour
{
    /// <summary>
    /// GameObjects MonthPanel and YearPanel.
    /// </summary>
    public GameObject DatePanel;
    public GameObject DaysPanel;
    private GameObject date;
    private GameObject monthPanel, yearPanel;
    private void Start() 
    {
        date = GameObject.Find("Data");
        gameObject.GetComponent<InteractableUnityEventWrapper>().WhenSelect.AddListener(OpenDate);
        monthPanel = GameObject.Find("MonthPanel");
        yearPanel = GameObject.Find("YearPanel");
    }

    /// <summary>
    /// Called whether MonthPanel or YearPanel is clicked.
    /// </summary>
    public void OpenDate()
    {
        // Close Day Panel
        DaysPanel.SetActive(false);
        // Open Date Panel
        DatePanel.SetActive(true);
    }

    /// <summary>
    /// Called when a month or a year is selected
    /// </summary>
    public void OpenDays(string month, string year)
    {
        Debug.Log("[STEP] 3: OpenDays");
        //read the month from the input field
        month ??= monthPanel.GetComponentInChildren<TextMeshPro>().text;
        // if (year == null)
        // {
        //     //read the year from the input field
        //     year = yearPanel.GetComponentInChildren<TextMeshPro>().text;
        // }
        year ??= yearPanel.GetComponentInChildren<TextMeshPro>().text;
        // Close Date Panel
        DatePanel.SetActive(false);
        // Open Day Panel
        DaysPanel.SetActive(true);
        date.GetComponent<Date>().UpdateDate(month, year);
    }
    
}
