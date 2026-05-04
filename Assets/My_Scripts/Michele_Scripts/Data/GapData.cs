using System;

namespace DataSpace
{
    [Serializable]
    public struct StructDate
    {
        public int year;
        public int month;
        public int day;

        public StructDate(int year, int month, int day)
        {
            this.year = year;
            this.month = month;
            this.day = day;
        }

        public static implicit operator string(StructDate date)
        {
            return $"{date.year}-{date.month:D2}-{date.day:D2}";  //Con questo metodo implicito, posso utilizzare direttamente la chiamata a Date per ottenere la data completa,
                                                                  //in formato stringa, senza usare altri attributi in GapData
        }

        public static implicit operator DateTime(StructDate date)
        {
            return new DateTime(date.year, date.month, date.day);
        }
    }


    [Serializable]
    public class GapData
    {
        public StructDate Date { get; set; }
        public string Name { get; set; }
        public int People_in { get; set; }
        public int People_out { get; set; }
        public int People_unique { get; set; }
        public DateTime DateTime { get; set; }
        public TimeSpan TimeSlotStart { get; set; }
        public TimeSpan TimeSlotEnd { get; set; }
        public double Utm_east { get; set; }
        public double Utm_north { get; set; }

        public GapData(string name, double utm_east, double utm_north, int people_in, int people_out, int people_unique, DateTime dateTime, TimeSpan timeSlotStart, TimeSpan timeSlotEnd)
        {
            Name = string.Intern(name); // Internamento delle stringhe per riutilizzare le istanze delle stringhe duplicate.
            Utm_east = utm_east;
            Utm_north = utm_north;
            People_in = people_in;
            People_out = people_out;
            People_unique = people_unique;
            DateTime = dateTime;
            // Date = date;
            TimeSlotStart = timeSlotStart;
            TimeSlotEnd = timeSlotEnd;
        }
    }
}