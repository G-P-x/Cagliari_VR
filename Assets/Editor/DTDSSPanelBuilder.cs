// =============================================================================
// DTDSSPanelBuilder.cs — Editor Script pannello VR DSS con Meta Interaction SDK
// Assets/Editor/DTDSSPanelBuilder.cs
// =============================================================================

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using DigitalTwin;

public class DTDSSPanelBuilder : Editor
{
    private static readonly Color ColoreHeader     = new Color(0.10f, 0.13f, 0.20f, 1f);
    private static readonly Color ColoreSezione    = new Color(0.14f, 0.18f, 0.27f, 1f);
    private static readonly Color ColoreAccent     = new Color(0.22f, 0.60f, 0.95f, 1f);
    private static readonly Color ColoreBottone    = new Color(0.20f, 0.55f, 0.30f, 1f);
    private static readonly Color ColoreBottonePoll= new Color(0.25f, 0.40f, 0.70f, 1f);
    private static readonly Color ColoreSliderFill = new Color(0.22f, 0.60f, 0.95f, 1f);
    private static readonly Color ColoreTesto      = new Color(0.90f, 0.93f, 1.00f, 1f);
    private static readonly Color ColoreTestoLabel = new Color(0.65f, 0.72f, 0.88f, 1f);
    private static readonly Color ColoreSliderLim  = new Color(0.95f, 0.35f, 0.35f, 1f);

