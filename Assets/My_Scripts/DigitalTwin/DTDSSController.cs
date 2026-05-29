// =============================================================================
// DTDSSController.cs — Controller VR DSS (PC Dashboard Decisore)
// My_Scripts/DigitalTwin/DTDSSController.cs
//
// UTILIZZO IN SCENA (Dashboard PC):
//   1. Aggiungi a un GameObject nella scena del DSS
//   2. Assegna i riferimenti TextMeshPro/Slider nell'Inspector
//   3. Le metriche si aggiornano automaticamente ogni 10s (push dal server)
//      e su richiesta esplicita (PollMetriche())
//
// RESPONSABILITÀ:
//   • Riceve metriche complete dal server (visitatori, storici, media, std)
//   • Aggiorna UI (TextMeshPro, Slider) con i dati ricevuti
//   • Permette al decisore di impostare le soglie semaforo
//   • Segue il pattern MapGapBehaviour (aggiornamento colore, testo) del progetto
// =============================================================================

using System;
using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DigitalTwin;

public class DTDSSController : MonoBehaviour
{
    // ─── Inspector — Riferimenti UI ───────────────────────────────────────────
    [Header("Pannello Stato Varco")]
    [SerializeField] private TextMeshProUGUI testoStatoVarco;
    [SerializeField] private TextMeshProUGUI testoVisitatori;
    [SerializeField] private TextMeshProUGUI testoVisitatoriVR;

    [Header("Pannello Metriche Statistiche")]
    [SerializeField] private TextMeshProUGUI testoMediaIngressi;
    [SerializeField] private TextMeshProUGUI testoDeviazioneStandard;

    [Header("Pannello Soglie Semaforo")]
    [SerializeField] private Slider sliderSogliaIntermedia;
    [SerializeField] private Slider sliderSogliaLimite;
    [SerializeField] private TextMeshProUGUI testoSogliaIntermedia;
    [SerializeField] private TextMeshProUGUI testoSogliaLimite;
    [SerializeField] private Button btnInviaSoglie;

    [Header("Colori Stato Varco")]
    [SerializeField] private Color coloreVarcoOk      = Color.green;
    [SerializeField] private Color coloreVarcoErrore  = Color.red;

    [Header("Debug")]
    [SerializeField] private bool logMetriche = true;

    // ─── Stato interno ────────────────────────────────────────────────────────
    private DTMetricheDSSMsg _ultimaMetrica;

    /// <summary>Evento per altri componenti che vogliono reagire alle nuove metriche.</summary>
    public event Action<DTMetricheDSSMsg> OnMetricheAggiornate;

    // ─── Unity Lifecycle ──────────────────────────────────────────────────────
    private void OnEnable()
    {
        // Collega listener slider e pulsante
        sliderSogliaIntermedia?.onValueChanged.AddListener(OnSliderIntermediaChanged);
        sliderSogliaLimite?.onValueChanged.AddListener(OnSliderLimiteChanged);
        btnInviaSoglie?.onClick.AddListener(InviaSoglie);

        if (DTWebSocketClient.Instance != null)
        {
            DTWebSocketClient.Instance.OnMetricheDSSReceived += HandleMetriche;
            DTWebSocketClient.Instance.OnStatoVarcoReceived  += HandleStatoVarco;
            DTWebSocketClient.Instance.OnConnected           += OnConnectedToServer;

            // Se connessione già attiva (race condition) richiedi metriche subito
            if (DTWebSocketClient.Instance.IsConnected)
            {
                Debug.Log("[DT DSS] Già connesso — richiedo metriche subito.");
                PollMetriche();
            }
        }
        else
        {
            StartCoroutine(AttesaClientDSS());
        }
    }

    private System.Collections.IEnumerator AttesaClientDSS()
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
        DTWebSocketClient.Instance.OnConnected           += OnConnectedToServer;

