// =============================================================================
// DTGuestController.cs — Controller VR Guest (Meta Quest 3)
// My_Scripts/DigitalTwin/DTGuestController.cs
//
// UTILIZZO IN SCENA (GAP1.unity, GAP2.unity):
//   1. Aggiungi a un GameObject nella scena (es. "VarcoManager")
//   2. Assegna DTConfig nel campo "Dt Config"
//   3. Assegna il riferimento all'oggetto Semaforo 3D (opzionale)
//   4. Questo script invia automaticamente gli eventi VR al server
//      e aggiorna il semaforo 3D in base allo stato ricevuto
//
// RESPONSABILITÀ:
//   • Invia evento ingresso VR quando la scena si carica (OnEnable)
//   • Invia evento uscita VR quando la scena si scarica (OnDisable)
//   • Riceve stato_varco e lo propaga al DTSemaforoVisual
//   • Espone evento C# OnStatoVarcoChanged per altri componenti della scena
// =============================================================================

using System;
using UnityEngine;
using DigitalTwin;

public class DTGuestController : MonoBehaviour
{
    // ─── Inspector ────────────────────────────────────────────────────────────
    [Header("Configurazione Digital Twin")]
    [SerializeField] private DTConfig dtConfig;

    [Header("Semaforo 3D (opzionale)")]
    [Tooltip("Assegna il GameObject del semaforo 3D nella scena")]
    [SerializeField] private DTSemaforoVisual semaforoVisual;

    [Header("Debug")]
    [SerializeField] private bool logEventiVR = true;

    // ─── Stato ────────────────────────────────────────────────────────────────
    private string _statoVarcoCorrente = DTStati.VARCO_OK;
    private bool _sessioneAttiva = false;

    /// <summary>
    /// Stato varco corrente ricevuto dal Digital Twin Server.
    /// "ok" = varco operativo | "Errore di comunicazione EchoBean" = fuori servizio
    /// </summary>
    public string StatoVarco => _statoVarcoCorrente;
    public bool IsVarcoOperativo => _statoVarcoCorrente == DTStati.VARCO_OK;

    // ─── Evento per altri componenti della scena ──────────────────────────────
    /// <summary>
    /// Invocato ogni volta che lo stato varco cambia.
    /// Altri MonoBehaviour possono iscriversi per reagire al cambio di stato.
    /// </summary>
    public event Action<string> OnStatoVarcoChanged;

    // ─── Unity Lifecycle ──────────────────────────────────────────────────────
    private void OnEnable()
    {
        if (DTWebSocketClient.Instance != null)
        {
            DTWebSocketClient.Instance.OnStatoVarcoReceived += HandleStatoVarco;
            DTWebSocketClient.Instance.OnConnected += HandleConnected;

            // Se già connesso invia subito, altrimenti aspetta HandleConnected
            if (DTWebSocketClient.Instance.IsConnected)
                InviaEventoVR(ingresso: true);
            // else: HandleConnected() lo farà appena la connessione è pronta
        }
        else
        {
            // DTWebSocketClient non ancora in scena — riprova al frame successivo
            StartCoroutine(AttesaClient());
        }
    }

    private System.Collections.IEnumerator AttesaClient()
    {
        // Aspetta finché il Singleton è disponibile (max 10 secondi)
        float timeout = 10f;
        while (DTWebSocketClient.Instance == null && timeout > 0f)
        {
            timeout -= Time.deltaTime;
            yield return null;
        }

        if (DTWebSocketClient.Instance == null)
        {
            Debug.LogError("[DT Guest] DTWebSocketClient non trovato in scena dopo 10s. " +
                           "Assicurati che sia presente nella Home scene.");
            yield break;
        }

        DTWebSocketClient.Instance.OnStatoVarcoReceived += HandleStatoVarco;
        DTWebSocketClient.Instance.OnConnected += HandleConnected;

        if (DTWebSocketClient.Instance.IsConnected)
            InviaEventoVR(ingresso: true);
    }

    private void OnDisable()
    {
        // Invia evento uscita VR prima di disiscriversi
        if (_sessioneAttiva)
        {
            InviaEventoVR(ingresso: false);
        }

        if (DTWebSocketClient.Instance != null)
        {
            DTWebSocketClient.Instance.OnStatoVarcoReceived -= HandleStatoVarco;
            DTWebSocketClient.Instance.OnConnected -= HandleConnected;
        }
    }

    // ─── Handlers ─────────────────────────────────────────────────────────────

    private void HandleConnected()
    {
        // Alla riconnessione, reinvia l'evento ingresso VR
        if (gameObject.activeInHierarchy)
        {
            InviaEventoVR(ingresso: true);
        }
    }

    private void HandleStatoVarco(string statoVarco)
    {
        // Esegue sul thread principale Unity (garantito da DispatchMessageQueue)
        if (_statoVarcoCorrente == statoVarco) return;  // Nessun cambiamento

        _statoVarcoCorrente = statoVarco;

        if (logEventiVR)
            Debug.Log($"[DT Guest] stato_varco aggiornato: {statoVarco}");

        // Aggiorna semaforo visuale 3D
        semaforoVisual?.AggiornaDaStatoVarco(statoVarco);

        // Notifica altri componenti della scena
        OnStatoVarcoChanged?.Invoke(statoVarco);
    }

    // ─── Invio eventi VR ──────────────────────────────────────────────────────

    /// <summary>
    /// Invia manualmente un evento di ingresso o uscita VR al server.
    /// Chiamato automaticamente da OnEnable/OnDisable, ma può essere invocato
    /// anche da altri script (es. TriggerZone, OVRManager events).
    /// </summary>
    public void InviaEventoVR(bool ingresso)
    {
        if (dtConfig == null)
        {
            Debug.LogWarning("[DT Guest] DTConfig non assegnato.");
            return;
        }

        if (DTWebSocketClient.Instance == null || !DTWebSocketClient.Instance.IsConnected)
        {
            if (logEventiVR)
                Debug.LogWarning("[DT Guest] InviaEventoVR: non connesso al server.");
            return;
        }

        _sessioneAttiva = ingresso;

        var msg = new DTEventoVRGuestMsg
        {
            ingresso_vr         = ingresso,
            uscita_vr           = !ingresso,
            datetimestamp_vr    = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            id_visore           = dtConfig.idVisore
        };

        DTWebSocketClient.Instance.SendJson(msg);

        if (logEventiVR)
            Debug.Log($"[DT Guest] Evento VR inviato: {(ingresso ? "INGRESSO" : "USCITA")} | visore={dtConfig.idVisore}");
    }

    // ─── API pubblica per altri script ────────────────────────────────────────

    /// <summary>
    /// Chiamata da OVRManager o altri trigger quando il visore viene indossato.
    /// Equivalente a InviaEventoVR(ingresso: true).
    /// </summary>
    public void OnVisoreIndossato() => InviaEventoVR(ingresso: true);

    /// <summary>
    /// Chiamata da OVRManager o altri trigger quando il visore viene rimosso.
    /// Equivalente a InviaEventoVR(ingresso: false).
    /// </summary>
    public void OnVisoreRimosso() => InviaEventoVR(ingresso: false);
}
