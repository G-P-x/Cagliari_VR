// =============================================================================
// DTDSSController.cs — Controller VR DSS
// My_Scripts/DigitalTwin/DTDSSController.cs
//
// PUSH  → riceve metriche real-time ogni 10s dal server (stato_varco,
//         visitatori, visitatori_vr)
// PULL  → su richiesta del decisore invia intervallo date e riceve
//         media e errore_standard calcolati dal server
// =============================================================================

using System;
using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DigitalTwin;

public class DTDSSController : MonoBehaviour
{
    // ─── Pannello PUSH — Metriche Real-Time ──────────────────────────────────
    [Header("Metriche Real-Time (PUSH)")]
    [SerializeField] private TextMeshProUGUI testoStatoVarco;
    [SerializeField] private TextMeshProUGUI testoVisitatori;
    [SerializeField] private TextMeshProUGUI testoVisitatoriVR;

    // ─── Pannello PULL — Storico On-Demand ───────────────────────────────────
    [Header("Pannello Storico (PULL on-demand)")]
    [SerializeField] private GameObject     pannelloStorico;     // GameObject da mostrare/nascondere
    [SerializeField] private TMP_InputField inputDataStart;      // YYYY-MM-DD
    [SerializeField] private TMP_InputField inputDataEnd;        // YYYY-MM-DD
    [SerializeField] private Button         btnRichiestaStorico;
    [SerializeField] private TextMeshProUGUI testoMedia;         // risultato media
    [SerializeField] private TextMeshProUGUI testoErroreStandard;// risultato errore_standard
    [SerializeField] private TextMeshProUGUI testoCampioni;      // n giorni calcolati
    [SerializeField] private TextMeshProUGUI testoStatoStorico;  // feedback / errori

    // ─── Pannello PULL — Soglie di Controllo ────────────────────────────────
    [Header("Pannello Soglie di Controllo")]
    [SerializeField] private GameObject     pannelloSoglie;
    [SerializeField] private Slider         sliderSogliaIntermedia;
    [SerializeField] private Slider         sliderSogliaLimite;
    [SerializeField] private TextMeshProUGUI testoValoreIntermedia;
    [SerializeField] private TextMeshProUGUI testoValoreLimite;
    [SerializeField] private Button         btnImpostaSoglie;
    [SerializeField] private Button         btnToggleSoglie;

    // ─── Pulsante toggle pannello storico ────────────────────────────────────
    [Header("Toggle Storico")]
    [SerializeField] private Button btnToggleStorico;

    // ─── Colori ───────────────────────────────────────────────────────────────
    [Header("Colori")]
    [SerializeField] private Color coloreVarcoOk     = Color.green;
    [SerializeField] private Color coloreVarcoErrore = Color.red;

    [Header("Debug")]
    [SerializeField] private bool logMetriche = true;

    private bool _storicoPanelVisible = false;
    private bool _sogliePanelVisible  = false;

    // ─── Unity Lifecycle ──────────────────────────────────────────────────────
    private void OnEnable()
    {
        btnRichiestaStorico?.onClick.AddListener(RichiestaStorico);
        btnToggleStorico?.onClick.AddListener(TogglePannelloStorico);
        btnImpostaSoglie?.onClick.AddListener(InviaSoglie);
        btnToggleSoglie?.onClick.AddListener(TogglePannelloSoglie);
        sliderSogliaIntermedia?.onValueChanged.AddListener(OnSliderIntermediaChanged);
        sliderSogliaLimite?.onValueChanged.AddListener(OnSliderLimiteChanged);

        pannelloStorico?.SetActive(false);
        pannelloSoglie?.SetActive(false);

        if (DTWebSocketClient.Instance != null)
        {
            DTWebSocketClient.Instance.OnMetricheDSSReceived += HandleMetriche;
            DTWebSocketClient.Instance.OnStatoVarcoReceived  += HandleStatoVarco;
            DTWebSocketClient.Instance.OnStoricoDSSReceived  += HandleStorico;
            DTWebSocketClient.Instance.OnConnected           += OnConnectedToServer;
        }
        else
        {
            StartCoroutine(AttesaClientDSS());
        }
    }

    private void OnDisable()
    {
        btnRichiestaStorico?.onClick.RemoveListener(RichiestaStorico);
        btnToggleStorico?.onClick.RemoveListener(TogglePannelloStorico);
        btnImpostaSoglie?.onClick.RemoveListener(InviaSoglie);
        btnToggleSoglie?.onClick.RemoveListener(TogglePannelloSoglie);
        sliderSogliaIntermedia?.onValueChanged.RemoveListener(OnSliderIntermediaChanged);
        sliderSogliaLimite?.onValueChanged.RemoveListener(OnSliderLimiteChanged);

        if (DTWebSocketClient.Instance != null)
        {
            DTWebSocketClient.Instance.OnMetricheDSSReceived -= HandleMetriche;
            DTWebSocketClient.Instance.OnStatoVarcoReceived  -= HandleStatoVarco;
            DTWebSocketClient.Instance.OnStoricoDSSReceived  -= HandleStorico;
            DTWebSocketClient.Instance.OnConnected           -= OnConnectedToServer;
        }
    }

    private IEnumerator AttesaClientDSS()
    {
        float timeout = 10f;
        while (DTWebSocketClient.Instance == null && timeout > 0f)
        {
            timeout -= Time.deltaTime;
            yield return null;
        }
        if (DTWebSocketClient.Instance == null)
        {
            Debug.LogError("[DT DSS] DTWebSocketClient non trovato dopo 10s.");
            yield break;
        }
        DTWebSocketClient.Instance.OnMetricheDSSReceived += HandleMetriche;
        DTWebSocketClient.Instance.OnStatoVarcoReceived  += HandleStatoVarco;
        DTWebSocketClient.Instance.OnStoricoDSSReceived  += HandleStorico;
        DTWebSocketClient.Instance.OnConnected           += OnConnectedToServer;
    }

