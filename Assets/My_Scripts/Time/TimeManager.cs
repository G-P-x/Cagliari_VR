using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UserInput;

public class TimeManager : MonoBehaviour
{
    [Tooltip("Campi di tempo che devono essere gestiti, IMPORTANTE CHE SIANO IN ORDINE!!!!!")]
    public TimeBehaviour[] timeBehaviours;
    private UserInput.InputFormat inputFormat = new UserInput.InputFormat();
    public TextMeshProUGUI textTime;
    public void SetTime(string input)
    {
        foreach (TimeBehaviour time in timeBehaviours)
        {
            time.SetTime(input);
        }
    }

    public string GetTimes()
    {
        string times = "";
        
        foreach (TimeBehaviour time in timeBehaviours)
        {
            times += time.SelectedTime + inputFormat.separatoreDATI;
        }        
        return times;
    }

    public void GetTime()
    {
        int i = 1;
        string times = "";
        foreach (TimeBehaviour time in timeBehaviours)
        {
            if (i < timeBehaviours.Length)
            {
                times += time.SelectedTime + inputFormat.separatoreDATI;
                i++;
            }
            else
            {
                times += time.SelectedTime;
            }
        }        
        textTime.text = times;
    }
}
