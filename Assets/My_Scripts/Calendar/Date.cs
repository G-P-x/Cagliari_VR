using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Oculus.Interaction;
using UnityEngine.Assertions;
using UserInput;
using Meta.XR.MRUtilityKit;
using Unity.VisualScripting;
using System.Linq;

/// <summary>
/// This class is attached to the Data GameObject in the scene.
/// </summary>
public class Date : MonoBehaviour
{
    public TextMeshProUGUI dateText;
    /*** Selezione anno ***/
    public GameObject yearsCanvas; // the parent object of the years in the calendar
    private List<GameObject> years = new(); // the years in the calendar

    /*** Selezione mese ***/
    public GameObject[] months; // the months in the calendar    
    private readonly Dictionary<int, string> monthsNumberFirst = new Dictionary<int, string>
    {
        {0, "Gennaio"},
        {1, "Febbraio"},
        {2, "Marzo"},
        {3, "Aprile"},
        {4, "Maggio"},
        {5, "Giugno"},
        {6, "Luglio"},
        {7, "Agosto"},
        {8, "Settembre"},
        {9, "Ottobre"},
        {10, "Novembre"},
        {11, "Dicembre"},
    }; // 0-based, 

    private readonly Dictionary<string, int> monthsNameFirst = new Dictionary<string, int>
    {
        {"Gennaio", 0},
        {"Febbraio", 1},
        {"Marzo", 2},
        {"Aprile", 3},
        {"Maggio", 4},
        {"Giugno", 5},
        {"Luglio", 6},
        {"Agosto", 7},
        {"Settembre", 8},
        {"Ottobre", 9},
        {"Novembre", 10},
        {"Dicembre", 11}
    };  // 0-based, 

    /*** Selezione giorno ***/
    private GameObject days; // the parent object of the days in the calendar (42 boxes)
    private InteractableUnityEventWrapper[] daysEvents;
    private TextMeshPro[] daysText; // 42 cells, 0 is Monday, 41 is Sunday
    private Color disableColor = new(0.5f, 0.5f, 0.5f, 0f);
    private List<int> firstDayOfMonths = new List<int>();  // first day of each month in the year

    /*** Data iniziale intervallo di date ***/
    private int firstMonth = -2; // 0-based, I use -2 so that the PreviousMonth function works correctly even if the first date is not selected
    // cannot use -1 because the first month, January, is 0, so -1 would be December then reset to 11
    private int firstYear = -1; // I use -1 so that the PreviousMonth function works correctly even if the first date is not selected
    private int firstBox; // the index of the first day selected

    /*** Selezione inserimento data iniziale o finale ***/
    private int startEnd = 0; // 0 = start date, 1 = end date - used to set the date in the correct text field
    
    /*** Formato data per ricerca nel database ***/
    private InputFormat inputFormat = new InputFormat(); // to format the date for the database
    private CheckInput checkInput = new CheckInput(); // to check the date format
    private string startDate = ""; 
    private string endDate = "";

    /*** Data selezionata mostrata nella scena ***/      
    // private GameObject selectedDate;
    public TextMeshPro selectedDatesStart;
    public TextMeshPro selectedDatesEnd;

    public TextMeshPro monthText; // from MonthPanel in the scene
    public TextMeshPro yearText;  // from YearPanel in the scene
    public TextMeshPro leftArrowText;
    public InteractableUnityEventWrapper leftArrowEvent;
    
    public List<GameObject> Years
    {
        set => years = value;
    }
    public int FirstYear
    {
        get => firstYear;
    }

    void Start()
    {
        // days
        days = GameObject.Find("Days");
        daysText = days.GetComponentsInChildren<TextMeshPro>();
        daysEvents = days.GetComponentsInChildren<InteractableUnityEventWrapper>();
        Assert.AreEqual(daysText.Length, 42);
        Assert.AreEqual(daysEvents.Length, 42);

        
        InitializeCalendar();
    }
    private void InitializeCalendar()
    {
        // calendar (Data) initialization on Gennaio 2024
        // questo dovrà essere aggiornato per tenere conto dei dati presenti nel database, verrà
        // quindi inizializzato con la prima data disponibile nel database
        FirstDayOfMonthsUpdate(2024);
        UpdateDays(0, 2024);
    }

