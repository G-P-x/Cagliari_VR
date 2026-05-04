using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Utils;
using UnityEngine;
using UserInput;

namespace DataDownloader
{
    internal class DatabaseDownloader
    {
        private static bool writeError = false;
        private static readonly LogFormat logFormat = new();

        internal static async Task<HttpResponseMessage> DownloadData(string serverUrl = null)
        {
            string databaseDirectory = ReadFromJson.GetJsonParameter("Database");
            string logFilePath = ReadFromJson.GetJsonParameter("LogFile");
            serverUrl ??= ReadFromJson.GetJsonParameter("ServerUrl");

            HttpResponseMessage response = null;

            try
            {
                var credentials = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes("varco:collin"));
                var authHeaderValue = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);

                HttpClientHandler handler = new HttpClientHandler { ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true };

                using HttpClient client = new HttpClient(handler);
                client.DefaultRequestHeaders.Authorization = authHeaderValue;

                UnityEngine.Debug.Log($"{logFormat.databaseLog} Download del database in corso...");

                response = await client.GetAsync(serverUrl);
                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();
                var dataset = JsonConvert.DeserializeObject<Dataset>(responseBody);

                if (dataset?.Data != null)
                {
                    // UnityEngine.Debug.Log("Download del database completato");
                    // UnityEngine.Debug.Log("Aggiornamento del database in corso...");

                    await WriteDataToFileAsync(dataset.Data, databaseDirectory, logFilePath);

                    // UnityEngine.Debug.Log("Aggiornamento del database completato");

                    if (writeError)
                    {
                        UnityEngine.Debug.LogWarning($"{logFormat.databaseWarningLog} Si è verificato un errore durante il recupero di alcuni dati dal database.\n" +
                            "Per avere più dettagli sull'errore, consultare il file log.txt");
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerFile.LogError(logFilePath, $"DOWNLOAD DATA ERROR\nError --> {ex}\n\n\n");
                UnityEngine.Debug.LogError($"{logFormat.databaseErrorLog} Error during database download: {ex.Message}");
            }

            return response;
        }

        private static async Task WriteDataToFileAsync(List<List<string>> data, string databasePath, string logFilePath)
        {
            await Task.Run(() =>
            {
                Directory.CreateDirectory(Path.GetDirectoryName(databasePath));

                using FileStream fs = new(databasePath, FileMode.Create);
                using BinaryWriter writer = new(fs);

                // Scrive la data di ultimo aggiornamento come prima riga
                DateTime lastUpdate = DateTime.Now;
                writer.Write(lastUpdate.ToBinary());

                foreach (var row in data)
                {
                    try
                    {
                        string name = row[0];
                        string[] location = row[1].Replace("(", "").Replace(")", "").Trim().Split(',');
                        double utm_east = string.IsNullOrWhiteSpace(location[0]) ? double.NaN : double.Parse(location[0], CultureInfo.InvariantCulture);
                        double utm_north = string.IsNullOrWhiteSpace(location[1]) ? double.NaN : double.Parse(location[1], CultureInfo.InvariantCulture);
                        int people_in = int.Parse(row[2], CultureInfo.InvariantCulture);
                        int people_out = int.Parse(row[3], CultureInfo.InvariantCulture);
                        int people_unique = int.Parse(row[4], CultureInfo.InvariantCulture);
                        DateTime date = DateTime.Parse(row[5], CultureInfo.InvariantCulture);
                        var times = row[6].Split('-');
                        TimeSpan timeSlotStart = TimeSpan.FromHours(int.Parse(times[0]));
                        TimeSpan timeSlotEnd = TimeSpan.FromHours(int.Parse(times[1]));

                        writer.Write(name);
                        writer.Write(utm_east);
                        writer.Write(utm_north);
                        writer.Write(people_in);
                        writer.Write(people_out);
                        writer.Write(people_unique);
                        writer.Write(date.ToBinary());
                        writer.Write(timeSlotStart.Ticks);
                        writer.Write(timeSlotEnd.Ticks);
                    }
                    catch (Exception ex)
                    {
                        writeError = true;
                        string rowData = string.Join(", ", row);
                        LoggerFile.LogError(logFilePath, $"[Database] DATA WRITE ERROR\nData --> {rowData}\nError --> {ex}\n\n\n");
                    }
                }
            });
        }

        private class Dataset { public List<List<string>> Data { get; set; } = new List<List<string>>(); }
    }
}
