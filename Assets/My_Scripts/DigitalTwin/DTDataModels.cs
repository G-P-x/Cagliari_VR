// =============================================================================
// DTDataModels.cs — Modelli dati Digital Twin VR Cagliari
// My_Scripts/DigitalTwin/DTDataModels.cs
//
// Classi C# che mappano esattamente i payload JSON del server Python.
// Usate da DTWebSocketClient per deserializzare i messaggi in arrivo
// e serializzare i messaggi in uscita.
//
// JsonUtility richiede che le classi siano [Serializable] e i campi
// siano public o [SerializeField]. I nomi dei field DEVONO corrispondere
// esattamente alle chiavi JSON del server (snake_case).
// =============================================================================

using System;
using System.Collections.Generic;

namespace DigitalTwin
{
    // ─── MESSAGGI IN USCITA (Unity → Server) ─────────────────────────────────

    /// <summary>
    /// Messaggio di registrazione tipo client.
    /// Deve essere il PRIMO messaggio inviato dopo OnOpen.
    /// </summary>
    [Serializable]
    public class DTRegistrazioneMsg
    {
        public string tipo;  // "vr_guest" oppure "vr_dss"

        public DTRegistrazioneMsg(string tipo) { this.tipo = tipo; }
    }

    /// <summary>
    /// Evento sessione VR inviato dal VR Guest (Meta Quest 3) al Server.
    /// Inviare per ogni ingresso/uscita dalla scena VR.
    /// </summary>
    [Serializable]
    public class DTEventoVRGuestMsg
    {
        public bool ingresso_vr;
        public bool uscita_vr;
        public string datetimestamp_vr;  // ISO 8601: "2024-06-01T10:30:00Z"
        public string id_visore;         // MAC address del visore
    }

    /// <summary>
    /// Aggiornamento soglie semaforo inviato dalla VR DSS al Server.
    /// </summary>
    [Serializable]
    public class DTSoglieMsg
    {
        public int soglia_ingressi_intermedia;
        public int soglia_ingressi_limite;
    }

    /// <summary>
    /// Richiesta on-demand metriche dalla VR DSS.
    /// </summary>
    [Serializable]
    public class DTRichiestaMetricheMsg
    {
        public string richiesta = "metriche";
    }

    // ─── MESSAGGI IN INGRESSO (Server → Unity) ────────────────────────────────

    /// <summary>
    /// Stato varco ricevuto dal Server verso VR Guest.
    /// Valori: "ok" | "Errore di comunicazione EchoBean"
    /// </summary>
    [Serializable]
    public class DTStatoVarcoMsg
    {
        public string stato_varco;

        public bool IsOperativo => stato_varco == "ok";
    }

    /// <summary>
    /// Pacchetto metriche completo ricevuto dal Server verso VR DSS.
    /// Inviato ogni 10 secondi (push automatico) e su richiesta.
    /// </summary>
    [Serializable]
    public class DTMetricheDSSMsg
    {
        public string tipo;                          // "metriche_dss"
        public string stato_varco;
        public int visitatori;
        public int visitatori_vr;
        public int soglia_intermedia;
        public int soglia_limite;
        public float media_ingressi;
        public float deviazione_standard_ingressi;

        // Storici come JSON array — deserializzati manualmente
        // (JsonUtility non supporta List<string> annidati in modo diretto)
        // Usare DTMetricheDSSRaw per il parsing grezzo, poi convertire
    }

    /// <summary>
    /// Envelope grezzo per identificare il tipo di messaggio in arrivo
    /// prima della deserializzazione completa.
    /// </summary>
    [Serializable]
    public class DTMsgEnvelope
    {
        public string tipo;
        public string stato_varco;
    }

    // ─── COSTANTI STATO ───────────────────────────────────────────────────────

    public static class DTStati
    {
        public const string VARCO_OK    = "ok";
        public const string VARCO_ERROR = "Errore di comunicazione EchoBean";

        public const string SEM_VERDE   = "verde";
        public const string SEM_GIALLO  = "giallo";
        public const string SEM_ROSSO   = "rosso";

        public const string TIPO_GUEST  = "vr_guest";
        public const string TIPO_DSS    = "vr_dss";
    }
}
