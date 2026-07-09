// =============================================================================
// DTDSSController.cs — Controller VR DSS
// Usa polling diretto su DTWebSocketClient.LastMetriche in Update()
// invece di sottoscrizione eventi — elimina il problema di OnDisable/OnEnable
// =============================================================================

using System;
using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DigitalTwin;

public class DTDSSController : MonoBehaviour
{
    // ─── Metriche Real-Time ───────────────────────────────────────────────────
    [Header("Metriche Real-Time (PUSH)")]
    [SerializeField] private TextMeshProUGUI testoStatoVarco;
    [SerializeField] private TextMeshProUGUI testoVisitatori;
    [SerializeField] private TextMeshProUGUI testoVisitatoriVR;

    // ─── Pannello Storico ─────────────────────────────────────────────────────
    [Header("Pannello Storico (PULL on-demand)")]
    [SerializeField] private GameObject      pannelloStorico;
    [SerializeField] private TMP_InputField  inputDataStart;
    [SerializeField] private TMP_InputField  inputDataEnd;
    [SerializeField] private Button          btnRichiestaStorico;
    [SerializeField] private TextMeshProUGUI testoMedia;
    [SerializeField] private TextMeshProUGUI testoErroreStandard;
    [SerializeField] private TextMeshProUGUI testoCampioni;
    [SerializeField] private TextMeshProUGUI testoStatoStorico;
    [SerializeField] private Button          btnToggleStorico;

    // ─── Pannello Soglie ──────────────────────────────────────────────────────
    [Header("Pannello Soglie di Controllo")]
    [SerializeField] private GameObject      pannelloSoglie;
    [SerializeField] private Slider          sliderSogliaIntermedia;
    [SerializeField] private Slider          sliderSogliaLimite;
    [SerializeField] private TextMeshProUGUI testoValoreIntermedia;
    [SerializeField] private TextMeshProUGUI testoValoreLimite;
    [SerializeField] private Button          btnImpostaSoglie;
    [SerializeField] private Button          btnToggleSoglie;

    // ─── Colori ───────────────────────────────────────────────────────────────
    [Header("Colori")]
    [SerializeField] private Color coloreVarcoOk     = Color.green;
    [SerializeField] private Color coloreVarcoErrore = Color.red;

    // ─── Stato interno ────────────────────────────────────────────────────────
    private bool _storicoPanelVisible = false;
    private bool _sogliePanelVisible  = false;

    // Riferimento all'ultima metrica mostrata — ReferenceEquals rileva ogni push
    private DTMetricheDSSMsg _ultimaMetrica   = null;
    private string           _ultimoStatoVarco = null;

    // ─── Unity Lifecycle ──────────────────────────────────────────────────────
    private void Start()
    {
        pannelloStorico?.SetActive(false);
        pannelloSoglie?.SetActive(false);

        btnRichiestaStorico?.onClick.AddListener(RichiestaStorico);
        btnToggleStorico?.onClick.AddListener(TogglePannelloStorico);
        btnImpostaSoglie?.onClick.AddListener(InviaSoglie);
        btnToggleSoglie?.onClick.AddListener(TogglePannelloSoglie);
        sliderSogliaIntermedia?.onValueChanged.AddListener(OnSliderIntermediaChanged);
        sliderSogliaLimite?.onValueChanged.AddListener(OnSliderLimiteChanged);

        // Rimane in ascolto per risposta storico
        if (DTWebSocketClient.Instance != null)
            DTWebSocketClient.Instance.OnStoricoDSSReceived += HandleStorico;
    }

    private void OnDestroy()
    {
        btnRichiestaStorico?.onClick.RemoveListener(RichiestaStorico);
        btnToggleStorico?.onClick.RemoveListener(TogglePannelloStorico);
        btnImpostaSoglie?.onClick.RemoveListener(InviaSoglie);
        btnToggleSoglie?.onClick.RemoveListener(TogglePannelloSoglie);
        sliderSogliaIntermedia?.onValueChanged.RemoveListener(OnSliderIntermediaChanged);
        sliderSogliaLimite?.onValueChanged.RemoveListener(OnSliderLimiteChanged);
        if (DTWebSocketClient.Instance != null)
            DTWebSocketClient.Instance.OnStoricoDSSReceived -= HandleStorico;
    }

    // ─── Update — polling diretto (no eventi) ─────────────────────────────────
    private void Update()
    {
        if (DTWebSocketClient.Instance == null) return;

        // Aggiorna stato varco se cambiato
        string statoVarco = DTWebSocketClient.Instance.LastStatoVarco;
        if (statoVarco != null && statoVarco != _ultimoStatoVarco)
        {
            _ultimoStatoVarco = statoVarco;
            AggiornaStatoVarco(statoVarco);
        }

        // Aggiorna metriche ad ogni nuovo push (confronto riferimento oggetto)
        // JsonUtility.FromJson crea sempre un nuovo oggetto → ogni push è rilevato
        DTMetricheDSSMsg m = DTWebSocketClient.Instance.LastMetriche;
        if (m != null && !ReferenceEquals(m, _ultimaMetrica))
        {
            _ultimaMetrica = m;
            AggiornaUI(m);
        }
    }

