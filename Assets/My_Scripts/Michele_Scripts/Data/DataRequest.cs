using System.Collections.Generic;
using DataSpace;
using System.Threading.Tasks;
using System;
using UnityEngine;
using System.Linq;
using System.Globalization;


/// <summary>
/// classe che ricerca i dati nel database
/// </summary>
public class DataRequest : ReadLocalDatabase
{
    private static List<GapData> database;

    internal static async Task<int> InitializeDatabase()
    {
        // return await Task.Run(() =>
        // {
        //     if (database == null)
        //     {
        //         //ReadLocalDatabase localDatabase = new();
        //         database = ReadDatabase();
        //         if (database.Count > 0) return 1;
        //         else return -1;
        //     }
        //     else return 0; //"Database already initialized.";
        // });
        return await Task.Run(() =>
        {
            database = ReadDatabase();
            if (database.Count > 0) return 1;
            else return -1;
        });
    }


    /// <summary>
    /// Restituisce una lista con tutti i dati inerenti al Gap inserito. Se non viene inserito nessun nome, verrà restituito l'intero database.
    /// </summary>
    /// <param name="gap_name"></param>
    /// <returns></returns>
    public List<GapData> Select_Gap(string gap_name = null)
    {
        if (gap_name == null) return database;
        gap_name = gap_name.ToUpper().Trim();
        return database.FindAll(x => x.Name == gap_name);  // Il metodo FindAll restituisce una lista di dati, filtrati in base ad un certo parametro
    }


    /// <summary>
    /// Restituisce una lista con tutti i dati acquisiti alla data inserita
    /// </summary>
    /// <param name="date"></param>
    /// <returns></returns>
    public List<GapData> Select_Date(string date)
    {
        DateTime dateTime = DateTime.Parse(date, CultureInfo.InvariantCulture);
        return database.FindAll(x => x.DateTime == dateTime);
    }


    /// <summary>
    /// Restituisce una lista con tutti i dati di un gapObj alla data inserita
    /// </summary>
    /// <param name="gap_name"></param>
    /// <param name="date"></param>
    /// <returns></returns>
    public List<GapData> Select_GapDate(string gap_name, string date)
    {
        gap_name = gap_name.ToUpper().Trim();
        DateTime dateTime = DateTime.Parse(date, CultureInfo.InvariantCulture);
        return database.FindAll(x => x.Name == gap_name && x.DateTime == dateTime);
    }


    /// <summary>
    /// restituisce una lista dati in funzione dell'intervallo di giorni di un certo mese di un certo anno
    /// </summary>
    /// <param name="year"></param>
    /// <param name="month"></param>
    /// <param name="day1"></param>
    /// <param name="day2"></param>
    /// <returns></returns>
    public List<GapData> Select_Date_DaySlot(string year, string month, string day1, string day2)
    {
        int year_int = int.Parse(year);
        int month_int = int.Parse(month);
        int day1_int = int.Parse(day1);
        int day2_int = int.Parse(day2);
        return database.FindAll(x => x.DateTime.Date.Year == year_int && x.DateTime.Date.Month == month_int && day1_int <= x.DateTime.Date.Day && x.DateTime.Date.Day <= day2_int);
    }


    /// <summary>
    /// restituisce una lista dati in funzione dell'intervallo di giorni di un certo mese di un certo anno
    /// </summary>
    /// <param name="gap_name"></param>
    /// <param name="year"></param>
    /// <param name="month"></param>
    /// <param name="day1"></param>
    /// <param name="day2"></param>
    /// <returns></returns>
    public List<GapData> Select_GapDate_DaySlot(string gap_name, string year, string month, string day1, string day2)
    {
        gap_name = gap_name.ToUpper().Trim();
        int year_int = int.Parse(year);
        int month_int = int.Parse(month);
        int day1_int = int.Parse(day1);
        int day2_int = int.Parse(day2);
        return database.FindAll(x => x.Name == gap_name && x.DateTime.Date.Year == year_int && x.DateTime.Date.Month == month_int && day1_int <= x.DateTime.Date.Day && x.DateTime.Date.Day <= day2_int);
    }


    public List<GapData> Select_GapDateByYear(string gap_name, string year)
    {
        gap_name = gap_name.ToUpper().Trim();
        int year_int = int.Parse(year);
        return database.FindAll(x => x.Name == gap_name && x.DateTime.Date.Year == year_int);
    }