    /// <summary>
    /// Select which date change, the initial or final one, depending on the button clicked in the scene (StartDate or StopDate)
    /// Default is the StartDate
    /// </summary>
    /// <param name="setDate"></param>
    public void SetInitialOrFinalDate(GameObject setDate)
    {
        // Set the selected date
        if (setDate.CompareTag("StartDate"))
        {
            // enable all events
            EnableAllEvents();
            startEnd = 0;
            // reset both text fields
            selectedDatesStart.text = "";
            selectedDatesEnd.text = "";
            // next time you click on a day, the first date will be reset            
        }
        else if (setDate.CompareTag("StopDate"))
        {
            startEnd = 1; 
            selectedDatesEnd.text = "";
        }
    }

    /******* 1. UPDATE CALENDAR 
    N.B. you must not pass months or years before the initial date. Theese possibilities must be disable before ********/

    /// <summary>
    /// Update the days of the month and year
    /// </summary>
    /// <param name="month"></param>
    /// <param name="year"></param>
    public void UpdateDays(int month, int year)
    {
        // Update the days of the month
        int firstDayOfMonth = firstDayOfMonths[month];
        int daysInMonth = DaysInMonth(month, year);
        int i = 0;
        for (i = 0; i < firstDayOfMonth; i++)
        {
            // color gray for days before the first day of the month
            // daysText[i].color = Color.gray;
            daysText[i].color = disableColor;        
            // previous month days
            int preMonth = month - 1;
            if (preMonth < 0)   
                preMonth = 11;
                year--;

            daysText[i].text = (DaysInMonth(preMonth, year) - firstDayOfMonth + i + 1).ToString();
            // disable the events for days before the first day of the month
            daysEvents[i].enabled = false;
        }
        for (i = firstDayOfMonth; i < daysInMonth + firstDayOfMonth; i++)
        {
            // color white for days of the month
            daysText[i].text = (i - firstDayOfMonth + 1).ToString();
            daysText[i].color = Color.white;
            daysEvents[i].enabled = true;
        }
        for (i = daysInMonth + firstDayOfMonth; i < daysText.Length; i++)
        {
            // color gray for days after the last day of the month
            // daysText[i].color = Color.gray;
            daysText[i].color = disableColor;
            daysText[i].text = (i - firstDayOfMonth - daysInMonth + 1).ToString();
            // disable the events for days after the last day of the month
            daysEvents[i].enabled = false;

        }
        // DisablePreviousDaysEvents();

    }    

    /*** 1.1. with arrows ***/

    /// <summary>
    /// Called when the RightArrow in the scene is clicked, update the month and year and the days of the month
    /// </summary>
    public void NextMonth()
    {
        // Increment the month        
        int next = monthsNameFirst[monthText.text] + 1;
        if (next > 11) // December
        {
            next = 0; // January next year
            // increment the year
            yearText.text = (int.Parse(yearText.text) + 1).ToString();
            FirstDayOfMonthsUpdate(int.Parse(yearText.text));  // because the year is updated
        }        
        monthText.text = monthsNumberFirst[next];
        UpdateDays(next, int.Parse(yearText.text));

        // enable the left arrow such that you can go back
        leftArrowText.color = Color.white;
        leftArrowEvent.enabled = true;
    }

    /// <summary>
    /// Called when LeftArrow in the scene is clicked, update the month and year and the days of the month
    /// </summary>
    public void PreviousMonth()
    {
        // Decrement the month
        int previous = monthsNameFirst[monthText.text] - 1;
        int currentYear = int.Parse(yearText.text); // not updated yet

        if (previous < 0) // January
        {
            FirstDayOfMonthsUpdate(currentYear - 1); // because the year is updated
            previous = 11; // December previous year            
            yearText.text = (currentYear - 1).ToString(); // decrement the year
            monthText.text = monthsNumberFirst[previous]; // aggiorno il mese a Dicembre nella scena
            if (IsThisFirstMonth(previous, currentYear -1)) 
            {
                // se il nuovo mese è lo stesso del mese selezionato, disabilito i giorni precedenti alla data selezionata
                // in questo caso, poiché passando da Gennaio 2024 a Dicembre 2023 cambia anche l'anno, devo tenerne conto
                UpdateDays(previous, currentYear - 1); // update the days of the month
                DisablePreviousDaysEvents();
                // leftArrowText.color = Color.gray;
                leftArrowText.color = disableColor;
                leftArrowEvent.enabled = false;
                return;
            }  
            UpdateDays(previous, currentYear - 1); // update the days of the month
            return;                     
        }

        monthText.text = monthsNumberFirst[previous];
        UpdateDays(previous, currentYear);
        if (IsThisFirstMonth(previous, currentYear))
        {
            // se il nuovo mese è lo stesso del mese selezionato, disabilito i giorni precedenti alla data selezionata
            DisablePreviousDaysEvents();
            // leftArrowText.color = Color.gray;
            leftArrowText.color = disableColor;
            leftArrowEvent.enabled = false;
        }
        
       
    }

