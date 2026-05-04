using System;
using System.Collections.Generic;
using System.Linq;
using DataSpace;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class DataUsage
{
    private readonly DataRequest dataRequest;
    public DataUsage()
    {
        dataRequest = new DataRequest();
    }
    /// <summary>
    /// Retrieves and aggregates data for a specific gap and time slot within an optional date range.
    /// </summary>
    /// <param name="timeSlot">The time slot to filter data by (e.g., "08:00-10:00").</param>
    /// <param name="gapName">The name of the gap to filter data by.</param>
    /// <param name="startdate">The start date of the range to filter data (optional, format: "yyyy-MM-dd").</param>
    /// <param name="enddate">The end date of the range to filter data (optional, format: "yyyy-MM-dd").</param>
    /// <returns>
    /// A dictionary containing aggregated data with the following keys:
    /// - "people_entered": The total number of people who entered.
    /// - "people_left": The total number of people who left.
    /// - "people_unique": The total number of unique people.
    /// - "latitude": The latitude of the selected gap.
    /// - "longitude": The longitude of the selected gap.
    /// </returns>
    /// <example>
    /// <code>
    /// var getDataInstance = new GetData();
    /// var data = getDataInstance.DataByTimeSlotAndDateRangeAndName("08:00-10:00", "GapName", "2023-01-01", "2023-01-31");
    /// 
    /// int peopleEntered = (int)data["people_entered"];
    /// int peopleLeft = (int)data["people_left"];
    /// int peopleUnique = (int)data["people_unique"];
    /// double latitude = (double)data["latitude"];
    /// double longitude = (double)data["longitude"];
    /// 
    /// Console.WriteLine($"People Entered: {peopleEntered}");
    /// Console.WriteLine($"People Left: {peopleLeft}");
    /// Console.WriteLine($"People Unique: {peopleUnique}");
    /// Console.WriteLine($"Latitude: {latitude}");
    /// Console.WriteLine($"Longitude: {longitude}");
    /// </code>
    /// </example>
    public Dictionary<string, object> DataByTimeSlotAndDateRangeAndName(string timeSlot, string gapName, string startdate = null, string enddate = null)
    {
        Dictionary<string, object> data = new();
        int people_entered = 0;
        int people_left = 0;
        int people_unique = 0;
        double location_lat = 0;
        double location_lon = 0;

        List<GapData> gap_list_by_time_slot = dataRequest.Select_GapDateTimeSlot(time_slot: timeSlot, gap_name: gapName, startDate: startdate, endDate: enddate);
        
        people_entered = gap_list_by_time_slot.Sum(gap_object => gap_object.People_in);
        people_left = gap_list_by_time_slot.Sum(gap_object => gap_object.People_out);
        people_unique = gap_list_by_time_slot.Sum(gap_object => gap_object.People_unique);

        double[] location = dataRequest.Get_GapCoordinates(gapName);

        if (location != null)
        {
            location_lat = location[0];
            location_lon = location[1];
        }

        data["people_entered"] = people_entered;
        data["people_left"] = people_left;
        data["people_unique"] = people_unique;
        data["latitude"] = location_lat;
        data["longitude"] = location_lon;

        // foreach (var item in data)
        // {
        //     Debug.Log($"{item.Key}: {item.Value}");
        // }

        return data;
    }

    /// <summary>
    /// Retrieves and aggregates data for a specific gap by name and an optional date range.    
    /// /// </summary>
    /// /// <param name="gapName">The name of the gap to filter data by.</param>
    /// <param name="startdate">The start date of the range to filter data (
    /// optional, format: "yyyy-MM-dd").</param>
    /// <param name="enddate">The end date of the range to filter data (
    /// optional, format: "yyyy-MM-dd").</param>
    /// <returns>
    /// A dictionary containing aggregated data with the following keys:
    /// - "gap_name": The name of the gap.
    /// - "average_accesses": The average number of people who entered the gap per unique date in the specified range.
    /// </returns>
  
    public Dictionary<string, object> GetGapAverageAffluenceInDateRange(string gapName, string startdate = null, string enddate = null)
    {
        Dictionary<string, object> data = new();
        float people_entered = 0;
        int dates_count = 0;
        float average = 0;

        List<GapData> gap_list_by_date_slot = dataRequest.Select_GapDateSlot(gap_name: gapName, startDate: startdate, endDate: enddate);

        people_entered = gap_list_by_date_slot.Sum(gap_object => gap_object.People_in);
        Debug.Log($"[A] DataUsage: People entered for gap {gapName}: {people_entered}");
        // Count unique dates
        dates_count = gap_list_by_date_slot
            .Select(gap_object => gap_object.DateTime.Date)
            .Distinct()
            .Count();
        Debug.Log($"[A] DataUsage: Unique dates for gap {gapName}: {dates_count}");
        
        DateTime start = DateTime.Parse(startdate);
        DateTime end = DateTime.Parse(enddate);
        float calendar_days_count = (float)(end - start).Days + 1; // Calculate the total number of days in the range +1 to include both start and end dates
        
        average = dates_count > 0 ? people_entered / calendar_days_count : 0; // Calculate average if dates_count is not zero
        Debug.Log($"[A] DataUsage: Average accesses for gap {gapName}: {average}");

        data["gap_name"] = gapName;
        data["average_accesses"] = average;
        return data;
    }
    public List<Dictionary<string, object>> GetAllGapsWithLocation()
    {
        return dataRequest.Get_AllGapsLocation();
    }

    public List<string> AvailableDates(string gapName)
    {
        return dataRequest.Get_AvailableDates(gapName);
    }

    public List<string> AvailableYears(string gapName)
    {
        return dataRequest.Get_AvailableYears(gapName);
    }

    public List<string> AvailableMonths(string gapName, string year)
    {
        return dataRequest.Get_AvailableMonths(gapName, year);
    }

    public List<string> AvailableDays(string gapName, string year, string month)
    {
        return dataRequest.Get_AvailableDays(gapName, year, month);
    }

}
