using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Timeline;

public class GapInfo : MonoBehaviour
{
    public TextMeshPro gapText;
    // private DataUsage dataFromDb = new();
    private Dictionary<string, object> data = new();
    private string fromData = "2024-03-12";
    private string toData = "2024-03-12";
    private string timeSlot = "09-10";
    // Start is called before the first frame update
    void Start()
    {
        WriteInfo();
        // Invoke("WriteInfo", 20.0f);       
        
    }

    private void WriteInfo()
    {
        Debug.LogWarning("Writing info");
        // data = dataFromDb.DataByTimeSlotAndDateRangeAndName(timeSlot, "GAP1", fromData, toData);
        gapText.text = $"BASTIONE DI SAINT REMI\n{fromData} / {toData}\n{timeSlot}\n\n";
        gapText.text += $"persone entrate: {data["people_entered"]}\n";
        gapText.text += $"persone uscite: {data["people_left"]}\n";                        
    }
}
