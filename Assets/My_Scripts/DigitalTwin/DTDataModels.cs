// =============================================================================
// DTDataModels.cs — Modelli dati Digital Twin VR Cagliari
// My_Scripts/DigitalTwin/DTDataModels.cs
// =============================================================================

using System;

namespace DigitalTwin
{
    // ─── MESSAGGI IN USCITA (Unity → Server) ─────────────────────────────────

    [Serializable]
    public class DTRegistrazioneMsg
    {
        public string tipo;
        public DTRegistrazioneMsg(string tipo) { this.tipo = tipo; }
    }

    [Serializable]
    public class DTEventoVRGuestMsg
    {
        public bool   ingresso_vr;
        public bool   uscita_vr;
        public string datetimestamp_vr;
        public string id_visore;
    }

    [Serializable]
    public class DTSoglieMsg
    {
        public int soglia_ingressi_intermedia;
        public int soglia_ingressi_limite;
    }

    /// <summary>
    /// Richiesta PULL storico — inviata dalla VR DSS on-demand.
    /// Il server risponde con DTStoricoResponseMsg.
    /// </summary>
    [Serializable]
    public class DTStoricoRequestMsg
    {
        public string data_start;   // formato YYYY-MM-DD
        public string data_end;     // formato YYYY-MM-DD
    }

    // ─── MESSAGGI IN INGRESSO (Server → Unity) ────────────────────────────────

    /// <summary>
    /// Payload PUSH automatico ogni 10s dal server verso VR DSS.
    /// Contiene solo le metriche real-time calcolate.
    /// </summary>
    [Serializable]
    public class DTMetricheDSSMsg
    {
        public string tipo;          // "metriche_dss"
        public string stato_varco;   // "ok" | "Errore di comunicazione EchoBean"
        public int    visitatori;    // persone fisicamente dentro ora
        public int    visitatori_vr; // sessioni visore VR attive
    }

    /// <summary>
    /// Payload PULL storico — risposta del server a DTStoricoRequestMsg.
    /// Contiene media pesata giornaliera e errore standard nel periodo richiesto.
    /// </summary>
    [Serializable]
    public class DTStoricoResponseMsg
    {
        public string tipo;              // "storico_dss"
        public string data_start;
        public string data_end;
        public float  media;             // media aritmetica delle medie giornaliere pesate
        public float  errore_standard;   // σ / √n
        public int    campioni;          // numero di giorni nel periodo
        public string errore;            // valorizzato solo in caso di errore del server
    }

    /// <summary>
    /// Envelope per identificare il tipo messaggio prima della deserializzazione.
    /// </summary>
    [Serializable]
    public class DTMsgEnvelope
    {
        public string tipo;
        public string stato_varco;
    }

    // ─── Costanti ─────────────────────────────────────────────────────────────
    public static class DTStati
    {
        public const string VARCO_OK     = "ok";
        public const string VARCO_ERROR  = "Errore di comunicazione EchoBean";
        public const string SEM_VERDE    = "verde";
        public const string SEM_GIALLO   = "giallo";
        public const string SEM_ROSSO    = "rosso";
        public const string TIPO_GUEST   = "vr_guest";
        public const string TIPO_DSS     = "vr_dss";
        public const string TIPO_STORICO = "storico_dss";
    }
}
