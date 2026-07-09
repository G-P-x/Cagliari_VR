// =============================================================================
// DTGuestPanelBuilder.cs — Pannello VR Guest
// Assets/Editor/DTGuestPanelBuilder.cs
// Menu: DigitalTwin → Crea Pannello VR Guest
//
// Genera il pannello UI per la scena VR_GUEST_Home con:
//   • Button "VARCO PRINCIPALE — {stato_varco}" aggiornato dal server
//   • Label azione contestuale (accedi / esci)
//   • Canvas World Space + OVRRaycaster per Meta Quest 3
// =============================================================================

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using DigitalTwin;

public class DTGuestPanelBuilder : Editor
{
    // ─── Palette ─────────────────────────────────────────────────────────────
    private static readonly Color ColoreHeader    = new Color(0.08f, 0.11f, 0.18f, 0.95f);
    private static readonly Color ColoreBottone   = new Color(0.15f, 0.60f, 0.25f, 1f);
    private static readonly Color ColoreAccent    = new Color(0.22f, 0.60f, 0.95f, 1f);
    private static readonly Color ColoreTesto     = new Color(0.92f, 0.95f, 1.00f, 1f);
    private static readonly Color ColoreSubtitle  = new Color(0.65f, 0.72f, 0.88f, 1f);