    // ─── Aggiornamento UI ─────────────────────────────────────────────────────
    private void AggiornaStatoVarco(string statoVarco)
    {
        bool ok = statoVarco == DTStati.VARCO_OK;
        if (testoStatoVarco != null)
        {
            testoStatoVarco.text  = ok ? "✓ OPERATIVO" : "⚠ FUORI SERVIZIO";
            testoStatoVarco.color = ok ? coloreVarcoOk : coloreVarcoErrore;
        }
    }

    // ─── Logging M1c ──────────────────────────────────────────────────────────
    private static int   _m1cEventId  = 0;
    private static bool  _m1cHeaderOk = false;

    private void LogM1c(long serverTs, long clientTs, float delta)
    {
        string path = System.IO.Path.Combine(
            Application.persistentDataPath, "latency_m1c.csv");
        bool exists = System.IO.File.Exists(path);
        using var w = new System.IO.StreamWriter(path, append: true);
        if (!exists || !_m1cHeaderOk)
        {
            w.WriteLine("event_id,server_timestamp_ms,unity_received_ms,delta_ms");
            _m1cHeaderOk = true;
        }
        w.WriteLine($"{++_m1cEventId},{serverTs},{clientTs},{delta:F1}");
    }

    private void AggiornaUI(DTMetricheDSSMsg m)
    {
        // M1c — latenza Server → VR (Unity)
        if (m.server_timestamp_ms > 0)
        {
            long  clientTs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            float delta    = clientTs - m.server_timestamp_ms;
            LogM1c(m.server_timestamp_ms, clientTs, delta);
            Debug.Log($"[M1c] Latenza Server→VR: {delta:F0}ms");
        }

        AggiornaStatoVarco(m.stato_varco);
        if (testoVisitatori   != null) testoVisitatori.text   = $"{m.visitatori}";
        if (testoVisitatoriVR != null) testoVisitatoriVR.text = $"{m.visitatori_vr}";
        Debug.Log($"[DT DSS] UI aggiornata: visitatori={m.visitatori} vr={m.visitatori_vr}");
    }

    // ─── Handler storico (evento — risposta one-shot) ─────────────────────────
    private void HandleStorico(DTStoricoResponseMsg s)
    {
        if (!string.IsNullOrEmpty(s.errore))
        {
            if (testoStatoStorico != null)
            {
                testoStatoStorico.text  = $"⚠ {s.errore}";
                testoStatoStorico.color = coloreVarcoErrore;
            }
            return;
        }
        if (testoMedia          != null) testoMedia.text          = $"{s.media:F2}";
        if (testoErroreStandard != null) testoErroreStandard.text = $"{s.errore_standard:F2}";
        if (testoCampioni       != null) testoCampioni.text       = $"{s.campioni} giorni";
        if (testoStatoStorico   != null)
        {
            testoStatoStorico.text  = $"✓ {s.data_start} → {s.data_end}";
            testoStatoStorico.color = coloreVarcoOk;
        }
    }

    // ─── Slider callbacks ─────────────────────────────────────────────────────
    private void OnSliderIntermediaChanged(float val)
    { if (testoValoreIntermedia != null) testoValoreIntermedia.text = $"{(int)val}"; }

    private void OnSliderLimiteChanged(float val)
    { if (testoValoreLimite != null) testoValoreLimite.text = $"{(int)val}"; }

    // ─── API pubblica ─────────────────────────────────────────────────────────
    public void TogglePannelloStorico()
    {
        _storicoPanelVisible = !_storicoPanelVisible;
        pannelloStorico?.SetActive(_storicoPanelVisible);
    }

    public void TogglePannelloSoglie()
    {
        _sogliePanelVisible = !_sogliePanelVisible;
        pannelloSoglie?.SetActive(_sogliePanelVisible);
    }

    public void RichiestaStorico()
    {
        if (DTWebSocketClient.Instance == null || !DTWebSocketClient.Instance.IsConnected)
        { Debug.LogWarning("[DT DSS] Non connesso."); return; }

        string start = inputDataStart?.text?.Trim();
        string end   = inputDataEnd?.text?.Trim();

        if (string.IsNullOrEmpty(start) || string.IsNullOrEmpty(end))
        {
            if (testoStatoStorico != null)
            { testoStatoStorico.text = "⚠ Inserisci data_start e data_end"; testoStatoStorico.color = coloreVarcoErrore; }
            return;
        }
        if (testoStatoStorico != null)
        { testoStatoStorico.text = "⏳ Richiesta in corso..."; testoStatoStorico.color = Color.yellow; }

        DTWebSocketClient.Instance.SendJson(new DTStoricoRequestMsg { data_start = start, data_end = end });
    }

    public void InviaSoglie()
    {
        if (DTWebSocketClient.Instance == null || !DTWebSocketClient.Instance.IsConnected)
        { Debug.LogWarning("[DT DSS] Non connesso."); return; }

        int intermedia = sliderSogliaIntermedia != null ? (int)sliderSogliaIntermedia.value : 5;
        int limite     = sliderSogliaLimite     != null ? (int)sliderSogliaLimite.value     : 10;
        if (limite <= intermedia) limite = intermedia + 1;

        DTWebSocketClient.Instance.SendJson(new DTSoglieMsg
        {
            soglia_ingressi_intermedia = intermedia,
            soglia_ingressi_limite     = limite
        });
        Debug.Log($"[DT DSS] Soglie → intermedia={intermedia}, limite={limite}");
    }
}
