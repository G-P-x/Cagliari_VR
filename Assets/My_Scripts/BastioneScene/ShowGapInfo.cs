using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UserInput;

public class ShowGapInfo : MonoBehaviour
{
    /// <summary>
    /// mostra risutlati di ricerca
    /// </summary>
    public TextMeshPro gapInfoText;

    /// <summary>
    /// nome del varco
    /// </summary>
    public TextMeshProUGUI gapText;

    /// <summary>
    /// data di inizio e fine
    /// </summary>
    public TextMeshProUGUI dateText;

    /// <summary>
    /// fascia oraria
    /// </summary>
    public TextMeshProUGUI timeText;    
    public GameObject error; 
    public GameObject arrow;
    private TextMeshProUGUI[] errorText;
    private SpawnHuman spawnHuman;
    private DataUsage dataFromDb = new();
    private InputFormat inputFormat = new();
    private CheckInput checkInput = new();
    private Dictionary<string, object> data = new();

    public Dictionary<string, object> Data
    {
        get => data;
    }
    private void Start() 
    {
        arrow.SetActive(false);
        errorText = error.GetComponentsInChildren<TextMeshProUGUI>();
        error.SetActive(false);
        spawnHuman = GameObject.FindGameObjectWithTag("SpawnHumanObj").GetComponent<SpawnHuman>();
    }
    /// <summary>
    /// called on select the ShowDataAndHuman button and Refresh button
    /// </summary>
    public void ShowInfo()
    {
        error.SetActive(false);
        string gapName = inputFormat.FormatGapName(gapText.text);
        string[] selectedDataArray = inputFormat.FormatDate(dateText.text);
        string selectedTimeSlot = inputFormat.FormatTime(timeText.text);
        if (selectedDataArray[0] == inputFormat.errorDateFormatCheck || selectedTimeSlot == inputFormat.errorTimeFormatCheck)
        {
            PrintError("ERRORE", "Inserire data e fascia oraria");
            return;
        }

        (bool check, string title, string message) = checkInput.CheckDate(selectedDataArray[0], selectedDataArray[1]);
        if (!check)
        {
            PrintError(title, message);
            return;
        }
        (check, title, message) = checkInput.LegitTime(selectedTimeSlot);
        if (!check)
        {
            PrintError(title, message);
            return;
        }
        data = dataFromDb.DataByTimeSlotAndDateRangeAndName(selectedTimeSlot, gapName, selectedDataArray[0], selectedDataArray[1]);
        if (data.Count == 0)
        {
            PrintError("", "Nessun dato trovato per il periodo selezionato");
            return;
        }
        // block the spawn of human from the previous search
        spawnHuman.StopSpawn();
        // function called only if data is not empty or wrong
        spawnHuman.SpawnHumanPrefabs(data);
        // entrances and exits version
        // gapInfoText.text = $"BASTIONE DI SAINT REMI\n{dateText.text}\n{timeText.text}\n\n";
        // gapInfoText.text += $"persone entrate: {data["people_entered"]}\n";
        // gapInfoText.text += $"persone uscite: {data["people_left"]}\n";

        // only passages version
        gapInfoText.text = $"BASTIONE DI SAINT REMI\n{dateText.text}\n{timeText.text}\n\n";
        gapInfoText.text += $"passaggi: {data["people_entered"]}\n";

        

        // reset the text fields
        // dateText.text = "";
        // timeText.text = "";

        arrow.SetActive(true);

    }
    public void PrintError(string title, string message)
    {
        error.SetActive(true);
        errorText[0].text = title;
        errorText[1].text = message;
    }
}
