using UnityEngine;
using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Linq;
using Utils;
using System.Collections.Generic;

internal class AppConfig
{
    private readonly JsonConfig jsonConfig;
    private readonly static string androidDirectory = Path.GetDirectoryName(Application.persistentDataPath);

    public AppConfig() { jsonConfig ??= new(); }  // Istanzio un oggetto json solamente una volta, all'avvio dell'applicazione.
                                                  // Le successive eventuali istanze di AppConfig, non determineranno l'istanziamento di un nuovo oggetto json

    internal async Task InitializeEnviroment()
    {
        await jsonConfig.InitializeJson();

        // Utilizzando Path.Combine, dovrebbe essere garantita la corretta formattazione della stringa path per tutte le piattaforme (Win, MacOS, Android...)
        string databaseDirectory = Path.Combine(androidDirectory, ".database");
        
        string[] directories = { databaseDirectory };
        string[] filesToEnsure = {  };  // File che NON devono essere sovrascritti ad ogni avvio dell'applicazione
        string[] filesToCreate = { ReadFromJson.GetJsonParameter("LogFile") };  // File che devono essere sovrascritti (ricreati) ad ogni avvio dell'applicazione

        if (await CreateDirectories(directories) == -1) return; // Se uno o più file non venissero creati correttamente, non sarebbe possibile utilizzare l'applicazione

        if (await CreateFiles(fToEnsure: filesToEnsure, fToCreate: filesToCreate) == -1) return;
    }


    public async Task SetServerUrlAsync(string url) { await jsonConfig.SetUrlAsync(url); }


    private async Task<int> CreateDirectories(string[] directories)
    {
        if (directories.All(Directory.Exists)) return 0;

        //CREAZIONE DELLE DIRECTORY
        await Task.Run(() =>
        {
            foreach (string directory in directories)
            {
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);

                    if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                    {
                        DirectoryInfo di = new(directory);
                        di.Attributes |= FileAttributes.Hidden;
                    }
                }
            }
        });

        //VERIFICA DELLA CREAZIONE DELLE DIRECTORIES
        if (!await FileUtility.WaitForExistenceAsync(directories, Directory.Exists))
        {
            Debug.LogError("Unable to create necessary directories. Please check permissions and try to restart.");
            return -1;
        }

        return 0;
    }


    /// <summary>
    /// Funzione per creare i file.
    /// La funzione è studiata sia per creare i file che devono essere sempre ricreati ad ogni avvio dell'applicazione, che per preservare i file che non devono essere sovrascritti
    /// nei successivi avvii dell'applicazione.
    /// Ad esempio, il file destinato a contenere il database viene creato solamente al primo avvio dell'applicazione, in maniera tale da poter essere riutilizzato anche durante
    /// le sessioni successive, nel caso in cui fosse assente la connessione di rete, e non risulterebbe possibile aggiornarlo.
    /// Il file destinato a contere gli errori invece, dev'essere sovrascritto, e quindi ricreato, ad ogni avvio dell'applicazione.
    /// </summary>
    /// <param name="fToCreate"></param>
    /// <param name="fToEnsure"></param>
    /// <returns></returns>
    private async Task<int> CreateFiles(string[] fToEnsure, string[] fToCreate)
    {
        //CREAZIONE DEI FILE
        await Task.Run(() =>
        {
            //CREAZIONE DEI FILE CHE DEVONO ESSERE SEMPRE RICREATI
            foreach (string file in fToCreate) using (File.Create(file)) { };  // Utilizzo "using" per garantire la chiusura del flusso dopo la creazione del file.
                                                                               // Questo garantisce che il contenuto del file venga scritto correttamente durante la fase di download,
                                                                               // nel caso in cui il file venisse creato durante la stessa esecuzione dell'applicazione

            //CREAZIONE DEI FILE SOLAMENTE AL PRIMO AVVIO DELL'APPLICAZONE
            foreach (string file in fToEnsure) if (!File.Exists(file)) using (File.Create(file)) { };

        });

        //VERIFICA DELLA CREAZIONE DEI FILE
        if (!await FileUtility.WaitForExistenceAsync(fToCreate.Concat(fToEnsure).ToArray(), File.Exists))
        {
            Debug.LogError("Unable to create necessary files. Please check permissions and try to restart.");
            return -1;
        }

        return 0;
    }
}

