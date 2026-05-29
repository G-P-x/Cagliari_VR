// =============================================================================
// DTWebSocketClient.cs — Core WebSocket Client Digital Twin
// My_Scripts/DigitalTwin/DTWebSocketClient.cs
//
// VERSIONE SENZA DIPENDENZE ESTERNE
// Usa System.Net.WebSockets (built-in .NET) — nessun pacchetto aggiuntivo.
// Compatibile con Unity 2021+ su PC e Android/Quest 3.
//
// NON serve NativeWebSocket di endel. Se lo hai installato:
//   1. Apri Packages/manifest.json
//   2. Rimuovi la riga: "com.endel.nativewebsocket": "..."
//   3. Salva e lascia Unity reimportare
// =============================================================================

using System;
using System.Collections;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using DigitalTwin;

public class DTWebSocketClient : MonoBehaviour
{
    // ─── Singleton ────────────────────────────────────────────────────────────
    public static DTWebSocketClient Instance { get; private set; }

    // ─── Inspector ────────────────────────────────────────────────────────────
    [Header("Configurazione Digital Twin")]
    [SerializeField] private DTConfig dtConfig;

    [Header("Tipo Client")]
    [Tooltip("VR Guest = Meta Quest 3 | VR DSS = PC Dashboard")]
    [SerializeField] private ClientType clientType = ClientType.VRGuest;

    public enum ClientType { VRGuest, VRDSS }

    // ─── Stato connessione ────────────────────────────────────────────────────
    public bool IsConnected => _ws != null && _ws.State == WebSocketState.Open;
    public WebSocketState ConnectionState => _ws?.State ?? WebSocketState.Closed;

    // ─── Eventi pubblici ──────────────────────────────────────────────────────
    public event Action<string> OnStatoVarcoReceived;
    public event Action<DTMetricheDSSMsg> OnMetricheDSSReceived;
    public event Action OnConnected;
    public event Action OnDisconnected;

    // ─── Stato interno ────────────────────────────────────────────────────────
    private ClientWebSocket _ws;
    private CancellationTokenSource _cts;
    private int _reconnectAttempts = 0;
    private bool _shouldReconnect = true;

    // Coda thread-safe: bridge thread WebSocket → main thread Unity (Update)
    private readonly System.Collections.Concurrent.ConcurrentQueue<string> _incomingQueue
        = new System.Collections.Concurrent.ConcurrentQueue<string>();

