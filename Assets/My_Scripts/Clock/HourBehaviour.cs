using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UserInput;

public class HourBehaviour : MonoBehaviour
{
    public TextMeshPro initialHour;
    public TextMeshPro finalHour;
    public TextMeshProUGUI textTime;
    private InputFormat inputFormat = new InputFormat();
    private readonly Dictionary<int, string> firstDigit = new Dictionary<int, string>
    {
        {0, "8:00"},
        {1, "9:00"},
        {2, "10:00"},
        {3, "11:00"},
        {4, "12:00"},
        {5, "13:00"},
        {6, "14:00"},
        {7, "15:00"},
        {8, "16:00"},
        {9, "17:00"},
        {10, "18:00"},
        {11, "19:00"},
        {12, "20:00"},
        {13, "21:00"},
        {14, "22:00"},
    };
    private readonly Dictionary<string, int> firstDigitReverse = new Dictionary<string, int>
    {
        {"", -1},
        {"8:00", 0},
        {"9:00", 1},
        {"10:00", 2},
        {"11:00", 3},
        {"12:00", 4},
        {"13:00", 5},
        {"14:00", 6},
        {"15:00", 7},
        {"16:00", 8},
        {"17:00", 9},
        {"18:00", 10},
        {"19:00", 11},
        {"20:00", 12},
        {"21:00", 13},
        {"22:00", 14},
    };

    private int initialFinal = 0;
    private void Start() 
    {
        initialHour.text = "";
        finalHour.text = "";
    }
    
    public void SetInitialOrFinalHour(GameObject gameObject)
    {
        Debug.Log("SetInitialOrFinalHour: " + gameObject.name);
        if (gameObject.CompareTag("StartTime"))
        {
            initialFinal = 0;
        }
        if(gameObject.CompareTag("StopTime"))
        {
            initialFinal = 1;
        }
    }

    public void SetHour(int hour)
    {
        if(hour < firstDigitReverse[initialHour.text])
        {
            initialFinal = 0;
            finalHour.text = initialHour.text;
        }
        if(hour > firstDigitReverse[finalHour.text] && finalHour.text != "" && initialFinal == 0)
        {
            initialFinal = 1;
            initialHour.text = finalHour.text;
        }
        if (initialFinal == 0)
        {
            initialHour.text = firstDigit[hour];
            initialFinal = 1;
        }
        else
        {
            finalHour.text = firstDigit[hour];
        }
    }

    public void ConfirmChoice()
    {
        textTime.text = initialHour.text + inputFormat.separatoreDATI + finalHour.text;
        initialHour.text = "";
        finalHour.text = "";
        initialFinal = 0;
        // Invoke(nameof(DisableMe), 0.5f); // nameof(DisableMe) instead of "DisableMe"
        gameObject.SetActive(false);
    }
    private void DisableMe()
    {
        gameObject.SetActive(false);
    }
}
