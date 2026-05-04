using UnityEngine;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Globalization;

namespace Utils
{

    public class FileUtility
    {
        /// <summary>
        /// Attende l'esistenza dei file specificati asincronamente.
        /// </summary>
        /// <param name="paths">Array di percorsi dei file da controllare.</param>
        /// <param name="checkExistenceFunc">Funzione che controlla l'esistenza di un file dato il percorso.</param>
        /// <param name="maxAttempts">Numero massimo di tentativi per verificare l'esistenza dei file (default: 6).</param>
        /// <param name="delayMilliseconds">Intervallo di tempo in millisecondi tra i tentativi di verifica (default: 5000).</param>
        /// <returns>True se tutti i file esistono, altrimenti false.</returns>
        internal static async Task<bool> WaitForExistenceAsync(string[] paths, Func<string, bool> checkExistenceFunc, int maxAttempts = 6, int delayMilliseconds = 5000)
        {
            if (paths.All(checkExistenceFunc)) return true; // Se tutti i file esistono già, non c'è bisogno di attendere

            int attempt = 0;
            while (attempt < maxAttempts)
            {
                if (paths.All(checkExistenceFunc)) return true; // Se tutti i file esistono, possiamo uscire dal ciclo
                // Altrimenti, si avvia una nuova verifica, dopo un certo tempo
                attempt++;
                await Task.Delay(delayMilliseconds);
            }
            return false; // Se dopo un certo numero di tentativi la condizione non è stata verificata, la funzione restituisce false
        }
    }


    public class ReadFromJson
    {
        /// <summary>
        /// Estrae il valore del parametro specificato dal file JSON di configurazione.
        /// </summary>
        /// <param name="jsonKey">La chiave del parametro da cercare.</param>
        /// <returns>Il valore del parametro corrispondente alla chiave specificata. Se la chiave non esiste o si verifica un errore, restituisce null.</returns>
        /// <remarks>
        /// Parametri disponibili nel file JSON di configurazione:
        /// - "Database": Il percorso del database.
        /// - "LogFile": Il percorso del file di log.
        /// - "ServerUrl": L'URL del server.
        /// </remarks>
        public static string GetJsonParameter(string jsonKey)
        {
            try
            {
                string jsonFilePath = JsonConfig.GetJsonFilePath;

                if (!File.Exists(jsonFilePath))
                {
                    Debug.LogError($"Il file JSON non esiste: {jsonFilePath}");
                    return null;
                }

                string jsonData = File.ReadAllText(jsonFilePath);
                JObject jsonObject = JObject.Parse(jsonData);

                if (jsonObject["Parameters"] is JObject parameters)
                {
                    if (parameters.TryGetValue(jsonKey, out var value))
                    {
                        return value.ToString();
                    }
                    else
                    {
                        Debug.LogWarning($"Chiave '{jsonKey}' non trovata nel file JSON.");
                        return null;
                    }
                }
                else
                {
                    Debug.LogError($"Nessun parametro 'Parameters' trovato nel file JSON.");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Errore durante l'estrazione del valore JSON con chiave '{jsonKey}':\n{ex}");
                return null;
            }
        }
    }

    public class ValidateData
    {
        public static void CheckDataFormat(string[] data)
        {
            List<string> errorMessages = new();

            // Controllo del formato dell'orario
            try
            {
                string[] splitted_time = data[7].Split("-");
                if (splitted_time[0].Length < 2 || splitted_time[1].Length < 2)
                {
                    errorMessages.Add($"Errore nel formato dell'orario: {data[7]}. Il formato dell'orario dev'essere HH-HH.");
                }
            }
            catch (Exception ex)
            {
                errorMessages.Add($"Errore durante la validazione dell'orario, assicurarsi che il formato dell'orario sia HH-HH: {ex.Message}");
            }

            // Aggiungi altre verifiche qui
            try
            {
                int people_unique = int.Parse(data[5], CultureInfo.InvariantCulture);
                if (people_unique < 0)
                {
                    errorMessages.Add($"Errore nel numero di persone uniche: {data[5]}. Il numero deve essere un valore positivo.");
                }
            }
            catch (Exception ex)
            {
                errorMessages.Add($"Errore durante la validazione del numero di persone uniche: {ex.Message}");
            }

            // Se ci sono messaggi di errore, lancia un'eccezione
            if (errorMessages.Count > 0)
            {
                throw new Exception(string.Join("\n", errorMessages));
            }
        }
    }

    public static class LoggerFile
    {
        private static readonly object logLock = new();

        public static void LogError(string logFilePath, string message)
        {
            lock (logLock)
            {
                try
                {
                    using StreamWriter writer = new(logFilePath, true);
                    writer.WriteLine($"{DateTime.Now}: {message}");
                }
                catch (Exception ex)
                {
                    // Handle any exceptions that might occur during logging.
                    Console.WriteLine($"Failed to write to log file: {ex.Message}");
                }
            }
        }
    }

}
