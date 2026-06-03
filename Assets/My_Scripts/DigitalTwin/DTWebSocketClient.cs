// =============================================================================
// DTWebSocketClient.cs — Core WebSocket Client Digital Twin
// My_Scripts/DigitalTwin/DTWebSocketClient.cs
//
// Usa System.Net.WebSockets (built-in .NET) — nessun pacchetto aggiuntivo.
//
// Il loop di ricezione gira su un Thread dedicato (non un Task async) per
// evitare il classico problema Unity di GC sui Task fire-and-forget che
// causava l'interruzione della ricezione dopo poche iterazioni.
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
    public event Action<DTStoricoResponseMsg> OnStoricoDSSReceived;
    public event Action OnConnected;
    public event Action OnDisconnected;

    // ─── Stato interno ────────────────────────────────────────────────────────
    private ClientWebSocket _ws;
    private CancellationTokenSource _cts;
    private Thread _receiveThread;          // Thread dedicato — non Task
    private bool _shouldReconnect = true;
    private int  _reconnectAttempts = 0;

    // Coda thread-safe: bridge Thread ricezione → main thread Unity (Update)
    private readonly System.Collections.Concurrent.ConcurrentQueue<string>
        _incomingQueue = new System.Collections.Concurrent.ConcurrentQueue<string>();

    // ─── Singleton flag — impedisce a Start() di girare su istanze duplicate ─
    private bool _isValidInstance = false;

    // ─── Watchdog — rileva assenza di dati e forza riconnessione ──────────────
    private float _lastReceivedTime = 0f;
    private const float WATCHDOG_TIMEOUT = 35f;  // secondi senza dati → riconnetti

    // ─── Unity Lifecycle ──────────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;  // _isValidInstance rimane false → Start() non girerà
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        _isValidInstance = true;

        if (dtConfig == null)
            Debug.LogError("[DT] DTConfig non assegnato nel Inspector!");
    }

    private void Start()
    {
        if (!_isValidInstance) return;
        _lastReceivedTime = Time.time;
        _ = ConnectAsync();
    }

    private void Update()
    {
        if (!_isValidInstance) return;

        // Svuota la coda sul main thread — unico thread Unity-safe
        while (_incomingQueue.TryDequeue(out string json))
        {
            _lastReceivedTime = Time.time;
            HandleIncomingMessage(json);
        }

        // Watchdog: se connesso ma nessun dato da WATCHDOG_TIMEOUT secondi → riconnetti
        if (IsConnected && _shouldReconnect
            && (Time.time - _lastReceivedTime) > WATCHDOG_TIMEOUT)
        {
            Debug.LogWarning($"[DT] Watchdog: nessun dato da {WATCHDOG_TIMEOUT}s — riconnessione forzata.");
            _lastReceivedTime = Time.time;  // reset per non triggerare subito di nuovo
            _cts?.Cancel();
            StartCoroutine(ReconnectRoutine());
        }
    }

    private void OnDestroy()
    {
        _shouldReconnect = false;
        ShutdownConnection();
    }

    private void OnApplicationQuit()
    {
        _shouldReconnect = false;
        ShutdownConnection();
    }

    private void ShutdownConnection()
    {
        _cts?.Cancel();
        try { _ws?.CloseAsync(WebSocketCloseStatus.NormalClosure, "closing", CancellationToken.None).Wait(500); }
        catch { }
        _ws?.Dispose();
        _receiveThread = null;
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

            // Registrazione tipo client — primo messaggio obbligatorio
            string tipoStr = clientType == ClientType.VRGuest
                ? DTStati.TIPO_GUEST
                : DTStati.TIPO_DSS;
            await SendJsonRawAsync(JsonUtility.ToJson(new DTRegistrazioneMsg(tipoStr)));
            Debug.Log($"[DT] Registrazione inviata: tipo={tipoStr}");

            _incomingQueue.Enqueue("__CONNECTED__");

            // Avvia Thread dedicato per la ricezione (sopravvive al GC)
            _receiveThread = new Thread(ReceiveLoop)
            {
                IsBackground = true,   // muore automaticamente alla chiusura app
                Name = "DT-WebSocket-Receive"
            };
            _receiveThread.Start();
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

    // ─── Thread di ricezione dedicato ────────────────────────────────────────
    private void ReceiveLoop()
    {
        var buffer = new byte[4096];
        Debug.Log("[DT] ReceiveThread avviato.");

        try
        {
            while (_ws.State == WebSocketState.Open
                   && !_cts.Token.IsCancellationRequested)
            {
                var sb = new StringBuilder();
                WebSocketReceiveResult result;

                // Ciclo per messaggi frammentati
                do
                {
                    // Wait() blocca il Thread in modo sincrono — nessun async
                    var task = _ws.ReceiveAsync(new ArraySegment<byte>(buffer), _cts.Token);
                    task.Wait(_cts.Token);
                    result = task.Result;

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
        catch (OperationCanceledException)
        {
            // Shutdown normale — non loggare
        }
        catch (AggregateException ae) when (ae.InnerException is OperationCanceledException)
        {
            // Wait() wrappa OperationCanceledException in AggregateException
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[DT] ReceiveThread interrotto: {e.GetType().Name} — {e.Message}");
        }

        Debug.Log("[DT] ReceiveThread terminato.");
        _incomingQueue.Enqueue("__DISCONNECTED__");
    }

    // ─── Dispatch messaggi (main thread — chiamato da Update) ─────────────────
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

            // Metriche DSS — PUSH
            if (envelope.tipo == "metriche_dss")
            {
                var m = JsonUtility.FromJson<DTMetricheDSSMsg>(json);
                Debug.Log($"[DT DSS] visitatori={m.visitatori} visitatori_vr={m.visitatori_vr} stato={m.stato_varco}");
                OnMetricheDSSReceived?.Invoke(m);
                return;
            }

            // Storico — PULL response
            if (envelope.tipo == "storico_dss")
            {
                var s = JsonUtility.FromJson<DTStoricoResponseMsg>(json);
                Debug.Log($"[DT] Storico | media={s.media} | errore_standard={s.errore_standard} | campioni={s.campioni}");
                OnStoricoDSSReceived?.Invoke(s);
                return;
            }

            // Stato varco — VR Guest
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
            Debug.LogError($"[DT] Max tentativi ({max}) raggiunto.");
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