    /*** 1.2. selecting a month or a year ***/

    /// <summary>
    /// Update the month and year when the selection is made via the MonthPanel or YearPanel
    /// </summary>
    /// <param name="month">string set in the Editor (gameObject.name)</param>
    /// <param name="year"></param>
    public void UpdateDate(string month, string year)
    {
        // check if the initial date is set
        // update month
        monthText.text = month;
        // update year
        int year_int = int.Parse(year);
        // yearText.text = year.ToString();
        FirstDayOfMonthsUpdate(year_int);
        UpdateDays(monthsNameFirst[month], year_int);
        if(IsThisFirstMonth(monthsNameFirst[month], year_int))
        {
            // se il nuovo mese è lo stesso del mese selezionato, disabilito i giorni precedenti alla data selezionata
            DisablePreviousDaysEvents();
            // leftArrowText.color = Color.gray;
            leftArrowText.color = disableColor;
            leftArrowEvent.enabled = false;
        }        
        
    }

    

    /****** 2. GET THE DATE INTERVAL ******/
    
    /// <summary>
    /// Called when a day is clicked, it set the selected date in the text field in the scene
    /// </summary>
    /// <param name="box"></param>
    public void SetSelectedDate(int box)
    {
        // Get date
        int month = monthsNameFirst[monthText.text];
        int year = int.Parse(yearText.text);
        int day = int.Parse(daysText[box].text);

        if (startEnd == 0)
        {
            // salvo la data per ricerca sul database
            startDate = day + inputFormat.dateSeparator + (month + 1) + inputFormat.dateSeparator + year;

            // scrivo la data nel testo nella scena
            selectedDatesStart.text = startDate;

            // imposto firstYear e firstMonth che vengono usati per disabilitare i giorni precedenti
            firstYear = year;
            firstMonth = month;
            firstBox = box;
            startEnd = 1; // next time you click on a day, the second date will be set

            DisablePreviousDaysEvents(); // anche firstBox posso passarlo come parametro
            leftArrowEvent.enabled = false; // Whatever the month, you cannot go back because you selected a new date in that month
            leftArrowText.color = disableColor;
            return;
        }
        if (startEnd == 1)
        {
            endDate = day + inputFormat.dateSeparator + (month + 1) + inputFormat.dateSeparator + year;
            selectedDatesEnd.text = endDate;
            return;
        }

        // selectedDate.GetComponentInChildren<TextMeshPro>().text = day + inputFormat.dateSeparator + (month + 1) + inputFormat.dateSeparator + year; 
        
    }

    /// <summary>
    /// Called when the Confirm button is clicked, it checks the date format and if the date is correct
    /// </summary>
    public void ConfirmChoice()
    {
        if (selectedDatesStart.text == "" || selectedDatesEnd.text == "") return;
        string newStart = inputFormat.CorrectDate(startDate);
        string newEnd = inputFormat.CorrectDate(endDate);        
        string[] newDates = inputFormat.FormatDate(newStart+inputFormat.separatoreDATI+newEnd);
        
        (bool b, string error, string message) = checkInput.CheckDate(newDates[0], newDates[1]);
        if (!b)
        {
            return;
        }
        
        dateText.text = newStart + inputFormat.separatoreDATI + newEnd;
        selectedDatesStart.text = "";
        selectedDatesEnd.text = "";
        startEnd = 0;
        startDate = "";
        endDate = "";
        gameObject.SetActive(false);
    }

    /****** ENABLE OR DISABLE FUNCTIONS ******/

