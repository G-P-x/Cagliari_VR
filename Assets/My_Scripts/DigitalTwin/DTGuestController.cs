// =============================================================================
// DTGuestController.cs — Controller VR Guest (Meta Quest 3)
// My_Scripts/DigitalTwin/DTGuestController.cs
//
// Gestisce il pannello UI nella scena VR_GUEST_Home:
//   • Mostra "VARCO PRINCIPALE — {stato_varco}" aggiornato dal server via PUSH
//   • Al click del button invia DTEventoVRGuestMsg (ingresso/uscita alternati)
//   • Aggiorna testo, colore e label azione ad ogni cambio di stato
// =============================================================================

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DigitalTwin;

public class DTGuestController : MonoBehaviour
{
    // ─── Inspector ────────────────────────────────────────────────────────────
    [Header("Configurazione Digital Twin")]
    [SerializeField] private DTConfig dtConfig;

    [Header("Pannello Varco")]
    [SerializeField] private Button          btnVarco;
    [SerializeField] private TextMeshProUGUI testoStatoVarco;   // "VARCO PRINCIPALE\n✓ OPERATIVO"
    [SerializeField] private TextMeshProUGUI testoAzione;       // "Tocca per accedere" / "Tocca per uscire"
    [SerializeField] private Image           sfondoBottone;     // colore feedback

    [Header("Semaforo 3D (opzionale)")]
    [SerializeField] private DTSemaforoVisual semaforoVisual;

    [Header("Colori")]
    [SerializeField] private Color coloreOperativo   = new Color(0.15f, 0.60f, 0.25f, 1f);
    [SerializeField] private Color coloreFuoriServ   = new Color(0.70f, 0.15f, 0.15f, 1f);
    [SerializeField] private Color coloreDentro      = new Color(0.20f, 0.45f, 0.75f, 1f);

    [Header("Debug")]
    [SerializeField] private bool logEventiVR = true;

    // ─── Stato interno ────────────────────────────────────────────────────────
    private string _statoVarco   = DTStati.VARCO_OK;
    private bool   _sessione     = false;   // true = utente "dentro" VR

    public string StatoVarco        => _statoVarco;
    public bool   IsVarcoOperativo  => _statoVarco == DTStati.VARCO_OK;
    public bool   IsSessioneAttiva  => _sessione;

    public event Action<string> OnStatoVarcoChanged;

    // ─── Unity Lifecycle ──────────────────────────────────────────────────────

    private void Start()
    {
        StartCoroutine(OnVarcoButtonClickedSimulator());
        
    }
    private void OnEnable()
    {
        btnVarco?.onClick.AddListener(OnVarcoButtonClicked);

        AggiornaPannello();   // stato iniziale

        if (DTWebSocketClient.Instance != null)
        {
            DTWebSocketClient.Instance.OnStatoVarcoReceived += HandleStatoVarco;
            DTWebSocketClient.Instance.OnConnected          += HandleConnected;
            if (DTWebSocketClient.Instance.IsConnected)
                InviaEventoVR(ingresso: true);
        }
        else
        {
            StartCoroutine(AttesaClient());
        }
    }

    private void OnDisable()
    {
        btnVarco?.onClick.RemoveListener(OnVarcoButtonClicked);

        if (_sessione)
            InviaEventoVR(ingresso: false);

        if (DTWebSocketClient.Instance != null)
        {
            DTWebSocketClient.Instance.OnStatoVarcoReceived -= HandleStatoVarco;
            DTWebSocketClient.Instance.OnConnected          -= HandleConnected;
        }
    }

    private IEnumerator AttesaClient()
    {
        float timeout = 10f;
        while (DTWebSocketClient.Instance == null && timeout > 0f)
        { timeout -= Time.deltaTime; yield return null; }

        if (DTWebSocketClient.Instance == null)
        {
            Debug.LogError("[DT Guest] DTWebSocketClient non trovato dopo 10s.");
            yield break;
        }

        DTWebSocketClient.Instance.OnStatoVarcoReceived += HandleStatoVarco;
        DTWebSocketClient.Instance.OnConnected          += HandleConnected;

        if (DTWebSocketClient.Instance.IsConnected)
            InviaEventoVR(ingresso: true);
    }

