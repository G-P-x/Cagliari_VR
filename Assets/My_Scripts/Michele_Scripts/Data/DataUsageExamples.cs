using System.Collections.Generic;
using UnityEngine;
using DataSpace;
using System;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Linq;

public class DataUsageExamples
{
    private readonly DataRequest dataRequest;
    private readonly DataUsage dataUsage;
    public DataUsageExamples()
    {
        dataRequest = new DataRequest();
        dataUsage = new DataUsage();
    }
    public void TryData()
    {
        //Stopwatch stopwatch = Stopwatch.StartNew();
        /// inserire qui i metodi di test per verificare il tempo necessario all'esecuzione
        /// ...
        // stopwatch.Stop();
        // long elapsedTicks = stopwatch.ElapsedTicks;
        // double elapsedMicroseconds = elapsedTicks * 1000000.0 / Stopwatch.Frequency;
        // float elapsedMilliseconds = (float)elapsedMicroseconds / 1000;
        // UnityEngine.Debug.Log($"Tempo impiegato per iterare tutti gli oggetti: {stopwatch.Elapsed} --> ({elapsedMicroseconds} µs, {elapsedMilliseconds:F3} ms)");

        // GapByName();
        
        // GapByDate("2024-05-24");

        // DataUsage dataUsage = new();
        //dataUsage.DataByTimeSlotAndDateRangeAndName("10-18", "GAP1", "2024-05-30", "2024-06-03");

        //PeopleByDateRange("2023-07-20", "2024-02-21"); // Da notare che le date possono essere inserite anche invertite
        //PeopleByDateRangeAndGapName("GAP1", "2023-07-20", "2024-02-21");
        //PeopleByDateRange(startDate: "2023-07-20");  // Ottengo i dati dalla data selezionata, fino all'ultima data disponibile nel database
        //PeopleByDateRange(endDate: "2024-02-21");  // Ottengo i dati dalla prima data disponibile nel database, fino alla data selezionata

        // PeopleByTimeSlot("08-15");
        // PeopleByTimeSlotAndName("08-15", "GAP1");
        // PeopleByTimeSlotAndDate("08-15", "2024-02-21");
        // PeopleByTimeSlotAndDateRange("08-15", "2024-05-31", "2024-06-03");
        PeopleByTimeSlotAndDateRange("08-15", startdate:"2024-02-21");  // Passo la sola data inziale -> la data finale verrà considerata come l'ultima dipsonibile nel database
        // PeopleByTimeSlotAndDateRange("08-15", enddate:"2024-06-06");  // Passo la sola data finale -> la data iniziale verrà considerata come la prima dipsonibile nel database
        // PeopleByTimeSlotAndDateAndName("08-14", "2024-02-20", "GAP1");
        // PeopleByTimeSlotAndDateRangeAndName("08-18", "GAP1", "2024-05-31", "2024-06-03");
        // GetLocation("gap1");
        // PrintAllGapsWithLocation();
        // AvailableDates("GAP1");
        // AvailableYearsByName("GAP1");
        // AvailableMonthsByNameAndYear("GAP1", "2024");
        // AvailableDaysByNameAndYearAndMonth("GAP1", "2024", "06");
    }

    private void AvailableDates(string gapName)
    {
        List<string> availableDates = dataRequest.Get_AvailableDates(gapName);
        string dates = string.Join(", ", availableDates);
        UnityEngine.Debug.Log($"Available dates for {gapName} --> {dates}");
    }

    private void AvailableYearsByName(string gapName)
    {
        List<string> availableYears = dataRequest.Get_AvailableYears(gapName);
        string years = string.Join(", ", availableYears);
        UnityEngine.Debug.Log($"Available years for {gapName} --> {years}");
    }

    private void AvailableMonthsByNameAndYear(string gapName, string year)
    {
        List<string> availableMonths = dataRequest.Get_AvailableMonths(gapName, year);
        string months = string.Join(", ", availableMonths);
        UnityEngine.Debug.Log($"Available months for {gapName} in {year} --> {months}");
    }

    private void AvailableDaysByNameAndYearAndMonth(string gapName, string year, string month)
    {
        List<string> availableDays = dataRequest.Get_AvailableDays(gapName, year, month);
        string days = string.Join(", ", availableDays);
        UnityEngine.Debug.Log($"Available days for {gapName} in month {month} of the {year} --> {days}");
    }

    private void PrintAllGapsWithLocation()
    {
        List<Dictionary<string, object>> gaps = dataRequest.Get_AllGapsLocation();
        foreach (var gap in gaps)
        {
            UnityEngine.Debug.Log($"Gap Name: {gap["gap_name"]} --> Location: {gap["utm_north"]} - {gap["utm_east"]}");
        }
    }