    /// <summary>
    /// Restituisce una lista dati in funzione dell'intervallo di date, e volendo anche per uno specifico gap.
    /// </summary>
    /// <param name="gap_name"></param>
    /// <param name="startDate"></param>
    /// <param name="endDate"></param>
    /// <returns></returns>
    public List<GapData> Select_GapDateSlot(string startDate = null, string endDate = null, string gap_name = null)
    {
        if (gap_name != null) gap_name = gap_name.ToUpper().Trim();
        List<GapData> GapByDateRangeList = new();
        DateTime startDateTime, endDateTime;

        if (startDate == null && endDate == null)
        {
            Debug.LogError("You must specify at least a start date or an end date");
            return null;
        }
        else if (startDate == null)
        {
            var sortedDatabase = database.OrderBy(entry => entry.DateTime);
            startDate = $"{sortedDatabase.First().DateTime}";  // Imposta come data iniziale la prima data disponibile nel database
        }
        else if (endDate == null)
        {
            var sortedDatabase = database.OrderBy(entry => entry.DateTime);
            endDate = $"{sortedDatabase.Last().DateTime}";  // Imposta come data finale l'ultima data disponibile nel database
        }

        try
        {
            (startDateTime, endDateTime) = (DateTime.Parse(startDate, CultureInfo.InvariantCulture), DateTime.Parse(endDate, CultureInfo.InvariantCulture));
            // Se la data iniziale è successiva alla data finale, inverto le date
            if (startDateTime > endDateTime) (endDateTime, startDateTime) = (startDateTime, endDateTime);
        }
        catch
        {
            Debug.LogError("Incorrect data input format.");
            return GapByDateRangeList;
        }

        var filteredData = database.Where(x => x.DateTime >= startDateTime && x.DateTime <= endDateTime);

        if (gap_name != null)
        {
            filteredData = filteredData.Where(x => x.Name == gap_name);
        }

        return filteredData.ToList();
    }

    public List<GapData> Select_GapDateTimeSlot(string time_slot, string gap_name = null, string date = null, string startDate = null, string endDate = null)
    {
        if (date != null && (startDate != null || endDate != null))
        {
            Debug.LogError("You cannot specify both a specific date and a date range.");
            return null;
        }

        List<GapData> GapByTimeSlotList = new();

        var times = time_slot.Split('-');
        TimeSpan startTime = TimeSpan.FromHours(int.Parse(times[0]));
        TimeSpan endTime = TimeSpan.FromHours(int.Parse(times[1]));

        // Se l'orario iniziale è successivo all'orario finale, inverto gli orari
        if (startTime > endTime) (endTime, startTime) = (startTime, endTime);

        IEnumerable<GapData> filteredData = database;

        if (gap_name != null)
        {
            gap_name = gap_name.ToUpper().Trim();
            filteredData = filteredData.Where(x => x.Name == gap_name);
        }

        if (date != null)
        {
            DateTime dateTime = DateTime.Parse(date, CultureInfo.InvariantCulture);
            filteredData = filteredData.Where(x => x.DateTime == dateTime);
        }
        else if (startDate != null || endDate != null)
        {
            filteredData = Select_GapDateSlot(startDate, endDate, gap_name);
        }

        filteredData = filteredData.Where(x => x.TimeSlotStart >= startTime && x.TimeSlotEnd <= endTime);

        return filteredData.ToList();
    }