    // ─── Handlers server ──────────────────────────────────────────────────────
    private void HandleConnected()
    {
        if (gameObject.activeInHierarchy)
            InviaEventoVR(ingresso: true);
    }

    private void HandleStatoVarco(string stato)
    {
        if (_statoVarco == stato) return;
        _statoVarco = stato;

        if (logEventiVR)
            Debug.Log($"[DT Guest] stato_varco → {stato}");

        semaforoVisual?.AggiornaDaStatoVarco(stato);
        OnStatoVarcoChanged?.Invoke(stato);
        AggiornaPannello();
    }

    // ─── Click button ─────────────────────────────────────────────────────────
    /// <summary>
    /// Chiamato dal button "VARCO PRINCIPALE".
    /// Alterna ingresso/uscita VR e comunica l'interazione al server.
    /// </summary>
    public void OnVarcoButtonClicked()
    {
        bool nuovoStato = !_sessione;
        InviaEventoVR(ingresso: nuovoStato);
        AggiornaPannello();

        if (logEventiVR)
            Debug.Log($"[DT Guest] Button cliccato → {(nuovoStato ? "INGRESSO" : "USCITA")} VR");
    }
    private IEnumerator OnVarcoButtonClickedSimulator()
    {
        while(true)
        {
            yield return new WaitForSeconds(5f);
            OnVarcoButtonClicked();
            Debug.Log("[DT Guest] Simulazione click button dopo 5s.");
        }
    }

    // ─── Aggiornamento UI ─────────────────────────────────────────────────────
    private void AggiornaPannello()
    {
        bool operativo = _statoVarco == DTStati.VARCO_OK;

        // Testo principale: "VARCO PRINCIPALE\n✓ OPERATIVO"
        if (testoStatoVarco != null)
        {
            string icona  = operativo ? "✓" : "⚠";
            string stato  = operativo ? "OPERATIVO" : "FUORI SERVIZIO";
            testoStatoVarco.text  = $"VARCO PRINCIPALE\n{icona} {stato}";
            testoStatoVarco.color = operativo ? Color.white : new Color(1f, 0.85f, 0.3f);
        }

        // Label azione: guida contestuale
        if (testoAzione != null)
        {
            if (!operativo)
                testoAzione.text = "Varco temporaneamente non disponibile";
            else if (_sessione)
                testoAzione.text = "Tocca per uscire dall'area";
            else
                testoAzione.text = "Tocca per accedere all'area";
        }

        // Colore sfondo button
        if (sfondoBottone != null)
        {
            if (!operativo)
                sfondoBottone.color = coloreFuoriServ;
            else if (_sessione)
                sfondoBottone.color = coloreDentro;
            else
                sfondoBottone.color = coloreOperativo;
        }

        // Interattività: disabilita se varco fuori servizio
        if (btnVarco != null)
            btnVarco.interactable = operativo;
    }

    // ─── Invio evento VR ──────────────────────────────────────────────────────
    public void InviaEventoVR(bool ingresso)
    {
        if (dtConfig == null)
        { Debug.LogWarning("[DT Guest] DTConfig non assegnato."); return; }

        if (DTWebSocketClient.Instance == null || !DTWebSocketClient.Instance.IsConnected)
        {
            if (logEventiVR)
                Debug.LogWarning("[DT Guest] Non connesso al server.");
            return;
        }

        _sessione = ingresso;

        DTWebSocketClient.Instance.SendJson(new DTEventoVRGuestMsg
        {
            ingresso_vr      = ingresso,
            uscita_vr        = !ingresso,
            datetimestamp_vr = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            id_visore        = dtConfig.idVisore
        });

        if (logEventiVR)
            Debug.Log($"[DT Guest] → Server: {(ingresso ? "INGRESSO" : "USCITA")} VR | {dtConfig.idVisore}");
    }

    // ─── API pubblica ─────────────────────────────────────────────────────────
    public void OnVisoreIndossato() => InviaEventoVR(ingresso: true);
    public void OnVisoreRimosso()   => InviaEventoVR(ingresso: false);
}