        if (DTWebSocketClient.Instance.IsConnected)
            PollMetriche();
    }

    private void OnDisable()
    {
        if (DTWebSocketClient.Instance != null)
        {
            DTWebSocketClient.Instance.OnMetricheDSSReceived -= HandleMetriche;
            DTWebSocketClient.Instance.OnStatoVarcoReceived  -= HandleStatoVarco;
            DTWebSocketClient.Instance.OnConnected           -= OnConnectedToServer;
        }

        sliderSogliaIntermedia?.onValueChanged.RemoveListener(OnSliderIntermediaChanged);
        sliderSogliaLimite?.onValueChanged.RemoveListener(OnSliderLimiteChanged);
        btnInviaSoglie?.onClick.RemoveListener(InviaSoglie);
    }

    // ─── Handlers messaggi server ─────────────────────────────────────────────

    private void OnConnectedToServer()
    {
        // Richiedi subito le metriche aggiornate alla connessione
        PollMetriche();
    }

    private void HandleStatoVarco(string statoVarco)
    {
        // Aggiornamento urgente stato varco (event-driven)
        AggiornaTesto(testoStatoVarco, statoVarco == DTStati.VARCO_OK ? "✓ OPERATIVO" : "⚠ FUORI SERVIZIO");
        if (testoStatoVarco != null)
            testoStatoVarco.color = statoVarco == DTStati.VARCO_OK ? coloreVarcoOk : coloreVarcoErrore;
    }

    private void HandleMetriche(DTMetricheDSSMsg metriche)
    {
        _ultimaMetrica = metriche;

        if (logMetriche)
            Debug.Log($"[DT DSS] Metriche: visitatori={metriche.visitatori} " +
                      $"| vr={metriche.visitatori_vr} " +
                      $"| media={metriche.media_ingressi:F1} " +
                      $"| std={metriche.deviazione_standard_ingressi:F1}");

        AggiornaUI(metriche);
        OnMetricheAggiornate?.Invoke(metriche);
    }

    // ─── Aggiornamento UI ─────────────────────────────────────────────────────

    private void AggiornaUI(DTMetricheDSSMsg m)
    {
        // Stato varco
        bool ok = m.stato_varco == DTStati.VARCO_OK;
        AggiornaTesto(testoStatoVarco, ok ? "✓ OPERATIVO" : "⚠ FUORI SERVIZIO");
        if (testoStatoVarco != null) testoStatoVarco.color = ok ? coloreVarcoOk : coloreVarcoErrore;

        // Contatori
        AggiornaTesto(testoVisitatori,   $"{m.visitatori}");
        AggiornaTesto(testoVisitatoriVR, $"{m.visitatori_vr}");

        // Metriche statistiche (ISO 23247 — solo dati validi)
        AggiornaTesto(testoMediaIngressi,     $"{m.media_ingressi:F1}");
        AggiornaTesto(testoDeviazioneStandard, $"{m.deviazione_standard_ingressi:F1}");

        // Soglie ricevute dal server (sincronizza slider se non in editing)
        if (sliderSogliaIntermedia != null && !sliderSogliaIntermedia.IsInteractable() == false)
            sliderSogliaIntermedia.value = m.soglia_intermedia;
        if (sliderSogliaLimite != null)
            sliderSogliaLimite.value = m.soglia_limite;

        AggiornaTesto(testoSogliaIntermedia, $"{m.soglia_intermedia}");
        AggiornaTesto(testoSogliaLimite,     $"{m.soglia_limite}");
    }

    private static void AggiornaTesto(TextMeshProUGUI campo, string testo)
    {
        if (campo != null) campo.text = testo;
    }

    // ─── Slider callbacks ─────────────────────────────────────────────────────

    private void OnSliderIntermediaChanged(float val)
    {
        AggiornaTesto(testoSogliaIntermedia, $"{(int)val}");
    }

    private void OnSliderLimiteChanged(float val)
    {
        AggiornaTesto(testoSogliaLimite, $"{(int)val}");
    }

    // ─── API pubblica ─────────────────────────────────────────────────────────

    /// <summary>
    /// Invia al server le soglie correnti dagli slider.
    /// Collegare al pulsante "Applica Soglie" nell'Inspector o via codice.
    /// </summary>
    public void InviaSoglie()
    {
        if (DTWebSocketClient.Instance == null || !DTWebSocketClient.Instance.IsConnected)
        {
            Debug.LogWarning("[DT DSS] InviaSoglie: non connesso al server.");
            return;
        }

        int intermedia = sliderSogliaIntermedia != null ? (int)sliderSogliaIntermedia.value : 5;
        int limite     = sliderSogliaLimite     != null ? (int)sliderSogliaLimite.value     : 10;

        // Garantisce che la soglia limite sia sempre >= intermedia
        if (limite <= intermedia)
        {
            Debug.LogWarning($"[DT DSS] Soglia limite ({limite}) <= intermedia ({intermedia}): aggiustata automaticamente.");
            limite = intermedia + 1;
            if (sliderSogliaLimite != null) sliderSogliaLimite.value = limite;
        }

        var msg = new DTSoglieMsg
        {
            soglia_ingressi_intermedia = intermedia,
            soglia_ingressi_limite     = limite
        };

        DTWebSocketClient.Instance.SendJson(msg);
        Debug.Log($"[DT DSS] Soglie inviate: intermedia={intermedia}, limite={limite}");
    }

    /// <summary>
    /// Richiede on-demand le metriche aggiornate al server (polling esplicito).
    /// Collegare a un pulsante "Aggiorna" nella UI DSS.
    /// </summary>
    public void PollMetriche()
    {
        if (DTWebSocketClient.Instance == null || !DTWebSocketClient.Instance.IsConnected)
        {
            Debug.LogWarning("[DT DSS] PollMetriche: non connesso.");
            return;
        }

        DTWebSocketClient.Instance.SendJson(new DTRichiestaMetricheMsg());
        Debug.Log("[DT DSS] Richiesta metriche on-demand inviata.");
    }

    /// <summary>
    /// Imposta le soglie via codice (es. da VoiceScript o da altri sistemi).
    /// </summary>
    public void ImpostaSoglie(int intermedia, int limite)
    {
        if (sliderSogliaIntermedia != null) sliderSogliaIntermedia.value = intermedia;
        if (sliderSogliaLimite != null)     sliderSogliaLimite.value     = limite;
        InviaSoglie();
    }

    /// <summary>Ultima metrica ricevuta dal server (può essere null).</summary>
    public DTMetricheDSSMsg UltimaMetrica => _ultimaMetrica;
}
