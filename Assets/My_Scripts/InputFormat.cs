using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal.Internal;

namespace UserInput
{
    public class CheckInput
    {
        private UserInput.InputFormat inputFormat = new();

        /// <summary>
        /// return a tuple (false, title, error message) if date 1 is less than date 2
        /// else return (true, "", "")
        /// </summary>
        public (bool, string, string) CheckDate(string date1, string date2)
        {
            try
            {
                string[] date1Array = date1.Split(inputFormat.dateSeparator.ToCharArray());
                string[] date2Array = date2.Split(inputFormat.dateSeparator.ToCharArray());

                if (date1Array.Length != 3 || date2Array.Length != 3)
                {
                    throw new System.Exception("Date format is not correct");
                }

                // check which date is greater, ricorda che la data è nel formato aaaa-mm-gg, vedi classe InputFormat
                if (int.Parse(date1Array[0]) > int.Parse(date2Array[0]))
                {
                    // aaaa_1 > aaaa_2
                    throw new System.Exception("Anno prima data maggiore di anno seconda data o non inserito");
                }
                else if(int.Parse(date1Array[0]) == int.Parse(date2Array[0]))
                {
                    if (int.Parse(date1Array[1]) > int.Parse(date2Array[1]))
                    {
                        throw new System.Exception("Mese prima data maggiore di mese seconda data nello stesso anno o non inserito");
                    }
                    else if (int.Parse(date1Array[1]) == int.Parse(date2Array[1]))
                    {
                        if (int.Parse(date1Array[2]) > int.Parse(date2Array[2]))
                        {
                            throw new System.Exception("Giorno prima data maggiore di giorno seconda data nello stesso mese e anno o non inserito");
                        }
                    }
                }
                return (true, "", "");
            }
            catch (System.Exception e)
            {
                return (false, "ERRORE INSERIMENTO DATA", e.Message) ;
            }            
        }

        /// <summary>
        /// return a tuple (false, title, error message) if the time 1 is less than time 2
        /// else return (true, "", "")
        /// </summary>
        /// <param name="timeInterval"></param>
        public (bool, string, string) LegitTime(string timeInterval)
        {            
            try
            {
                string[] time = timeInterval.Split("-".ToCharArray());
                if (time.Length != 2)
                {
                    throw new System.Exception("Time format is not correct");
                }
                int t1 = int.Parse(time[0]);
                int t2 = int.Parse(time[1]);
                if (t2 <= t1)
                {
                    throw new System.Exception("Time format is not correct, t2 <= t1");                    
                }
                return (true, "", "");
            }
            catch (System.Exception e)
            {
                return (false, "ERRORE INSERIMENTO ORA", e.Message);
            }
        }
    }

    
    public class InputFormat
    {
        public readonly string separatoreDATI = "/";
        public readonly string timeSeparator = ":";
        public readonly string dateSeparator = "-";
        public readonly string errorDateFormatCheck = "ERRORE";
        public readonly string errorTimeFormatCheck = "";
        /// <summary>
        /// Use this method to format a date string from "dd-mm-yyyy / dd-mm-yyyy" to "yyyy-mm-dd / yyyy-mm-dd".
        /// It splits the input date string by the specified separator and rearranges the date components.
        /// </summary>
        /// <param name="date"></param>
        /// <returns>
        /// A string array with two elements:
        /// - [0]: Start date in "yyyy-mm-dd" format
        /// - [1]: End date in "yyyy-mm-dd" format
        /// If an error occurs, returns an array with error information.
        /// </returns>
        public string[] FormatDate(string date)
        {
            // in ingresso l'intervallo di date è nel formato gg-mm-aaaa / gg-mm-aaaa
            string[] dateArray = date.Split(separatoreDATI.ToCharArray());
            try
            {
                string year = dateArray[0].Split(dateSeparator.ToCharArray())[2].Trim();
                string month = dateArray[0].Split(dateSeparator.ToCharArray())[1].Trim();
                string day = dateArray[0].Split(dateSeparator.ToCharArray())[0].Trim();

                dateArray[0] = year + dateSeparator + month + dateSeparator + day;

                year = dateArray[1].Split(dateSeparator.ToCharArray())[2].Trim();
                month = dateArray[1].Split(dateSeparator.ToCharArray())[1].Trim();
                day = dateArray[1].Split(dateSeparator.ToCharArray())[0].Trim();

                dateArray[1] = year + dateSeparator + month + dateSeparator + day;
                // restituisce un array con due elementi, la data di inizio e la data di fine
                // il formato è aaaa-mm-gg
                return dateArray;
            }
            catch (System.Exception)
            {
                string[] error = { errorDateFormatCheck, "ERRORE INSERIMENTO DATA" };
                return error;
            }

        }
        /// <summary>
        /// Use this method to format a array of string ["dd-mm-yyyy", " dd-mm-yyyy"] to ["yyyy-mm-dd", "yyyy-mm-dd"].
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public string[] FormatDate(string[] date)
        {
            // in ingresso l'intervallo di date è nel formato gg-mm-aaaa / gg-mm-aaaa
            try
            {
                string year = date[0].Split(dateSeparator.ToCharArray())[2].Trim();
                string month = date[0].Split(dateSeparator.ToCharArray())[1].Trim();
                string day = date[0].Split(dateSeparator.ToCharArray())[0].Trim();

                date[0] = year + dateSeparator + month + dateSeparator + day;

                year = date[1].Split(dateSeparator.ToCharArray())[2].Trim();
                month = date[1].Split(dateSeparator.ToCharArray())[1].Trim();
                day = date[1].Split(dateSeparator.ToCharArray())[0].Trim();

                date[1] = year + dateSeparator + month + dateSeparator + day;
                // restituisce un array con due elementi, la data di inizio e la data di fine
                // il formato è aaaa-mm-gg
                return date;
            }
            catch (System.Exception)
            {
                string[] error = { errorDateFormatCheck, "ERRORE INSERIMENTO DATA" };
                return error;
            }
        }
        public string FormatTime(string time)
        {
            try
            {
                // in ingresso l'intervallo di tempo è nel formato hh:mm / hh:mm
                string[] timeArray = time.Split(separatoreDATI.ToCharArray());
                string time1 = timeArray[0].Replace(":00", "").Trim();
                string time2 = timeArray[1].Replace(":00", "").Trim();
                // restituisce un array con due elementi, l'ora di inizio e l'ora di fine
                // l'intervallo di tempo è nel formato "hh-hh"
                string final = time1 + "-" + time2;

                return final;
            }
            catch (System.Exception)
            {
                return errorTimeFormatCheck;
            }
        }
        public string FormatGapName(string gap)
        {
            string formattedGap = gap.Replace("varco ", "gap").ToUpper().Trim();
            return formattedGap;
        }

        // use this if date is in the format d-m-yyyy
        public string CorrectDate(string date)
        {
            string[] newDate = date.Split(dateSeparator.ToCharArray());

            int day = int.Parse(newDate[0]);
            int month = int.Parse(newDate[1]);
            string sDay = "", sMonth = "";
            if (day < 10)
            {
                sDay = "0" + day;
            }
            else
            {
                sDay = day.ToString();
            }
            if (month < 10)
            {
                sMonth = "0" + month;
            }
            else
            {
                sMonth = month.ToString();
            }
            return sDay + dateSeparator + sMonth + dateSeparator + newDate[2];
        }
    }

    public class LogFormat
    {
        public readonly string databaseLog = "[D] ";
        public readonly string databaseErrorLog = "[D_E] ";
        public readonly string databaseReadyLog = "[D_R] ";
        public readonly string databaseWarningLog = "[D_W] ";
    }

}