    private double[] GetLocation(string gapName)
    {
        double[] coordinates = dataRequest.Get_GapCoordinates(gapName);

        if (coordinates.Length >= 2)
        {
            string coordinatesString = $"Utm East: {coordinates[0]}, Utm North: {coordinates[1]}";
            UnityEngine.Debug.Log(coordinatesString);
        }
        else
        {
            UnityEngine.Debug.Log("Coordinates array does not contain enough elements.");
        }

        return coordinates;
    }


    /// <summary>
    /// Inserire come argomento della funzione il nome del gap che si vuole filtrare per ottenere i dati
    /// </summary>
    /// <param name="gap_name"></param>
    private void GapByName(string gap_name = null)
    {
        UnityEngine.Debug.Log("---DATA USAGE TEST: GAP BY NAME---");

        List<GapData> gap_list_by_name = dataRequest.Select_Gap(gap_name);  // Richiedo tutti i dati relativi agli oggetti che presentano quel nome del GAP
                                                                            // e li salvo in una lista destinata a contenere oggetti di tipo GapData

        int count = 0;
        UnityEngine.Debug.Log($"{gap_list_by_name.Count} oggetti rispondono al nome {gap_name}");
        foreach (GapData gap_object in gap_list_by_name)  // Itero tutti gli oggetti di tipo GapData presenti nella lista
        {
            count += gap_object.People_in;
            // Ottengo tutti gli attributi realitivi agli oggetti filtrati con il nome selezionato
            // string date = gap_object.Date;
            // UnityEngine.Debug.Log($"{date}; {gap_object.Utm_east}, {gap_object.Utm_north}; {gap_object.People_unique}; {gap_object.Date.day}, {gap_object.Date.month}, {gap_object.Date.year};");
        }
        UnityEngine.Debug.Log($"Totale persone registrate in tutti in gap {gap_name} --> {count}");
    }


    private void GapByDate(string date)
    {
        UnityEngine.Debug.Log("---DATA USAGE TEST: GAP BY DATE---");

        List<GapData> gap_list_by_date = dataRequest.Select_Date(date);

        UnityEngine.Debug.Log($"{gap_list_by_date.Count} oggetti rispondono alla data {date}");
        foreach (GapData gap_object in gap_list_by_date)
        {
            // Ottengo tutti gli attributi che mi servono, realitivi agli oggetti filtrati con la data selezionata
            //UnityEngine.Debug.Log($"{gap_object.Utm_east}, {gap_object.Utm_north}; {gap_object.Time_slot}");
            //UnityEngine.Debug.Log($"Splitted time slot: {gap_object.Time_slot.Split('-')[0]} and {gap_object.Time_slot.Split('-')[1]}");
            UnityEngine.Debug.Log($"{gap_object.People_unique}; {gap_object.People_in}, {gap_object.People_out};");
        }
    }


    /// <summary>
    /// Esempio per ottenere i dati di tutti gap, corrispondenti all'intervallo di date selezionato.
    /// Formato data consigliato: "yyyy-MM-dd"
    /// </summary>
    /// <param name="startDate"></param>
    /// <param name="endDate"></param>
    private void PeopleByDateRange(string startDate = null, string endDate = null)
    {
        UnityEngine.Debug.Log("---DATA USAGE TEST: GAP BY DATE RANGE---");

        int totalPeople = 0;

        List<GapData> gap_list_by_date = dataRequest.Select_GapDateSlot(startDate: startDate, endDate: endDate);

        foreach (GapData gap_object in gap_list_by_date)
        {
            //UnityEngine.Debug.Log($"{gap_object.Date.year}-{gap_object.Date.month}-{gap_object.Date.day}");
            totalPeople += gap_object.People_unique;
        }

        UnityEngine.Debug.Log($"Totale persone registrate in tutti in gap nell'intervallo di date {startDate}/{endDate} --> {totalPeople}");
    }


    /// <summary>
    /// Esempio per ottenere i dati di un certo gap, corrispondenti all'intervallo di date selezionato
    /// </summary>
    /// <param name="gap_name"></param>
    /// <param name="startDate"></param>
    /// <param name="endDate"></param>
    private void PeopleByDateRangeAndGapName(string gap_name, string startDate, string endDate)
    {
        UnityEngine.Debug.Log("---DATA USAGE TEST: GAP BY DATE RANGE---");

        int totalPeople = 0;
        List<GapData> gap_list_by_date_and_name = dataRequest.Select_GapDateSlot(gap_name: gap_name, startDate: startDate, endDate: endDate);

        foreach (GapData gap_object in gap_list_by_date_and_name)
        {
            totalPeople += gap_object.People_unique;
        }

        UnityEngine.Debug.Log($"Totale persone registrate per il gap {gap_name}, nell'intervallo di date {startDate}/{endDate} --> {totalPeople}");
    }