    [MenuItem("DigitalTwin/Crea Pannello DSS")]
    public static void CreaPannelloDSS()
    {
        DTConfig dtConfig = TrovaDTConfig();

        GameObject radice = new GameObject("DT_DSS_Panel");
        Undo.RegisterCreatedObjectUndo(radice, "Crea Pannello DSS");

        Canvas canvas = radice.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        CanvasScaler scaler = radice.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 10f;
        radice.AddComponent<OVRRaycaster>();

        RectTransform rtCanvas = radice.GetComponent<RectTransform>();
        rtCanvas.sizeDelta  = new Vector2(600, 860);
        rtCanvas.localScale = Vector3.one * 0.003f;
        radice.transform.position = new Vector3(0f, 1.5f, 2f);

        GarantisciOVREventSystem();

        // Sfondo
        CreaBox(radice, "Sfondo", ColoreHeader, new Vector2(600, 860), Vector2.zero);

        // Header
        GameObject header = CreaBox(radice, "Header", new Color(0.08f, 0.10f, 0.17f), new Vector2(600, 72), new Vector2(0, 394));
        CreaTesto(header, "Titolo", "DIGITAL TWIN — VR DSS", 22, ColoreTesto, new Vector2(0, 8), FontStyles.Bold);
        CreaTesto(header, "Sottotitolo", "Cagliari Smart Access Control", 10, ColoreAccent, new Vector2(0, -16));

        // Stato Varco
        float y = 300f;
        CreaLabel(radice, "LblStatoVarco", "STATO VARCO", y + 24);
        GameObject boxStato = CreaBox(radice, "BoxStatoVarco", ColoreSezione, new Vector2(560, 60), new Vector2(0, y));
        TextMeshProUGUI testoStatoVarco = CreaTesto(boxStato, "TestoStatoVarco", "— IN ATTESA —", 18, ColoreTesto, Vector2.zero, FontStyles.Bold);

        // Visitatori
        y = 195f;
        CreaLabel(radice, "LblVisitatori", "VISITATORI", y + 40);
        GameObject boxVis = CreaBox(radice, "BoxVisitatori", ColoreSezione, new Vector2(560, 84), new Vector2(0, y));
        TextMeshProUGUI testoVisitatori   = CreaRigaKV(boxVis, "RigaFisici", "Fisici (varco):", "—", new Vector2(0, 18));
        TextMeshProUGUI testoVisitatoriVR = CreaRigaKV(boxVis, "RigaVR",     "Virtuali (VR):", "—", new Vector2(0, -18));

        // Metriche
        y = 80f;
        CreaLabel(radice, "LblMetriche", "METRICHE STATISTICHE", y + 40);
        GameObject boxMet = CreaBox(radice, "BoxMetriche", ColoreSezione, new Vector2(560, 84), new Vector2(0, y));
        TextMeshProUGUI testoMedia = CreaRigaKV(boxMet, "RigaMedia", "Media ingressi/giorno:", "—", new Vector2(0, 18));
        TextMeshProUGUI testoStd   = CreaRigaKV(boxMet, "RigaStd",   "Deviazione standard:",  "—", new Vector2(0, -18));

        // Soglia Intermedia
        y = -55f;
        CreaLabel(radice, "LblSogliaInt", "SOGLIA INTERMEDIA  ●  LED GIALLO", y + 55);
        GameObject boxInt = CreaBox(radice, "BoxSogliaInt", ColoreSezione, new Vector2(560, 110), new Vector2(0, y));
        TextMeshProUGUI testoSogliaInt = CreaTesto(boxInt, "ValoreInt", "5", 28, ColoreAccent, new Vector2(230, 0), FontStyles.Bold);
        Slider sliderInt = CreaSlider(boxInt, "SliderIntermedia", 1, 100, 5, ColoreSliderFill, new Vector2(-25, 0), 440);

        // Soglia Limite
        y = -195f;
        CreaLabel(radice, "LblSogliaLim", "SOGLIA LIMITE  ●  LED ROSSO", y + 55);
        GameObject boxLim = CreaBox(radice, "BoxSogliaLim", ColoreSezione, new Vector2(560, 110), new Vector2(0, y));
        TextMeshProUGUI testoSogliaLim = CreaTesto(boxLim, "ValoreLim", "10", 28, ColoreSliderLim, new Vector2(230, 0), FontStyles.Bold);
        Slider sliderLim = CreaSlider(boxLim, "SliderLimite", 1, 100, 10, ColoreSliderLim, new Vector2(-25, 0), 440);

        // Pulsanti
        y = -335f;
        GameObject boxBtn = CreaBox(radice, "BoxBottoni", new Color(0.08f, 0.10f, 0.16f), new Vector2(560, 72), new Vector2(0, y));
        Button btnSoglie = CreaBottone(boxBtn, "BtnInviaSoglie", "APPLICA SOGLIE",   ColoreBottone,    new Vector2(-145, 0), new Vector2(250, 52));
        Button btnPoll   = CreaBottone(boxBtn, "BtnAggiorna",    "AGGIORNA DATI",    ColoreBottonePoll, new Vector2(145, 0), new Vector2(250, 52));

        // Barra connessione
        y = -408f;
        GameObject barraConn = CreaBox(radice, "BarraConnessione", new Color(0.10f, 0.14f, 0.22f), new Vector2(560, 28), new Vector2(0, y));
        CreaTesto(barraConn, "TestoConn", "⬤  Connessione al Digital Twin Server in corso...", 9, ColoreTestoLabel, Vector2.zero);

        // DigitalTwinManager
        GameObject dtManager = new GameObject("DigitalTwinManager");
        dtManager.transform.SetParent(radice.transform, false);
        DTWebSocketClient wsClient = dtManager.AddComponent<DTWebSocketClient>();
        if (dtConfig != null)
        {
            SerializedObject soWS = new SerializedObject(wsClient);
            soWS.FindProperty("dtConfig").objectReferenceValue = dtConfig;
            soWS.FindProperty("clientType").enumValueIndex = 1;
            soWS.ApplyModifiedProperties();
        }

        // DTDSSController — tutti i riferimenti cablati
        DTDSSController dssCtrl = radice.AddComponent<DTDSSController>();
        SerializedObject soDSS  = new SerializedObject(dssCtrl);
        soDSS.FindProperty("testoStatoVarco").objectReferenceValue         = testoStatoVarco;
        soDSS.FindProperty("testoVisitatori").objectReferenceValue         = testoVisitatori;
        soDSS.FindProperty("testoVisitatoriVR").objectReferenceValue       = testoVisitatoriVR;
        soDSS.FindProperty("testoMediaIngressi").objectReferenceValue      = testoMedia;
        soDSS.FindProperty("testoDeviazioneStandard").objectReferenceValue = testoStd;
        soDSS.FindProperty("sliderSogliaIntermedia").objectReferenceValue  = sliderInt;
        soDSS.FindProperty("sliderSogliaLimite").objectReferenceValue      = sliderLim;
        soDSS.FindProperty("testoSogliaIntermedia").objectReferenceValue   = testoSogliaInt;
        soDSS.FindProperty("testoSogliaLimite").objectReferenceValue       = testoSogliaLim;
        soDSS.FindProperty("btnInviaSoglie").objectReferenceValue          = btnSoglie;
        soDSS.ApplyModifiedProperties();

        UnityEditor.Events.UnityEventTools.AddPersistentListener(btnSoglie.onClick, dssCtrl.InviaSoglie);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btnPoll.onClick,   dssCtrl.PollMetriche);

        Selection.activeGameObject = radice;
        SceneView.FrameLastActiveSceneView();
        EditorUtility.SetDirty(radice);

        string configMsg = dtConfig != null
            ? $"DTConfig assegnato: '{dtConfig.name}'"
            : "⚠ DTConfig non trovato — assegnalo manualmente in DigitalTwinManager.";