    /// <summary>
    /// Return true if the year and the month are the same as the initial date, else false
    /// </summary>
    /// <param name="previous"></param>
    /// <param name="newYear"></param>
    /// <returns></returns>
    private bool IsThisFirstMonth(int previous, int newYear)
    {
        if (newYear == firstYear && previous == firstMonth)
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// Disable the events for the days before the first day selected
    /// </summary>
    /// <param name="box"></param>
    private void DisablePreviousDaysEvents()
    {
        // disable the days when you are in the same month of the first date selected
        for (int i = 0; i < firstBox; i++)
        {
            daysEvents[i].enabled = false;
            // daysText[i].color = Color.gray;
            daysText[i].color = disableColor;
        }
    }

    /// <summary>
    /// Called when MonthPanel is selected
    /// </summary>
    public void DisablePreviousMonthsEvents()
    {
        if (!(int.Parse(yearText.text) == firstYear)) return;  // if the year is different, all months are enabled
        // disable the months before the first month selected
        for(int i = 0; i < months.Length; i++)
        {
            if (firstMonth > monthsNameFirst[months[i].GetComponentInChildren<TextMeshPro>().text])
            {
                months[i].GetComponent<InteractableUnityEventWrapper>().enabled = false;
                // months[i].GetComponentInChildren<TextMeshPro>().color = Color.gray;
                months[i].GetComponentInChildren<TextMeshPro>().color = disableColor;
            }
        }
    }
    /// <summary>   
    /// Called when Start object is selected   
    /// </summary>
    private void EnableAllMonthsEvents()
    {
        // if a End date is selected, enable only previous months

        // enable all months
        for(int i = 0; i < months.Length; i++)
        {
            months[i].GetComponent<InteractableUnityEventWrapper>().enabled = true;
            months[i].GetComponentInChildren<TextMeshPro>().color = Color.white;
        }

    }

    /// <summary>
    /// Called when a new YearPanel is selected
    /// </summary>
    public void DisablePreviousYearsEvents()
    {
        // disable the years before the first year selected
        for (int i = 0; i < years.Count; i++)
        {
            if (firstYear > int.Parse(years[i].name))
            {
                years[i].GetComponent<InteractableUnityEventWrapper>().enabled = false;
                // years[i].GetComponentInChildren<TextMeshPro>().color = Color.gray;
                years[i].GetComponentInChildren<TextMeshPro>().color = disableColor;
            }
        }
    }

    private void EnableAllEvents()
    {
        foreach (InteractableUnityEventWrapper dayEvent in daysEvents)
        {
            UpdateDays(monthsNameFirst[monthText.text], int.Parse(yearText.text));            
        }        
        leftArrowEvent.enabled = true;
        leftArrowText.color = Color.white;
        firstYear = -1;
        firstMonth = -2;
        EnableAllMonthsEvents();

    }

    /****** UTILITY FUNCTIONS *****/

    private bool IsLeapYear(int year)
    {
        return year % 4 == 0 && year % 100 != 0 || year % 400 == 0;
    }
    private int DaysInMonth(int month, int year)
    {
        int[] daysInMonth = { 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };
        if (month < 0 || month > 11) return 0;
        if (month == monthsNameFirst["Febbraio"] && IsLeapYear(year)) return 29;
        return daysInMonth[month];
    }

    /// <summary>
    /// Calculate the first day of each month in the year
    /// </summary>
    /// <param name="year"></param>
    private void FirstDayOfMonthsUpdate(int year)
    {
        // FUNZIONA CORRETTAMENTE
        int[] t = { 0, 3, 2, 5, 0, 3, 5, 1, 4, 6, 2, 4 };
        firstDayOfMonths.Clear();
        for (int month = 0; month < 12; month++)  // Mesi da 0 (Gennaio) a 11 (Dicembre)
        {
            int adjustedYear = year;  // Zeller formula works with March as 1, February as 12
            if (month < 2)
            {
                adjustedYear -= 1;
            }

            int firstDay = (adjustedYear + adjustedYear / 4 - adjustedYear / 100 + adjustedYear / 400 + t[month] + 1) % 7;
            // Adattamento per avere Lunedì come 0
            firstDay = (firstDay + 6) % 7;
            firstDayOfMonths.Add(firstDay);
        }
    }
}
