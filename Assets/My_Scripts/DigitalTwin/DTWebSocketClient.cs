using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using DigitalTwin;

public class DTWebSocketClient : MonoBehaviour
{
    public static DTWebSocketClient Instance { get; private set; }
    private bool _isValidInstance = false;

    [Header("Configurazione Digital Twin")]
    [SerializeField] private DTConfig dtConfig;

    [Header("Tipo Client")]
    [SerializeField] private ClientType clientType = ClientType.VRGuest;
    public enum ClientType { VRGuest, VRDSS }

    public bool IsConnected => _connected;
    private volatile bool _connected = false;

    public event Action<string>               OnStatoVarcoReceived;
    public event Action<DTMetricheDSSMsg>     OnMetricheDSSReceived;
    public event Action<DTStoricoResponseMsg> OnStoricoDSSReceived;
    public event Action OnConnected;
    public event Action OnDisconnected;

    // Ultime metriche ricevute — accessibili via polling da DTDSSController
    public DTMetricheDSSMsg     LastMetriche   { get; private set; }
    public DTStoricoResponseMsg LastStorico    { get; private set; }
    public string               LastStatoVarco { get; private set; } = "";

    private TcpClient     _tcp;
    private NetworkStream _stream;
    private Thread        _receiveThread;
    private volatile bool _shouldRun       = false;
    private volatile bool _shouldReconnect = true;
    private int           _reconnectAttempts = 0;

    private readonly System.Collections.Concurrent.ConcurrentQueue<string>
        _queue = new System.Collections.Concurrent.ConcurrentQueue<string>();
    private readonly System.Collections.Concurrent.ConcurrentQueue<string>
        _sendQueue = new System.Collections.Concurrent.ConcurrentQueue<string>();

