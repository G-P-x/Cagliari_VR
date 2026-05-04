using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UserInput;
using TMPro;
using UnityEngine.UI;

public class DateManager : MonoBehaviour
{
    public DayBehaviour[] days;
    public MonthBehaviour[] months;
    public YearBehaviour[] years;

    public TextMeshProUGUI textDate;

    public void SetDay(string input)
    {
        foreach (DayBehaviour day in days)
        {
            day.SetDay(input);
        }
    }
    public void SetMonth(string input)
    {
        foreach (MonthBehaviour month in months)
        {
            month.SetMonth(input);
        }
    }

    public void SetYear(string input)
    {
        foreach (YearBehaviour year in years)
        {
            year.SetYear(input);
        }
    }

    public void GetDates()
    {
        try
        {
            string[] day = new string[2];
            string[] month = new string[2];
            string[] year = new string[2];

            int i = 0;
            foreach (DayBehaviour d in days)
            {
                day[i] = d.Day;
                i++;
            }
            i = 0;
            foreach (MonthBehaviour m in months)
            {
                month[i] = m.Month;
                i++;
            }
            i = 0;
            foreach (YearBehaviour y in years)
            {
                year[i] = y.Year;
                i++;
            }
            string initialDate = day[0] + "-" + month[0] + "-" + year[0];
            string finalDate = day[1] + "-" + month[1] + "-" + year[1];
            textDate.text = initialDate + " / " + finalDate;
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
        }
        
    }
}