        EditorUtility.DisplayDialog("Pannello DSS Creato ✓",
            $"Pannello generato con OVRRaycaster e OVRInputModule.\n\n{configMsg}", "OK");
    }

    // ─── EventSystem OVR ─────────────────────────────────────────────────────
    private static void GarantisciOVREventSystem()
    {
        EventSystem es = FindObjectOfType<EventSystem>();
        if (es == null)
        {
            GameObject goES = new GameObject("EventSystem (OVR)");
            Undo.RegisterCreatedObjectUndo(goES, "Crea OVR EventSystem");
            goES.AddComponent<EventSystem>();
            goES.AddComponent<OVRInputModule>();
        }
        else
        {
            StandaloneInputModule sim = es.GetComponent<StandaloneInputModule>();
            if (sim != null) { Undo.DestroyObjectImmediate(sim); Undo.AddComponent<OVRInputModule>(es.gameObject); }
            else if (es.GetComponent<OVRInputModule>() == null) Undo.AddComponent<OVRInputModule>(es.gameObject);
        }
    }

    private static DTConfig TrovaDTConfig()
    {
        string[] guids = AssetDatabase.FindAssets("t:DTConfig");
        if (guids.Length > 0) return AssetDatabase.LoadAssetAtPath<DTConfig>(AssetDatabase.GUIDToAssetPath(guids[0]));
        return null;
    }

    // =========================================================================
    // HELPERS UI
    // =========================================================================

    private static GameObject CreaBox(GameObject parent, string nome, Color colore, Vector2 size, Vector2 pos)
    {
        GameObject go = new GameObject(nome);
        go.transform.SetParent(parent.transform, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = size; rt.anchoredPosition = pos;
        go.AddComponent<Image>().color = colore;
        return go;
    }

    private static void CreaLabel(GameObject parent, string nome, string testo, float y)
    {
        GameObject go = new GameObject(nome);
        go.transform.SetParent(parent.transform, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(560, 20);
        rt.anchoredPosition = new Vector2(-10f, y);
        rt.pivot = new Vector2(0f, 0.5f);
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = testo; tmp.fontSize = 9;
        tmp.color = ColoreAccent; tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Left;
    }

    private static TextMeshProUGUI CreaTesto(GameObject parent, string nome, string contenuto,
        float size, Color colore, Vector2 pos, FontStyles stile = FontStyles.Normal)
    {
        GameObject go = new GameObject(nome);
        go.transform.SetParent(parent.transform, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(520, 40); rt.anchoredPosition = pos;
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = contenuto; tmp.fontSize = size;
        tmp.color = colore; tmp.fontStyle = stile;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.enableWordWrapping = false;
        return tmp;
    }

    private static TextMeshProUGUI CreaRigaKV(GameObject parent, string nome,
        string label, string valore, Vector2 pos)
    {
        GameObject riga = new GameObject(nome);
        riga.transform.SetParent(parent.transform, false);
        RectTransform rtR = riga.AddComponent<RectTransform>();
        rtR.sizeDelta = new Vector2(540, 28); rtR.anchoredPosition = pos;

        GameObject goL = new GameObject("Label");
        goL.transform.SetParent(riga.transform, false);
        RectTransform rtL = goL.AddComponent<RectTransform>();
        rtL.sizeDelta = new Vector2(320, 28); rtL.anchoredPosition = new Vector2(-100, 0);
        TextMeshProUGUI tmpL = goL.AddComponent<TextMeshProUGUI>();
        tmpL.text = label; tmpL.fontSize = 12; tmpL.color = ColoreTestoLabel;
        tmpL.alignment = TextAlignmentOptions.Left; tmpL.enableWordWrapping = false;

        GameObject goV = new GameObject("Valore");
        goV.transform.SetParent(riga.transform, false);
        RectTransform rtV = goV.AddComponent<RectTransform>();
        rtV.sizeDelta = new Vector2(160, 28); rtV.anchoredPosition = new Vector2(190, 0);
        TextMeshProUGUI tmpV = goV.AddComponent<TextMeshProUGUI>();
        tmpV.text = valore; tmpV.fontSize = 20; tmpV.color = ColoreTesto;
        tmpV.fontStyle = FontStyles.Bold; tmpV.alignment = TextAlignmentOptions.Right;
        tmpV.enableWordWrapping = false;
        return tmpV;
    }

    // ─── Slider orizzontale corretto ──────────────────────────────────────────
    private static Slider CreaSlider(GameObject parent, string nome,
        float min, float max, float value, Color coloreRiempimento, Vector2 pos, float larghezza)
    {
        // Contenitore Slider
        GameObject goS = new GameObject(nome);
        goS.transform.SetParent(parent.transform, false);
        RectTransform rtS = goS.AddComponent<RectTransform>();
        rtS.sizeDelta        = new Vector2(larghezza, 20);
        rtS.anchoredPosition = pos;

        Slider slider = goS.AddComponent<Slider>();
        slider.direction    = Slider.Direction.LeftToRight;
        slider.minValue     = min;
        slider.maxValue     = max;
        slider.value        = value;
        slider.wholeNumbers = true;

        // ── Background (stretch completo) ─────────────────────────────────────
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(goS.transform, false);
        RectTransform rtBG = bg.AddComponent<RectTransform>();
        rtBG.anchorMin       = Vector2.zero;
        rtBG.anchorMax       = Vector2.one;
        rtBG.sizeDelta       = Vector2.zero;
        rtBG.anchoredPosition= Vector2.zero;
        Image imgBG = bg.AddComponent<Image>();
        imgBG.color = new Color(0.08f, 0.10f, 0.16f, 1f);

        // ── Fill Area ─────────────────────────────────────────────────────────
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(goS.transform, false);
        RectTransform rtFA = fillArea.AddComponent<RectTransform>();
        rtFA.anchorMin        = new Vector2(0f, 0.25f);
        rtFA.anchorMax        = new Vector2(1f, 0.75f);
        rtFA.offsetMin        = new Vector2(5f, 0f);
        rtFA.offsetMax        = new Vector2(-5f, 0f);

        // ── Fill ──────────────────────────────────────────────────────────────
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        RectTransform rtFill = fill.AddComponent<RectTransform>();
        rtFill.anchorMin        = new Vector2(0f, 0f);
        rtFill.anchorMax        = new Vector2(0f, 1f);  // Unity Slider aggiorna anchorMax.x automaticamente
        rtFill.sizeDelta        = new Vector2(0f, 0f);
        rtFill.pivot            = new Vector2(0f, 0.5f);
        Image imgFill = fill.AddComponent<Image>();
        imgFill.color = coloreRiempimento;

        // ── Handle Slide Area ─────────────────────────────────────────────────
        GameObject handleArea = new GameObject("Handle Slide Area");
        handleArea.transform.SetParent(goS.transform, false);
        RectTransform rtHA = handleArea.AddComponent<RectTransform>();
        rtHA.anchorMin        = new Vector2(0f, 0f);
        rtHA.anchorMax        = new Vector2(1f, 1f);
        rtHA.offsetMin        = new Vector2(10f, 0f);
        rtHA.offsetMax        = new Vector2(-10f, 0f);

        // ── Handle ────────────────────────────────────────────────────────────
        GameObject handle = new GameObject("Handle");
        handle.transform.SetParent(handleArea.transform, false);
        RectTransform rtH = handle.AddComponent<RectTransform>();
        rtH.sizeDelta        = new Vector2(24f, 24f);
        rtH.anchoredPosition = Vector2.zero;
        rtH.anchorMin        = new Vector2(0f, 0.5f);
        rtH.anchorMax        = new Vector2(0f, 0.5f);
        rtH.pivot            = new Vector2(0.5f, 0.5f);
        Image imgHandle = handle.AddComponent<Image>();
        imgHandle.color = coloreRiempimento;

        // Collega i riferimenti al Slider
        slider.fillRect      = rtFill;
        slider.handleRect    = rtH;
        slider.targetGraphic = imgHandle;

        // Feedback visivo hover/press
        ColorBlock cb = slider.colors;
        cb.normalColor      = Color.white;
        cb.highlightedColor = coloreRiempimento * 1.4f;
        cb.pressedColor     = coloreRiempimento * 0.7f;
        cb.fadeDuration     = 0.08f;
        slider.colors = cb;

        return slider;
    }

    private static Button CreaBottone(GameObject parent, string nome, string etichetta,
        Color colore, Vector2 pos, Vector2 size)
    {
        GameObject go = CreaBox(parent, nome, colore, size, pos);
        Button btn = go.AddComponent<Button>();
        btn.targetGraphic = go.GetComponent<Image>();
        ColorBlock cb = btn.colors;
        cb.normalColor      = colore;
        cb.highlightedColor = colore * 1.35f;
        cb.pressedColor     = colore * 0.65f;
        cb.selectedColor    = colore * 1.15f;
        cb.fadeDuration     = 0.08f;
        btn.colors = cb;
        CreaTesto(go, "Label", etichetta, 13, Color.white, Vector2.zero, FontStyles.Bold);
        return btn;
    }
}
#endif