    // ─── Unity Lifecycle ──────────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        _isValidInstance = true;
        if (dtConfig == null) Debug.LogError("[DT] DTConfig non assegnato!");
    }

    private void Start()
    {
        if (!_isValidInstance) return;
        StartCoroutine(ConnectCoroutine());
    }

    private void Update()
    {
        if (!_isValidInstance) return;
        while (_queue.TryDequeue(out string msg))
            HandleMessage(msg);
    }

    private void OnDestroy()      { _shouldReconnect = false; Disconnect(); }
    private void OnApplicationQuit() { _shouldReconnect = false; Disconnect(); }

    // ─── Connessione ─────────────────────────────────────────────────────────
    private IEnumerator ConnectCoroutine()
    {
        if (dtConfig == null) yield break;

        Disconnect();

        bool   success = false;
        string error   = null;

        var t = new Thread(() =>
        {
            try
            {
                _tcp = new TcpClient();
                _tcp.NoDelay = true;
                _tcp.Connect(dtConfig.serverHost, dtConfig.serverPort);
                _stream = _tcp.GetStream();
                // Timeout 1s per ReadByte — permette al loop di girare
                _stream.ReadTimeout = 1000;
                DoHandshake(dtConfig.serverHost, dtConfig.serverPort);
                success = true;
            }
            catch (Exception e) { error = e.Message; }
        }) { IsBackground = true };
        t.Start();

        float timeout = 10f;
        while (t.IsAlive && timeout > 0f) { timeout -= Time.deltaTime; yield return null; }

        if (!success)
        {
            Debug.LogWarning($"[DT] Connessione fallita: {error ?? "timeout"}");
            if (_shouldReconnect) yield return StartCoroutine(ReconnectCoroutine());
            yield break;
        }

        _connected = true;
        _shouldRun = true;
        _reconnectAttempts = 0;
        Debug.Log("[DT] Connesso.");

        string tipo = clientType == ClientType.VRGuest ? DTStati.TIPO_GUEST : DTStati.TIPO_DSS;
        SendRaw(JsonUtility.ToJson(new DTRegistrazioneMsg(tipo)));

        _queue.Enqueue("__CONNECTED__");

        _receiveThread = new Thread(ReceiveLoop) { IsBackground = true, Name = "DT-Recv" };
        _receiveThread.Start();
    }

    private void Disconnect()
    {
        _shouldRun = false;
        _connected = false;
        try { _stream?.Close(); } catch { }
        try { _tcp?.Close();    } catch { }
        _stream = null;
        _tcp    = null;
    }

    // ─── Handshake WebSocket ──────────────────────────────────────────────────
    private void DoHandshake(string host, int port)
    {
        byte[] keyBytes = new byte[16];
        new System.Random().NextBytes(keyBytes);
        string key = Convert.ToBase64String(keyBytes);

        string req =
            $"GET / HTTP/1.1\r\nHost: {host}:{port}\r\n" +
            "Upgrade: websocket\r\nConnection: Upgrade\r\n" +
            $"Sec-WebSocket-Key: {key}\r\nSec-WebSocket-Version: 13\r\n\r\n";

        byte[] reqB = Encoding.ASCII.GetBytes(req);
        // Disabilita timeout per l'handshake iniziale
        _stream.ReadTimeout = 5000;
        _stream.Write(reqB, 0, reqB.Length);

        var sb = new StringBuilder();
        byte[] buf = new byte[1];
        while (true)
        {
            _stream.Read(buf, 0, 1);
            sb.Append((char)buf[0]);
            if (sb.Length >= 4 && sb.ToString(sb.Length - 4, 4) == "\r\n\r\n")
                break;
        }

        if (!sb.ToString().Contains("101"))
            throw new Exception($"Handshake fallito.");

        // Imposta timeout operativo per il loop di ricezione
        _stream.ReadTimeout = 1000;
    }

    // ─── Receive Loop (thread dedicato) ──────────────────────────────────────
    private void ReceiveLoop()
    {
        Debug.Log("[DT] ReceiveLoop avviato.");
        try
        {
            while (_shouldRun)
            {
                // Processa la coda di invio
                while (_sendQueue.TryDequeue(out string msg))
                    SendFrameRaw(msg);

                // Leggi primo byte del frame — blocca fino a 1s (ReadTimeout)
                // Se nessun dato → IOException → continue (non è un errore)
                byte b0;
                try { b0 = ReadByte(); }
                catch (IOException) { continue; } // timeout — normale, riprova
                catch (Exception)   { break; }    // errore reale — esci

                if (!_shouldRun) break;

                // Ora che il primo byte è arrivato, leggi il resto del frame
                // Aumenta temporaneamente il timeout per completare la lettura
                _stream.ReadTimeout = 5000;

                byte b1        = ReadByte();
                int  opcode    = b0 & 0x0F;
                bool masked    = (b1 & 0x80) != 0;
                long payLen    = b1 & 0x7F;

                if (payLen == 126)
                    payLen = (ReadByte() << 8) | ReadByte();
                else if (payLen == 127)
                {
                    payLen = 0;
                    for (int i = 0; i < 8; i++) payLen = (payLen << 8) | ReadByte();
                }

                byte[] maskKey = new byte[4];
                if (masked) for (int i = 0; i < 4; i++) maskKey[i] = ReadByte();

                byte[] payload = new byte[payLen];
                ReadExact(payload, (int)payLen);

                if (masked)
                    for (int i = 0; i < payload.Length; i++)
                        payload[i] ^= maskKey[i % 4];

                // Ripristina timeout breve per il prossimo ciclo
                _stream.ReadTimeout = 1000;

                switch (opcode)
                {
                    case 0x1:
                    case 0x0:
                        string text = Encoding.UTF8.GetString(payload);
                        Debug.Log($"[DT] Frame ricevuto: {text.Substring(0, Math.Min(80, text.Length))}");
                        _queue.Enqueue(text);
                        break;
                    case 0x8:
                        Debug.Log("[DT] Close frame.");
                        _queue.Enqueue("__DISCONNECTED__");
                        return;
                    case 0x9: // ping
                        SendPong(payload);
                        break;
                }
            }
        }
        catch (Exception e)
        {
            if (_shouldRun) Debug.LogWarning($"[DT] ReceiveLoop errore: {e.Message}");
        }

        Debug.Log("[DT] ReceiveLoop terminato.");
        _connected = false;
        if (_shouldRun) _queue.Enqueue("__DISCONNECTED__");
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────
    private byte ReadByte()
    {
        int b = _stream.ReadByte();
        if (b < 0) throw new IOException("Stream chiuso.");
        return (byte)b;
    }

    private void ReadExact(byte[] buf, int count)
    {
        int read = 0;
        while (read < count)
        {
            int n = _stream.Read(buf, read, count - read);
            if (n <= 0) throw new IOException("Stream chiuso.");
            read += n;
        }
    }

    private void SendRaw(string text)
    {
        try { SendFrameRaw(text); }
        catch (Exception e) { Debug.LogWarning($"[DT] SendRaw: {e.Message}"); }
    }

    private readonly object _sendLock = new object();
    private void SendFrameRaw(string text)
    {
        byte[] payload = Encoding.UTF8.GetBytes(text);
        byte[] mask    = new byte[4];
        new System.Random().NextBytes(mask);
        var frame = new List<byte> { 0x81 };
        if (payload.Length < 126) frame.Add((byte)(0x80 | payload.Length));
        else { frame.Add(0xFE); frame.Add((byte)(payload.Length >> 8)); frame.Add((byte)(payload.Length & 0xFF)); }
        frame.AddRange(mask);
        for (int i = 0; i < payload.Length; i++) frame.Add((byte)(payload[i] ^ mask[i % 4]));
        byte[] f = frame.ToArray();
        lock (_sendLock) { _stream?.Write(f, 0, f.Length); }
    }

    private void SendPong(byte[] payload)
    {
        var f = new List<byte> { 0x8A, (byte)payload.Length };
        f.AddRange(payload);
        byte[] fb = f.ToArray();
        lock (_sendLock) { _stream?.Write(fb, 0, fb.Length); }
    }

    // ─── API pubblica ─────────────────────────────────────────────────────────
    public void SendJson<T>(T data)
    {
        if (!IsConnected) { Debug.LogWarning("[DT] Non connesso."); return; }
        _sendQueue.Enqueue(JsonUtility.ToJson(data));
    }

    // ─── Dispatch messaggi (main thread) ─────────────────────────────────────
    private void HandleMessage(string json)
    {
        if (json == "__CONNECTED__")    { OnConnected?.Invoke(); return; }
        if (json == "__DISCONNECTED__")
        {
            _connected = false;
            Disconnect();
            OnDisconnected?.Invoke();
            if (_shouldReconnect) StartCoroutine(ReconnectCoroutine());
            return;
        }
        try
        {
            var env = JsonUtility.FromJson<DTMsgEnvelope>(json);
            if (env == null) return;

            if (env.tipo == "metriche_dss")
            {
                var m = JsonUtility.FromJson<DTMetricheDSSMsg>(json);
                LastMetriche = m;  // polling — accessibile senza eventi
                Debug.Log($"[DT] metriche: visitatori={m.visitatori} vr={m.visitatori_vr}");
                OnMetricheDSSReceived?.Invoke(m);
                return;
            }
            if (env.tipo == "storico_dss")
            {
                var s = JsonUtility.FromJson<DTStoricoResponseMsg>(json);
                LastStorico = s;
                Debug.Log($"[DT] Storico media={s.media} errore={s.errore_standard}");
                OnStoricoDSSReceived?.Invoke(s);
                return;
            }
            if (!string.IsNullOrEmpty(env.stato_varco))
            {
                LastStatoVarco = env.stato_varco;
                OnStatoVarcoReceived?.Invoke(env.stato_varco);
                return;
            }
            if (json.Contains("\"errore\""))
            {
                Debug.LogWarning($"[DT] Errore server: {json}");
                return;
            }
        }
        catch (Exception e) { Debug.LogError($"[DT] Parse: {e.Message}"); }
    }

    // ─── Riconnessione ────────────────────────────────────────────────────────
    private IEnumerator ReconnectCoroutine()
    {
        int   max   = dtConfig?.maxReconnectAttempts ?? 0;
        float delay = dtConfig?.reconnectDelaySeconds ?? 3f;
        if (max > 0 && _reconnectAttempts >= max) { Debug.LogError("[DT] Max tentativi."); yield break; }
        _reconnectAttempts++;
        Debug.Log($"[DT] Riconnessione #{_reconnectAttempts} tra {delay}s...");
        yield return new WaitForSeconds(delay);
        StartCoroutine(ConnectCoroutine());
    }
}
