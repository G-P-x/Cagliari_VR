using UnityEngine;
using DataDownloader;
using System.Net.Http;
using System.Threading.Tasks;
using Utils;
using System.IO;
using UserInput;

[CreateAssetMenu(fileName = "AppInitializer", menuName = "ScriptableObjects/AppInitializer")]
public class AppInitializer : ScriptableObject
{
    private static readonly LogFormat logFormat = new();
    private static bool isInitialized = false;
    private static bool downloadAtStartup;
    private static string databasePath;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private async static void _()  // Non è necessario assegnare un nome al metodo di avvio, in quanto non dev'essere chiamato da nessuna parte
    {
        // Il codice qui verrà eseguito all'avvio dell'applicazione
        if ( !await Initializer() ) return;
        // Debug.Log("Application ready!");
        // DataUsageExamples dataUsageExamples = new();
        // dataUsageExamples.TryData();
    }


    public async static Task<bool> Initializer()
    {
        /// Configurazione dell'applicazione
        AppConfig appConfig = new();
        await appConfig.InitializeEnviroment();
        downloadAtStartup = bool.Parse(ReadFromJson.GetJsonParameter("DownloadAtStartup"));
        databasePath = ReadFromJson.GetJsonParameter("Database");

        /// Download del database
        if ( !downloadAtStartup ) Debug.LogWarning("Download at startup is disabled. Set 'DownloadAtStartup' to 'true' in the json configuration file to enable it.");
        if (downloadAtStartup || !File.Exists(databasePath))
        {
            await DownloadDatabase();
        }
    
        /// Inizializzazione del database
        if ( !await InitializeDatabase() ) return false;

        return true;
    }

    private static async Task<bool> DownloadDatabase()
    {
        HttpResponseMessage serverResponse = await DatabaseDownloader.DownloadData();
        if ( serverResponse != null && serverResponse.IsSuccessStatusCode) 
        {
            Debug.Log($"{logFormat.databaseLog} Il download del database è stato completato con successo.");
            return true;
        }
        else
        {
            // Si è verificato un errore durante il download del database
            Debug.LogWarning("Impossibile aggiornare il database. Si è verificato un errore durante la richiesta al server." +
                " Assicurarsi che il server sia raggiungibile all'indirizzo impostato nella configurazione.");

            // Informazioni sull'errore del server
            if (serverResponse != null)
            {
                string errorMessage = await serverResponse.Content.ReadAsStringAsync();
                Debug.LogWarning("Dettagli dell'errore: " + errorMessage);
            }
            else Debug.LogWarning("Dettagli dell'errore: Nessuna risposta dal server.");
            if ( !File.Exists(databasePath) ) 
            {   
                Debug.LogError("Impossibile avviare l'applicazione: il database non esiste.");
            }
            return false;
        }
    }


    private static async Task<bool> InitializeDatabase()
    {
        int databaseResponse = await DataRequest.InitializeDatabase();
        if (databaseResponse == 1) 
        {
            if (!isInitialized)
            {                
                isInitialized = true;
            }
            Debug.Log($"{logFormat.databaseLog} Database inizializzato correttamente.\nUltimo aggiornamento: {ReadLocalDatabase.GetLastUpdate:dd/MM/yyyy HH:mm:ss}");
            Debug.Log($"{logFormat.databaseReadyLog} Goditi Cagliari!");
            return true;
        }
        else 
        {
            Debug.LogError("Cant' initialize database. Try to restart the application.");
            isInitialized = false;
            return false;
        }
    }


    public async void UpdateDatabase()
    {
        if ( await DownloadDatabase() ) await InitializeDatabase();
    }

}
