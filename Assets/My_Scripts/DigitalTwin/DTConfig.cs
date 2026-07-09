// =============================================================================
// DTConfig.cs — ScriptableObject configurazione Digital Twin
// My_Scripts/DigitalTwin/DTConfig.cs
//
// Crea in Unity: Assets → Create → DigitalTwin → DTConfig
// Assegna l'asset creato al campo "Dt Config" di DTWebSocketClient
// nel GameObject della scena.
//
// Segue il pattern AppConfig/JsonConfig già presente nel progetto.
// =============================================================================

using UnityEngine;

namespace DigitalTwin
{
    [CreateAssetMenu(fileName = "DTConfig", menuName = "DigitalTwin/DTConfig")]
    public class DTConfig : ScriptableObject
    {
        [Header("WebSocket Server (Python)")]
        [Tooltip("Indirizzo IP del Server Python (localhost per sviluppo)")]
        public string serverHost = "localhost";

        [Tooltip("Porta WebSocket del Server Python")]
        public int serverPort = 8765;

        [Header("Identità Visore")]
        [Tooltip("MAC address del visore Meta Quest 3 — usato come id_visore")]
        public string idVisore = "AA:BB:CC:DD:EE:FF";

        [Header("Comportamento Connessione")]
        [Tooltip("Secondi tra un tentativo di riconnessione e il successivo")]
        public float reconnectDelaySeconds = 3f;

        [Tooltip("Numero massimo di tentativi di riconnessione (0 = infiniti)")]
        public int maxReconnectAttempts = 0;

        // ── Proprietà calcolate ───────────────────────────────────────────────
        public string WebSocketUrl => $"ws://{serverHost}:{serverPort}";
    }
}