    // ─── Unity Lifecycle ──────────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (dtConfig == null)
            Debug.LogError("[DT] DTConfig non assegnato nel Inspector!");
    }

    private void Start()
    {
        _ = ConnectAsync();
    }

    private void Update()
    {
        // Svuota la coda sul main thread — unico modo sicuro per toccare Unity
        while (_incomingQueue.TryDequeue(out string json))
            HandleIncomingMessage(json);
    }

    private void OnDestroy()
    {
        _shouldReconnect = false;
        _cts?.Cancel();
        _ws?.Dispose();
    }

    private void OnApplicationQuit()
    {
        _shouldReconnect = false;
        _cts?.Cancel();
    }

    // ─── Connessione ─────────────────────────────────────────────────────────
    private async Task ConnectAsync()
    {
        if (dtConfig == null) return;

        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();

        _ws?.Dispose();
        _ws = new ClientWebSocket();

        string url = dtConfig.WebSocketUrl;
        Debug.Log($"[DT] Connessione a {url} come [{clientType}]...");

        try
        {
            await _ws.ConnectAsync(new Uri(url), _cts.Token);

            _reconnectAttempts = 0;
            Debug.Log($"[DT] Connesso a {url}");

            // Registra tipo client — PRIMO messaggio obbligatorio
            string tipoStr = clientType == ClientType.VRGuest
                ? DTStati.TIPO_GUEST
                : DTStati.TIPO_DSS;
            await SendJsonRawAsync(JsonUtility.ToJson(new DTRegistrazioneMsg(tipoStr)));
            Debug.Log($"[DT] Registrazione inviata: tipo={tipoStr}");

            _incomingQueue.Enqueue("__CONNECTED__");

            // Avvia loop di ricezione su questo task (await = non blocca Unity)
            await ReceiveLoopAsync();
        }
        catch (OperationCanceledException)
        {
            Debug.Log("[DT] Connessione annullata.");
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[DT] Connessione fallita: {e.Message}");
            if (_shouldReconnect)
                StartCoroutine(ReconnectRoutine());
        }
    }

    // ─── Loop di ricezione (gira su task separato) ────────────────────────────
    private async Task ReceiveLoopAsync()
    {
        var buffer = new byte[4096];

        try
        {
            while (_ws.State == WebSocketState.Open && !_cts.Token.IsCancellationRequested)
            {
                var sb = new StringBuilder();
                WebSocketReceiveResult result;

                // Ciclo per messaggi frammentati
                do
                {
                    result = await _ws.ReceiveAsync(new ArraySegment<byte>(buffer), _cts.Token);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        Debug.Log("[DT] Server ha chiuso la connessione.");
                        _incomingQueue.Enqueue("__DISCONNECTED__");
                        return;
                    }

                    sb.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));
                }
                while (!result.EndOfMessage);

                _incomingQueue.Enqueue(sb.ToString());
            }
        }
        catch (OperationCanceledException) { /* shutdown normale */ }
        catch (Exception e)
        {
            Debug.LogWarning($"[DT] Ricezione interrotta: {e.Message}");
        }

        _incomingQueue.Enqueue("__DISCONNECTED__");
    }

    // ─── Dispatch messaggi sul main thread (chiamato da Update) ──────────────
    private void HandleIncomingMessage(string json)
    {
        if (json == "__CONNECTED__")    { OnConnected?.Invoke();    return; }
        if (json == "__DISCONNECTED__")
        {
            OnDisconnected?.Invoke();
            if (_shouldReconnect) StartCoroutine(ReconnectRoutine());
            return;
        }

        try
        {
            var envelope = JsonUtility.FromJson<DTMsgEnvelope>(json);
            if (envelope == null) { Debug.LogWarning($"[DT] Msg non parsabile: {json}"); return; }

            if (envelope.tipo == "metriche_dss")
            {
                var m = JsonUtility.FromJson<DTMetricheDSSMsg>(json);
                Debug.Log($"[DT] Metriche | visitatori={m.visitatori} | media={m.media_ingressi:F1}");
                OnMetricheDSSReceived?.Invoke(m);
                return;
            }

            if (!string.IsNullOrEmpty(envelope.stato_varco))
            {
                Debug.Log($"[DT] stato_varco={envelope.stato_varco}");
                OnStatoVarcoReceived?.Invoke(envelope.stato_varco);
                return;
            }

            if (json.Contains("\"errore\"")) { Debug.LogWarning($"[DT] Errore server: {json}"); return; }

            Debug.LogWarning($"[DT] Messaggio non riconosciuto: {json}");
        }
        catch (Exception e) { Debug.LogError($"[DT] Parsing: {e.Message} | JSON: {json}"); }
    }

    // ─── Invio messaggi ───────────────────────────────────────────────────────
    public async void SendJson<T>(T data)
    {
        if (!IsConnected) { Debug.LogWarning("[DT] SendJson: non connesso."); return; }
        try
        {
            string json = JsonUtility.ToJson(data);
            await SendJsonRawAsync(json);
            Debug.Log($"[DT] → {json}");
        }
        catch (Exception e) { Debug.LogError($"[DT] Invio fallito: {e.Message}"); }
    }

    private async Task SendJsonRawAsync(string json)
    {
        if (_ws?.State != WebSocketState.Open) return;
        byte[] bytes = Encoding.UTF8.GetBytes(json);
        await _ws.SendAsync(
            new ArraySegment<byte>(bytes),
            WebSocketMessageType.Text,
            endOfMessage: true,
            cancellationToken: _cts.Token
        );
    }

    // ─── Riconnessione automatica ─────────────────────────────────────────────
    private IEnumerator ReconnectRoutine()
    {
        int max = dtConfig?.maxReconnectAttempts ?? 0;
        bool infinite = max == 0;

        if (!infinite && _reconnectAttempts >= max)
        {
            Debug.LogError($"[DT] Max tentativi ({max}) raggiunto. Connessione abbandonata.");
            yield break;
        }

        _reconnectAttempts++;
        float delay = dtConfig?.reconnectDelaySeconds ?? 3f;
        string info = infinite ? $"#{_reconnectAttempts}" : $"#{_reconnectAttempts}/{max}";
        Debug.Log($"[DT] Riconnessione {info} tra {delay}s...");

        yield return new WaitForSeconds(delay);
        _ = ConnectAsync();
    }
}