public class JsonConfig
{
    private JsonData jsonData;
    private static string jsonFilePath;

    private readonly static string androidDirectory = Path.GetDirectoryName(Application.persistentDataPath);
    private readonly static string databasePath = Path.Combine(androidDirectory, ".database", ".local_database.bin");
    private readonly static string logFilePath = Path.Combine(androidDirectory, "log.txt");
    // private readonly static string defaultUrl = "http://192.167.133.35:8006/api/unity_resources";
    private readonly static string defaultUrl = "http://192.168.1.217:8000/api/unity_resources/debug";
    [Serializable]
    private class JsonData
    {
        public Dictionary<string, string> Parameters = new()
        {
            { "Database", databasePath },
            { "LogFile", logFilePath },
            { "ServerUrl", defaultUrl },
            { "DownloadAtStartup", "true"},
        };
        // Put here other dictionaries to add to the json file
        // ex:
        //public Dictionary<string, string> Authentication = new()
        //{
        //    { "Username", "..." },
        //    { "Password", "..." },
        //};
    }

    public JsonConfig()
    {
        jsonData = new JsonData();
        jsonFilePath = Path.Combine(androidDirectory, "jsonConfig.json");
    }

    internal async Task InitializeJson()
    {
        try
        {
            if (!File.Exists(jsonFilePath)) 
            {
                await WriteJsonToFile(); // Se non esiste, creo il file JSON con i parametri di default
            }
            else
            {
                await UpdateJsonFile(); // Altrimenti, aggiorno il json con le modifiche apportate
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error initializing json file: {ex.Message}");
        }
    }

    private async Task LoadJsonData()
    {
        if (File.Exists(jsonFilePath))
        {
            string jsonContent = await File.ReadAllTextAsync(jsonFilePath);
            jsonData = JsonConvert.DeserializeObject<JsonData>(jsonContent);
        }
        else
        {
            Debug.LogWarning("JSON file does not exist. Using default values.");
        }
    }


    private async Task UpdateJsonFile()
    {
        await LoadJsonData();

        // PARAMETRI IMMUTABILI -> Se questi parametri vengono cambiati nel file json, verranno reimpostati ai valori di default
        jsonData.Parameters["Database"] = databasePath;
        jsonData.Parameters["LogFile"] = logFilePath;

        // PARAMETRI MUTABILI -> Questi parametri possono essere cambiati direttamente dal file json. In caso di valore nullo, viene reimpostato il valore di default
        if (!jsonData.Parameters.ContainsKey("ServerUrl") || string.IsNullOrEmpty(jsonData.Parameters["ServerUrl"]))
        {
            jsonData.Parameters["ServerUrl"] = defaultUrl;
            jsonData.Parameters["DownloadAtStartup"] = "false";
        }

        await WriteJsonToFile();
    }


    private async Task WriteJsonToFile()
    {
        try
        {
            string updatedJsonData = JsonConvert.SerializeObject(jsonData, Formatting.Indented);

            using StreamWriter writer = File.CreateText(jsonFilePath);
            await writer.WriteAsync(updatedJsonData);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error writing json file: {ex.Message}");
        }
    }


    internal async Task SetUrlAsync(string url)
    {
        if (jsonData.Parameters.ContainsKey("ServerUrl"))
        {
            jsonData.Parameters["ServerUrl"] = url;
            await WriteJsonToFile();
        }
        else
        {
            Debug.LogError("ServerUrl parameter not found in JSON data.");
        }
    }


    public static string GetJsonFilePath { get { return jsonFilePath; } }
}
