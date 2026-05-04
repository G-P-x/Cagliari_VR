using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Questo script va applicato ai Toggle dei giorni del mese. I button in MonthSelection.
/// </summary>
public class MonthButtonBehaviour : MonoBehaviour
{
    private Toggle myToggle;

    private string monthName;

    private DateManager dateSelection_DateManager;

    // Start is called before the first frame update
    void Start()
    {
        myToggle = GetComponent<Toggle>();
        monthName = gameObject.name;  // nome del button (es. "1", "2", "3", ecc.)
        myToggle.onValueChanged.AddListener(delegate { ToggleValueChanged(); });
        dateSelection_DateManager = GameObject.FindGameObjectWithTag("Date").GetComponent<DateManager>();
    }

    private void ToggleValueChanged()
    {
        if (myToggle.isOn)
        {
            // se il buttono viene selezionato chiama la mia funzione ed è true
            dateSelection_DateManager.SetMonth(monthName); // imposta il giorno selezionato                        
        }
    }
}