    public double[] Get_GapCoordinates(string gapName)
    {
        try
        {
            var validGap = Select_Gap(gapName).FirstOrDefault(gapData => gapData != null && !double.IsNaN(gapData.Utm_east) && !double.IsNaN(gapData.Utm_north));

            if (validGap != null)
            {
                return new double[] { validGap.Utm_east, validGap.Utm_north };
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Non sono state trovate coordinate valide per {gapName}: {ex.Message}");
        }
        return new double[0];
    }

    /// <summary>
    /// Retrieves the unique locations of all gaps from the database.
    /// Each gap is represented by a dictionary containing its name and UTM coordinates (north and east).
    /// </summary>
    /// <returns>
    /// A list of dictionaries, each containing the following keys:
    /// - "gap_name": The name of the gap.
    /// - "utm_north": The UTM north coordinate of the gap.
    /// - "utm_east": The UTM east coordinate of the gap.
    /// </returns>
    public List<Dictionary<string, object>> Get_AllGapsLocation()
    {
        return database
            // Filtra i dati con nomi non nulli e coordinate valide
            .Where(data => !string.IsNullOrEmpty(data.Name) && 
                        !double.IsNaN(data.Utm_north) && 
                        !double.IsNaN(data.Utm_east))
            // Raggruppa per nome del gap per assicurarsi che ogni nome sia unico
            .GroupBy(data => data.Name)
            // Seleziona il primo elemento di ogni gruppo
            .Select(group => group.First())
            // Trasforma in un dizionario con i campi richiesti
            .Select(data => new Dictionary<string, object>
            {
                { "gap_name", data.Name },
                { "utm_north", data.Utm_north },
                { "utm_east", data.Utm_east }
            })
            // Converte il risultato in una lista
            .ToList();
    }


    /// <summary>
    /// Retrieves a list of available dates for a given gap name in the format "yyyy-MM-dd".
    /// </summary>
    /// <param name="gapName">The name of the gap for which to retrieve the dates.</param>
    /// <returns>A list of strings representing available dates in "yyyy-MM-dd" format.</returns>
    public List<string> Get_AvailableDates(string gapName)
    {
        return Select_Gap(gapName)
        .Select(gapData => $"{gapData.DateTime.Date.Year:D4}-{gapData.DateTime.Date.Month:D2}-{gapData.DateTime.Date.Day:D2}")
        .Distinct()
        .ToList();
    }
    

    /// <summary>
    /// Retrieves a list of available years for a given gap name.
    /// </summary>
    /// <param name="gapName">The name of the gap for which to retrieve the years.</param>
    /// <returns>A list of strings representing available years.</returns>
    public List<string> Get_AvailableYears(string gapName)
    {
        return Select_Gap(gapName)
        .Select(gapData => gapData.DateTime.Date.Year.ToString())
        .Distinct()
        .ToList();
    }

    /// <summary>
    /// Retrieves the count of unique dates available for a given gap name and year.
    /// </summary>
    /// <param name="gapName">The name of the gap for which to retrieve the unique date count.</param>
    /// <param name="year">The year for which to count the unique dates.</param>
    /// <returns>The number of unique dates available for the specified gap and year.</returns>
    public int Get_AvailableDatesCount(string gapName, string year)
    {
        int year_int = int.Parse(year);
        return Select_Gap(gapName)
            .Where(gapData => gapData.DateTime.Date.Year == year_int)
            .Select(gapData => gapData.DateTime.Date)
            .Distinct()
            .Count();
    }

    /// <summary>
    /// Retrieves a list of available months for a given gap name and year.
    /// </summary>
    /// <param name="gapName">The name of the gap for which to retrieve the months.</param>
    /// <param name="year">The year for which to retrieve the months.</param>
    /// <returns>A list of strings representing available months in "MM" format.</returns>
    public List<string> Get_AvailableMonths(string gapName, string year)
    {
        int year_int = int.Parse(year);
        return Select_Gap(gapName)
        .Select(gapData => gapData.DateTime)
        .Where(dateTime => dateTime.Date.Year == year_int)
        .Select(dateTime => dateTime.Date.Month.ToString("D2"))
        .Distinct()
        .ToList();
    }

    /// <summary>
    /// Retrieves a list of available days for a given gap name, year, and month.
    /// </summary>
    /// <param name="gapName">The name of the gap for which to retrieve the days.</param>
    /// <param name="year">The year for which to retrieve the days.</param>
    /// <param name="month">The month for which to retrieve the days.</param>
    /// <returns>A list of strings representing available days in "dd" format.</returns>
    public List<string> Get_AvailableDays(string gapName, string year, string month)
    {
        int year_int = int.Parse(year);
        int month_int = int.Parse(month);

        return Select_Gap(gapName)
            .Select(gapData => gapData.DateTime)
            .Where(dateTime => dateTime.Year == year_int && dateTime.Month == month_int)
            .Select(dateTime => dateTime.Day.ToString("D2"))
            .Distinct()
            .ToList();
    }

}