    // ─── Handlers PUSH ────────────────────────────────────────────────────────
    private void OnConnectedToServer()
    {
        Debug.Log("[DT DSS] Connesso — in attesa PUSH dal server.");
    }

    private void HandleStatoVarco(string statoVarco)
    {
        bool ok = statoVarco == DTStati.VARCO_OK;
        if (testoStatoVarco != null)
        {
            testoStatoVarco.text  = ok ? "✓ OPERATIVO" : "⚠ FUORI SERVIZIO";
            testoStatoVarco.color = ok ? coloreVarcoOk : coloreVarcoErrore;
        }
    }

    private void HandleMetriche(DTMetricheDSSMsg m)
    {
        if (logMetriche)
            Debug.Log($"[DT DSS] visitatori={m.visitatori} visitatori_vr={m.visitatori_vr} stato={m.stato_varco}");

        bool ok = m.stato_varco == DTStati.VARCO_OK;
        if (testoStatoVarco != null)
        {
            testoStatoVarco.text  = ok ? "✓ OPERATIVO" : "⚠ FUORI SERVIZIO";
            testoStatoVarco.color = ok ? coloreVarcoOk : coloreVarcoErrore;
        }

        if (testoVisitatori   != null) testoVisitatori.text   = $"{m.visitatori}";
        if (testoVisitatoriVR != null) testoVisitatoriVR.text = $"{m.visitatori_vr}";
    }

    // ─── Handler PULL Storico ─────────────────────────────────────────────────
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

        Debug.Log($"[DT DSS] Storico ricevuto | media={s.media} errore_standard={s.errore_standard} campioni={s.campioni}");
    }

    // ─── API pubblica ─────────────────────────────────────────────────────────

    // ─── Callbacks slider soglie ─────────────────────────────────────────────
    private void OnSliderIntermediaChanged(float val)
    {
        if (testoValoreIntermedia != null) testoValoreIntermedia.text = $"{(int)val}";
    }

    private void OnSliderLimiteChanged(float val)
    {
        if (testoValoreLimite != null) testoValoreLimite.text = $"{(int)val}";
    }

    // ─── API Soglie ───────────────────────────────────────────────────────────

    /// <summary>Toggle visibilità pannello soglie di controllo.</summary>
    public void TogglePannelloSoglie()
    {
        _sogliePanelVisible = !_sogliePanelVisible;
        pannelloSoglie?.SetActive(_sogliePanelVisible);
    }

    /// <summary>
    /// Legge i valori dagli slider e invia le soglie al server.
    /// Payload: { "soglia_ingressi_intermedia": int, "soglia_ingressi_limite": int }
    /// </summary>
    public void InviaSoglie()
    {
        if (DTWebSocketClient.Instance == null || !DTWebSocketClient.Instance.IsConnected)
        {
            Debug.LogWarning("[DT DSS] InviaSoglie: non connesso.");
            return;
        }

        int intermedia = sliderSogliaIntermedia != null ? (int)sliderSogliaIntermedia.value : 5;
        int limite     = sliderSogliaLimite     != null ? (int)sliderSogliaLimite.value     : 10;

        if (limite <= intermedia)
        {
            Debug.LogWarning($"[DT DSS] Soglia limite ({limite}) deve essere > intermedia ({intermedia}).");
            limite = intermedia + 1;
            if (sliderSogliaLimite != null) sliderSogliaLimite.value = limite;
        }

        var msg = new DTSoglieMsg
        {
            soglia_ingressi_intermedia = intermedia,
            soglia_ingressi_limite     = limite
        };

        DTWebSocketClient.Instance.SendJson(msg);
        Debug.Log($"[DT DSS] Soglie inviate → intermedia={intermedia}, limite={limite}");
    }

    /// <summary>Toggle visibilità pannello storico.</summary>
    public void TogglePannelloStorico()
    {
        _storicoPanelVisible = !_storicoPanelVisible;
        pannelloStorico?.SetActive(_storicoPanelVisible);
    }

    /// <summary>
    /// Invia richiesta PULL storico al server con l'intervallo date inserito
    /// dal decisore nei campi inputDataStart e inputDataEnd.
    /// </summary>
    public void RichiestaStorico()
    {
        if (DTWebSocketClient.Instance == null || !DTWebSocketClient.Instance.IsConnected)
        {
            Debug.LogWarning("[DT DSS] RichiestaStorico: non connesso.");
            return;
        }

        string dataStart = inputDataStart?.text?.Trim();
        string dataEnd   = inputDataEnd?.text?.Trim();

        if (string.IsNullOrEmpty(dataStart) || string.IsNullOrEmpty(dataEnd))
        {
            if (testoStatoStorico != null)
            {
                testoStatoStorico.text  = "⚠ Inserisci data_start e data_end";
                testoStatoStorico.color = coloreVarcoErrore;
            }
            return;
        }

        if (testoStatoStorico != null)
        {
            testoStatoStorico.text  = "⏳ Richiesta in corso...";
            testoStatoStorico.color = Color.yellow;
        }

        var msg = new DTStoricoRequestMsg
        {
            data_start = dataStart,
            data_end   = dataEnd
        };

        DTWebSocketClient.Instance.SendJson(msg);
        Debug.Log($"[DT DSS] Richiesta storico: {dataStart} → {dataEnd}");
    }
}
