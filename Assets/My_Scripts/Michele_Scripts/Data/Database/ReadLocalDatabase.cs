using System.Collections.Generic;
using System.IO;
using System;
using System.Diagnostics;
using System.Threading;
using DataSpace;
using Utils;

public class ReadLocalDatabase
{
    public static List<GapData> dataList = new();
    private static DateTime lastUpdate;
    private static bool readError = false;

    public static List<GapData> ReadDatabase()
    {
        string databasePath = ReadFromJson.GetJsonParameter("Database");
        string logFilePath = ReadFromJson.GetJsonParameter("LogFile");
        dataList = new List<GapData>();

        int attempt = 0;
        while (attempt < 5)
        {
            try
            {
                if (!File.Exists(databasePath))
                {
                    throw new FileNotFoundException();
                }
                break; // Esci dal ciclo se il file esiste
            }
            catch
            {
                attempt++;
                UnityEngine.Debug.LogWarning($"Attempting to read the database. Please wait...");
                Thread.Sleep(3000); // Ritardo di 3 secondi tra i tentativi
            }
        }

        if (attempt >= 5)
        {
            UnityEngine.Debug.LogError($"[D_R] Unable to run the application: database file not found! Please download the database.");
            return dataList;
        }

        // Stopwatch stopwatch = Stopwatch.StartNew();

        try
        {
            using FileStream fs = new(databasePath, FileMode.Open);
            using BinaryReader reader = new(fs);

            // Legge la data di ultimo aggiornamento come prima riga
            lastUpdate = DateTime.FromBinary(reader.ReadInt64());

            while (fs.Position < fs.Length)
            {
                try
                {
                    string name = reader.ReadString();
                    double utm_east = reader.ReadDouble();
                    double utm_north = reader.ReadDouble();
                    int people_in = reader.ReadInt32();
                    int people_out = reader.ReadInt32();
                    int people_unique = reader.ReadInt32();
                    DateTime date = DateTime.FromBinary(reader.ReadInt64());
                    TimeSpan timeSlotStart = new(reader.ReadInt64());
                    TimeSpan timeSlotEnd = new(reader.ReadInt64());

                    var gapData = new GapData(name, utm_east, utm_north, people_in, people_out, people_unique, date, timeSlotStart, timeSlotEnd);
                    dataList.Add(gapData);
                }
                catch (Exception ex)
                {
                    readError = true;
                    LoggerFile.LogError(logFilePath, $"DATA ANALYSES ERROR\nError while reading data at position {fs.Position}\nError --> {ex}\n\n\n");
                }
            }
        }
        catch (Exception ex)
        {
            LoggerFile.LogError(logFilePath, $"DATA ANALYSES ERROR\nError --> {ex}\n\n\n");
            return dataList;
        }

        // stopwatch.Stop();
        // long elapsedTicks = stopwatch.ElapsedTicks;
        // double elapsedMicroseconds = elapsedTicks * 1000000.0 / Stopwatch.Frequency;
        // float elapsedMilliseconds = (float)elapsedMicroseconds / 1000;

        // UnityEngine.Debug.Log($"Tempo impiegato per creare gli oggetti: {stopwatch.Elapsed} --> ({elapsedMicroseconds} µs, {elapsedMilliseconds:F3} ms)");

        if (readError)
        {
            UnityEngine.Debug.LogWarning("Si è verificato un errore nell'analisi di alcuni dati durante la lettura del database.\n" +
                "Si consiglia di eseguire un aggiornamento del database. " +
                "Per avere più dettagli sull'errore, consultare il file log.txt");
        }

        return dataList;
    }

    public static DateTime GetLastUpdate { get { return lastUpdate; } }
}