    [MenuItem("DigitalTwin/Crea Pannello VR Guest")]
    public static void CreaPannelloGuest()
    {
        DTConfig dtConfig = TrovaDTConfig();

        // ── Radice pannello ───────────────────────────────────────────────────
        GameObject radice = new GameObject("DT_Guest_Panel");
        Undo.RegisterCreatedObjectUndo(radice, "Crea Pannello VR Guest");

        Canvas canvas = radice.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        radice.AddComponent<CanvasScaler>().dynamicPixelsPerUnit = 10f;
        radice.AddComponent<OVRRaycaster>();

        // Posizionamento in scena: davanti all'utente a ~2m
        RectTransform rt = radice.GetComponent<RectTransform>();
        rt.sizeDelta  = new Vector2(480, 320);
        rt.localScale = Vector3.one * 0.003f;
        radice.transform.position = new Vector3(0f, 1.4f, 2f);

        GarantisciOVREventSystem();

        // ── Sfondo pannello ───────────────────────────────────────────────────
        CreaBox(radice, "Sfondo", ColoreHeader, new Vector2(480, 320), Vector2.zero);

        // ── Riga decorativa in cima ────────────────────────────────────────────
        CreaBox(radice, "Striscia", ColoreAccent, new Vector2(480, 4), new Vector2(0, 148));

        // ── Titolo fisso ──────────────────────────────────────────────────────
        CreaTesto(radice, "LabelSito",
            "SISTEMA DI MONITORAGGIO ACCESSI",
            8, ColoreSubtitle, new Vector2(0, 126), FontStyles.Normal);

        // ── BUTTON VARCO PRINCIPALE ───────────────────────────────────────────
        // Occupa la metà superiore — grande e leggibile in VR
        GameObject btnGo = CreaBox(radice, "BtnVarco",
            ColoreBottone, new Vector2(440, 140), new Vector2(0, 40));

        Button btn = btnGo.AddComponent<Button>();
        btn.targetGraphic = btnGo.GetComponent<Image>();

        ColorBlock cb = btn.colors;
        cb.normalColor      = ColoreBottone;
        cb.highlightedColor = ColoreBottone * 1.4f;
        cb.pressedColor     = ColoreBottone * 0.65f;
        cb.disabledColor    = new Color(0.35f, 0.15f, 0.15f, 1f);
        cb.fadeDuration     = 0.08f;
        btn.colors = cb;

        // Testo principale del button: stato varco
        TextMeshProUGUI testoStatoVarco = CreaTesto(btnGo, "TestoStatoVarco",
            "VARCO PRINCIPALE\n✓ OPERATIVO",
            20, ColoreTesto, new Vector2(0, 10), FontStyles.Bold);
        testoStatoVarco.alignment    = TextAlignmentOptions.Center;
        testoStatoVarco.lineSpacing  = 12f;

        // ── Label azione ──────────────────────────────────────────────────────
        TextMeshProUGUI testoAzione = CreaTesto(radice, "TestoAzione",
            "Tocca per accedere all'area",
            11, ColoreAccent, new Vector2(0, -65), FontStyles.Italic);

        // ── Barra connessione ─────────────────────────────────────────────────
        GameObject barraConn = CreaBox(radice, "BarraConn",
            new Color(0.06f, 0.08f, 0.14f, 1f), new Vector2(480, 28), new Vector2(0, -140));
        CreaTesto(barraConn, "TestoConn",
            "⬤  Connessione al Digital Twin Server in corso...",
            8, ColoreSubtitle, Vector2.zero);

        // ── DigitalTwinManager ────────────────────────────────────────────────
        GameObject dtManager = new GameObject("DigitalTwinManager");
        dtManager.transform.SetParent(radice.transform, false);
        DTWebSocketClient wsClient = dtManager.AddComponent<DTWebSocketClient>();

        if (dtConfig != null)
        {
            SerializedObject soWS = new SerializedObject(wsClient);
            soWS.FindProperty("dtConfig").objectReferenceValue    = dtConfig;
            soWS.FindProperty("clientType").enumValueIndex        = 0;  // VRGuest
            soWS.ApplyModifiedProperties();
        }

        // ── DTGuestController — riferimenti cablati ───────────────────────────
        DTGuestController guestCtrl = radice.AddComponent<DTGuestController>();
        SerializedObject soGuest    = new SerializedObject(guestCtrl);

        soGuest.FindProperty("dtConfig").objectReferenceValue         = dtConfig;
        soGuest.FindProperty("btnVarco").objectReferenceValue         = btn;
        soGuest.FindProperty("testoStatoVarco").objectReferenceValue  = testoStatoVarco;
        soGuest.FindProperty("testoAzione").objectReferenceValue      = testoAzione;
        soGuest.FindProperty("sfondoBottone").objectReferenceValue    = btnGo.GetComponent<Image>();
        soGuest.ApplyModifiedProperties();

        // Collega click del button a OnVarcoButtonClicked
        UnityEditor.Events.UnityEventTools.AddPersistentListener(
            btn.onClick, guestCtrl.OnVarcoButtonClicked);

        Selection.activeGameObject = radice;
        SceneView.FrameLastActiveSceneView();
        EditorUtility.SetDirty(radice);

        string cfgMsg = dtConfig != null
            ? $"DTConfig: '{dtConfig.name}'"
            : "⚠ DTConfig non trovato — assegnalo manualmente.";

        EditorUtility.DisplayDialog("Pannello VR Guest Creato ✓",
            $"Pannello generato nella scena.\n\n" +
            $"• Button: VARCO PRINCIPALE — stato aggiornato dal server\n" +
            $"• Click: alterna INGRESSO / USCITA VR\n\n{cfgMsg}", "OK");
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────
    private static void GarantisciOVREventSystem()
    {
        EventSystem es = FindObjectOfType<EventSystem>();
        if (es == null)
        {
            GameObject g = new GameObject("EventSystem (OVR)");
            Undo.RegisterCreatedObjectUndo(g, "Crea OVR EventSystem");
            g.AddComponent<EventSystem>();
            g.AddComponent<OVRInputModule>();
        }
        else
        {
            StandaloneInputModule sim = es.GetComponent<StandaloneInputModule>();
            if (sim != null)
            {
                Undo.DestroyObjectImmediate(sim);
                Undo.AddComponent<OVRInputModule>(es.gameObject);
            }
            else if (es.GetComponent<OVRInputModule>() == null)
                Undo.AddComponent<OVRInputModule>(es.gameObject);
        }
    }

    private static DTConfig TrovaDTConfig()
    {
        string[] guids = AssetDatabase.FindAssets("t:DTConfig");
        if (guids.Length > 0)
            return AssetDatabase.LoadAssetAtPath<DTConfig>(
                AssetDatabase.GUIDToAssetPath(guids[0]));
        return null;
    }

    private static GameObject CreaBox(GameObject parent, string nome,
        Color colore, Vector2 size, Vector2 pos)
    {
        GameObject go = new GameObject(nome);
        go.transform.SetParent(parent.transform, false);
        RectTransform r = go.AddComponent<RectTransform>();
        r.sizeDelta = size; r.anchoredPosition = pos;
        go.AddComponent<Image>().color = colore;
        return go;
    }

    private static TextMeshProUGUI CreaTesto(GameObject parent, string nome,
        string contenuto, float size, Color colore, Vector2 pos,
        FontStyles stile = FontStyles.Normal)
    {
        GameObject go = new GameObject(nome);
        go.transform.SetParent(parent.transform, false);
        RectTransform r = go.AddComponent<RectTransform>();
        r.sizeDelta = new Vector2(460, 100); r.anchoredPosition = pos;
        TextMeshProUGUI t = go.AddComponent<TextMeshProUGUI>();
        t.text = contenuto; t.fontSize = size;
        t.color = colore; t.fontStyle = stile;
        t.alignment = TextAlignmentOptions.Center;
        t.enableWordWrapping = false;
        return t;
    }
}
#endif
