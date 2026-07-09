// =============================================================================
// DTSemaforoVisual.cs — Semaforo 3D Visual Component
// My_Scripts/DigitalTwin/DTSemaforoVisual.cs
//
// UTILIZZO IN SCENA:
//   Aggiungi questo script al GameObject 3D del semaforo nella scena VR.
//   Aggiorna colore e materiale del MeshRenderer in base allo stato del varco.
//
// Segue lo stesso pattern di MapGapBehaviour (ChangeColorOnAverageAccess)
// già presente nel progetto: lavora sul MeshRenderer del GameObject corrente.
//
// STATI:
//   stato_varco = "ok"                              → emissione VERDE
//   stato_varco = "Errore di comunicazione EchoBean" → emissione ROSSO (+ testo fuori servizio)
//   stato_semaforo = "verde" / "giallo" / "rosso"   → colore standard semaforo
// =============================================================================

using UnityEngine;
using TMPro;
using DigitalTwin;

public class DTSemaforoVisual : MonoBehaviour
{
    // ─── Inspector ────────────────────────────────────────────────────────────
    [Header("Riferimenti 3D")]
    [Tooltip("MeshRenderer della luce verde del semaforo (o unica luce)")]
    [SerializeField] private MeshRenderer lucePrincipale;

    [Tooltip("Testo 3D opzionale che mostra lo stato testuale")]
    [SerializeField] private TextMeshPro testoStato;

    [Header("Colori Semaforo")]
    [SerializeField] private Color coloreVerde   = Color.green;
    [SerializeField] private Color coloreGiallo  = Color.yellow;
    [SerializeField] private Color coloreRosso   = Color.red;
    [SerializeField] private Color coloreGrigio  = Color.gray;  // Fuori servizio

    [Header("Intensità Emissione HDR")]
    [SerializeField] private float intensitaEmissione = 2f;

    [Header("Stato Iniziale")]
    [SerializeField] private string statoIniziale = DTStati.VARCO_OK;

    // ─── Stato interno ────────────────────────────────────────────────────────
    private Material _materialInstance;
    private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

    // ─── Unity Lifecycle ──────────────────────────────────────────────────────
    private void Awake()
    {
        // Cache material instance — non modifica lo shared material
        // (stesso pattern di MapGapBehaviour.cs del progetto)
        if (lucePrincipale == null)
            lucePrincipale = GetComponent<MeshRenderer>();

        if (lucePrincipale != null)
            _materialInstance = lucePrincipale.material;
    }

    private void Start()
    {
        AggiornaDaStatoVarco(statoIniziale);
    }

    // ─── API pubblica ─────────────────────────────────────────────────────────

    /// <summary>
    /// Aggiorna il semaforo in base allo stato_varco ricevuto dal server.
    /// Chiamato da DTGuestController quando arriva un aggiornamento.
    /// </summary>
    public void AggiornaDaStatoVarco(string statoVarco)
    {
        if (statoVarco == DTStati.VARCO_OK)
        {
            ImpostaColore(coloreVerde, "VARCO APERTO");
        }
        else
        {
            // Qualsiasi stato non-ok → fuori servizio
            ImpostaColore(coloreGrigio, "FUORI SERVIZIO");
        }
    }

    /// <summary>
    /// Aggiorna il semaforo con stato esplicito verde/giallo/rosso.
    /// Utile se la DSS vuole visualizzare lo stato semaforo fisico.
    /// </summary>
    public void AggiornaDaStatoSemaforo(string statoSemaforo)
    {
        switch (statoSemaforo.ToLower())
        {
            case DTStati.SEM_VERDE:
                ImpostaColore(coloreVerde, "ACCESSO LIBERO");
                break;
            case DTStati.SEM_GIALLO:
                ImpostaColore(coloreGiallo, "ATTENZIONE");
                break;
            case DTStati.SEM_ROSSO:
                ImpostaColore(coloreRosso, "ACCESSO BLOCCATO");
                break;
            default:
                ImpostaColore(coloreGrigio, "SCONOSCIUTO");
                break;
        }
    }

    // ─── Logica visuale ───────────────────────────────────────────────────────

    private void ImpostaColore(Color colore, string etichetta)
    {
        if (_materialInstance != null)
        {
            _materialInstance.color = colore;

            // Aggiorna emissione HDR (funziona con URP + Emission abilitata sul materiale)
            if (_materialInstance.IsKeywordEnabled("_EMISSION"))
            {
                _materialInstance.SetColor(EmissionColor, colore * intensitaEmissione);
            }
        }

        if (testoStato != null)
        {
            testoStato.text  = etichetta;
            testoStato.color = colore;
        }
    }

    private void OnDestroy()
    {
        // Unity pulisce automaticamente le material instance create con .material
        // ma è buona prassi esplicitarlo
        if (_materialInstance != null)
            Destroy(_materialInstance);
    }
}