    private void PeopleByTimeSlot(string timeSlot)
    {
        UnityEngine.Debug.Log("---DATA USAGE TEST: GAP BY TIME SLOT---");

        int totalPeople = 0;

        List<GapData> gap_list_by_time_slot = dataRequest.Select_GapDateTimeSlot(timeSlot);

        foreach (GapData gap_object in gap_list_by_time_slot)
        {
            totalPeople += gap_object.People_unique;
        }

        UnityEngine.Debug.Log($"Totale persone registrate in tutti in gap nell'intervallo orario {timeSlot} --> {totalPeople}");
    }


    private void PeopleByTimeSlotAndDate(string timeSlot, string date)
    {
        UnityEngine.Debug.Log("---DATA USAGE TEST: GAP BY TIME SLOT AND DATE---");

        int totalPeople = 0;

        List<GapData> gap_list_by_time_slot = dataRequest.Select_GapDateTimeSlot(time_slot: timeSlot, date: date);

        foreach (GapData gap_object in gap_list_by_time_slot)
        {
            totalPeople += gap_object.People_unique;
        }

        UnityEngine.Debug.Log($"Totale persone registrate in tutti in gap nell'intervallo orario {timeSlot} nella data {date} --> {totalPeople}");
    }


    private void PeopleByTimeSlotAndDateRange(string timeSlot, string startdate = null, string enddate = null)
    {
        UnityEngine.Debug.Log("---DATA USAGE TEST: GAP BY TIME SLOT AND DATE RANGE---");

        // int totalPeople = 0;

        // List<GapData> gap_list_by_time_slot = dataRequest.Select_GapDateTimeSlot(time_slot: timeSlot, startDate: startdate, endDate: enddate);

        int totalPeople = dataRequest.Select_GapDateTimeSlot(time_slot: timeSlot, startDate: startdate, endDate: enddate).Sum(gap => gap.People_in);

        // foreach (GapData gap_object in gap_list_by_time_slot)
        // {
        //     totalPeople += gap_object.People_unique;
        // }

        UnityEngine.Debug.Log($"Totale persone registrate in tutti in gap nell'intervallo orario {timeSlot} e nell'intervallo di date {startdate}/{enddate} --> {totalPeople}");
    }


    private void PeopleByTimeSlotAndName(string timeSlot, string gapName)
    {
        UnityEngine.Debug.Log("---DATA USAGE TEST: GAP BY TIME SLOT AND GAP NAME---");

        int totalPeople = 0;

        List<GapData> gap_list_by_time_slot = dataRequest.Select_GapDateTimeSlot(time_slot: timeSlot, gap_name: gapName);

        foreach (GapData gap_object in gap_list_by_time_slot)
        {
            totalPeople += gap_object.People_unique;
        }

        UnityEngine.Debug.Log($"Totale persone registrate nel gap {gapName}, all'intervallo orario {timeSlot} --> {totalPeople}");
    }


    private void PeopleByTimeSlotAndDateAndName(string timeSlot, string date, string gapName)
    {
        UnityEngine.Debug.Log("---DATA USAGE TEST: GAP BY TIME SLOT AND GAP NAME AND DATE---");

        int totalPeople = 0;

        List<GapData> gap_list_by_time_slot = dataRequest.Select_GapDateTimeSlot(time_slot: timeSlot, gap_name: gapName, date: date);

        foreach (GapData gap_object in gap_list_by_time_slot)
        {
            totalPeople += gap_object.People_unique;
        }

        UnityEngine.Debug.Log($"Totale persone registrate nel gap {gapName}, nell'intervallo orario {timeSlot} e nella data {date} --> {totalPeople}");
    }


    private void PeopleByTimeSlotAndDateRangeAndName(string timeSlot, string gapName, string startdate = null, string enddate = null)
    {
        UnityEngine.Debug.Log("---DATA USAGE TEST: GAP BY TIME SLOT AND GAP NAME AND DATE RANGE---");

        int totalPeople = dataRequest.Select_GapDateTimeSlot(time_slot: timeSlot, gap_name: gapName, startDate: startdate, endDate: enddate).Sum(gap => gap.People_unique);

        // foreach (GapData gap_object in gap_list_by_time_slot)
        // {
        //     totalPeople += gap_object.People_unique;
        // }

        UnityEngine.Debug.Log($"Totale persone registrate nel gap {gapName}, nell'intervallo orario {timeSlot} e nell'intervallo di date {startdate}/{enddate} --> {totalPeople}");
    }
}
