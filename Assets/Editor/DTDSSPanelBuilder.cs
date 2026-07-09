// =============================================================================
// DTDSSPanelBuilder.cs — Pannello VR DSS
// Assets/Editor/DTDSSPanelBuilder.cs
// Menu: DigitalTwin → Crea Pannello DSS
//
// Layout:
//   • Pannello principale: metriche PUSH (stato_varco, visitatori, visitatori_vr)
//   • Pulsante toggle "STORICO"
//   • Pannello storico (nascosto di default): input date + risultati PULL
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
    // ─── Palette ─────────────────────────────────────────────────────────────
    private static readonly Color ColoreHeader      = new Color(0.10f, 0.13f, 0.20f, 1f);
    private static readonly Color ColoreSezione     = new Color(0.14f, 0.18f, 0.27f, 1f);
    private static readonly Color ColoreAccent      = new Color(0.22f, 0.60f, 0.95f, 1f);
    private static readonly Color ColoreVerde       = new Color(0.20f, 0.80f, 0.40f, 1f);
    private static readonly Color ColoreTesto       = new Color(0.90f, 0.93f, 1.00f, 1f);
    private static readonly Color ColoreTestoLabel  = new Color(0.65f, 0.72f, 0.88f, 1f);
    private static readonly Color ColoreBottoneOk   = new Color(0.20f, 0.55f, 0.30f, 1f);
    private static readonly Color ColoreBottoneInfo = new Color(0.25f, 0.40f, 0.70f, 1f);
    private static readonly Color ColoreStorico     = new Color(0.18f, 0.22f, 0.35f, 1f);
    private static readonly Color ColoreInput       = new Color(0.08f, 0.10f, 0.18f, 1f);

    [MenuItem("DigitalTwin/Crea Pannello DSS")]
    public static void CreaPannelloDSS()
    {
        DTConfig dtConfig = TrovaDTConfig();

        // ── Radice pannello ───────────────────────────────────────────────────
        GameObject radice = new GameObject("DT_DSS_Panel");
        Undo.RegisterCreatedObjectUndo(radice, "Crea Pannello DSS");

        Canvas canvas = radice.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        radice.AddComponent<CanvasScaler>().dynamicPixelsPerUnit = 10f;
        radice.AddComponent<OVRRaycaster>();

        RectTransform rt = radice.GetComponent<RectTransform>();
        rt.sizeDelta  = new Vector2(560, 700);
        rt.localScale = Vector3.one * 0.003f;
        radice.transform.position = new Vector3(0f, 1.6f, 2f);

        GarantisciOVREventSystem();
        CreaBox(radice, "Sfondo", ColoreHeader, new Vector2(560, 700), Vector2.zero);

        // ── Header ────────────────────────────────────────────────────────────
        GameObject header = CreaBox(radice, "Header",
            new Color(0.08f,0.10f,0.17f), new Vector2(560,64), new Vector2(0, 318));
        CreaTesto(header, "Titolo",      "DIGITAL TWIN — VR DSS",        20, ColoreTesto,  new Vector2(0, 8), FontStyles.Bold);
        CreaTesto(header, "Sottotitolo", "Cagliari Smart Access Control", 9,  ColoreAccent, new Vector2(0,-16));

        // ── Stato Varco ───────────────────────────────────────────────────────
        float y = 228f;
        CreaLabel(radice, "LblStato", "STATO VARCO", y + 22);
        GameObject boxStato = CreaBox(radice, "BoxStato", ColoreSezione, new Vector2(520,48), new Vector2(0,y));
        TextMeshProUGUI testoStatoVarco = CreaTesto(boxStato, "TestoStatoVarco",
            "— IN ATTESA —", 16, ColoreTesto, Vector2.zero, FontStyles.Bold);

        // ── Visitatori ────────────────────────────────────────────────────────
        y = 140f;
        CreaLabel(radice, "LblVis", "VISITATORI  —  REAL TIME", y + 38);
        GameObject boxVis = CreaBox(radice, "BoxVisitatori", ColoreSezione, new Vector2(520,80), new Vector2(0,y));
        TextMeshProUGUI testoVisitatori   = CreaRigaKV(boxVis, "RigaFisici", "Presenti nel sito ora:", "—", new Vector2(0, 20), ColoreVerde);
        TextMeshProUGUI testoVisitatoriVR = CreaRigaKV(boxVis, "RigaVR",     "Presenti in VR ora:",   "—", new Vector2(0,-20), ColoreAccent);

        // ── Toggle storico ────────────────────────────────────────────────────
        y = 40f;
        Button btnToggle = CreaBottone(radice, "BtnToggleStorico",
            "▼  STORICO  ON-DEMAND", ColoreBottoneInfo, new Vector2(0, y), new Vector2(520, 44));

        // ── Pannello Storico (nascosto di default) ────────────────────────────
        y = -140f;
        GameObject pannelloStorico = CreaBox(radice, "PannelloStorico",
            ColoreStorico, new Vector2(520, 300), new Vector2(0, y));
        pannelloStorico.SetActive(false);  // nascosto di default

        CreaLabel(pannelloStorico, "LblIntervallo", "INTERVALLO DATE  (formato YYYY-MM-DD)", 130);

        // Input Data Start
        TMP_InputField inputStart = CreaInputField(pannelloStorico, "InputDataStart",
            "Data inizio (es. 2026-05-29)", new Vector2(0, 90));

        // Input Data End
        TMP_InputField inputEnd = CreaInputField(pannelloStorico, "InputDataEnd",
            "Data fine (es. 2026-05-30)", new Vector2(0, 50));

        // Pulsante richiesta
        Button btnStorico = CreaBottone(pannelloStorico, "BtnRichiestaStorico",
            "CALCOLA STORICO", ColoreBottoneOk, new Vector2(0, 10), new Vector2(300, 40));

        // Risultati
        CreaLabel(pannelloStorico, "LblRisultati", "RISULTATI", -30);
        TextMeshProUGUI testoMedia          = CreaRigaKV(pannelloStorico, "RigaMedia",    "Media pesata giornaliera:", "—", new Vector2(0, -60), ColoreTesto);
        TextMeshProUGUI testoErroreStandard = CreaRigaKV(pannelloStorico, "RigaErrore",   "Errore standard:",          "—", new Vector2(0, -85), ColoreTesto);
        TextMeshProUGUI testoCampioni       = CreaRigaKV(pannelloStorico, "RigaCampioni", "Campioni (giorni):",        "—", new Vector2(0,-110), ColoreTestoLabel);

        // Stato / feedback
        GameObject boxStato2 = CreaBox(pannelloStorico, "BoxStatoStorico",
            new Color(0.08f,0.10f,0.18f), new Vector2(500, 28), new Vector2(0,-138));
        TextMeshProUGUI testoStatoStorico = CreaTesto(boxStato2, "TestoStatoStorico",
            "Inserisci le date e premi CALCOLA", 9, ColoreTestoLabel, Vector2.zero);

        // ── Toggle soglie ─────────────────────────────────────────────────────
        Button btnToggleSoglie = CreaBottone(radice, "BtnToggleSoglie",
            "▼  SOGLIE DI CONTROLLO",
            new Color(0.30f, 0.20f, 0.55f), new Vector2(0, -40f), new Vector2(520, 44));

        // ── Pannello Soglie (nascosto di default) ─────────────────────────────
        GameObject pannelloSoglie = CreaBox(radice, "PannelloSoglie",
            new Color(0.16f, 0.12f, 0.28f), new Vector2(520, 240), new Vector2(0, -210f));
        pannelloSoglie.SetActive(false);

        CreaLabel(pannelloSoglie, "LblSoglieTitle", "IMPOSTA SOGLIE SEMAFORO", 100);

        // Slider soglia intermedia
        CreaLabel(pannelloSoglie, "LblInt", "SOGLIA INTERMEDIA  ●  LED GIALLO", 68);
        TextMeshProUGUI testoValInt = CreaTesto(pannelloSoglie, "ValoreInt",
            "5", 20, new Color(0.95f, 0.75f, 0.10f), new Vector2(215, 40), FontStyles.Bold);
        Slider sliderInt = CreaSlider(pannelloSoglie, "SliderIntermedia",
            1, 500, 5, new Color(0.95f, 0.75f, 0.10f), new Vector2(-20, 40), 380);

        // Slider soglia limite
        CreaLabel(pannelloSoglie, "LblLim", "SOGLIA LIMITE  ●  LED ROSSO", -12);
        TextMeshProUGUI testoValLim = CreaTesto(pannelloSoglie, "ValoreLim",
            "10", 20, new Color(0.95f, 0.35f, 0.35f), new Vector2(215, -38), FontStyles.Bold);
        Slider sliderLim = CreaSlider(pannelloSoglie, "SliderLimite",
            1, 500, 10, new Color(0.95f, 0.35f, 0.35f), new Vector2(-20, -38), 380);

        // Pulsante imposta soglie
        Button btnImpostaSoglie = CreaBottone(pannelloSoglie, "BtnImpostaSoglie",
            "IMPOSTA SOGLIE", new Color(0.55f, 0.20f, 0.55f),
            new Vector2(0, -95), new Vector2(300, 42));

        // ── Barra connessione ─────────────────────────────────────────────────
        GameObject barraConn = CreaBox(radice, "BarraConn",
            new Color(0.10f,0.14f,0.22f), new Vector2(520,24), new Vector2(0,-326));
        CreaTesto(barraConn, "TestoConn",
            "⬤  Connessione al Digital Twin Server in corso...", 8, ColoreTestoLabel, Vector2.zero);

        // ── DigitalTwinManager ────────────────────────────────────────────────
        GameObject dtManager = new GameObject("DigitalTwinManager");
        dtManager.transform.SetParent(radice.transform, false);
        DTWebSocketClient wsClient = dtManager.AddComponent<DTWebSocketClient>();
        if (dtConfig != null)
        {
            SerializedObject soWS = new SerializedObject(wsClient);
            soWS.FindProperty("dtConfig").objectReferenceValue = dtConfig;
            soWS.FindProperty("clientType").enumValueIndex = 1; // VRDSS
            soWS.ApplyModifiedProperties();
        }

        // ── DTDSSController — riferimenti cablati ─────────────────────────────
        DTDSSController dssCtrl = radice.AddComponent<DTDSSController>();
        SerializedObject soDSS  = new SerializedObject(dssCtrl);

        soDSS.FindProperty("testoStatoVarco").objectReferenceValue      = testoStatoVarco;
        soDSS.FindProperty("testoVisitatori").objectReferenceValue      = testoVisitatori;
        soDSS.FindProperty("testoVisitatoriVR").objectReferenceValue    = testoVisitatoriVR;
        soDSS.FindProperty("pannelloStorico").objectReferenceValue      = pannelloStorico;
        soDSS.FindProperty("inputDataStart").objectReferenceValue       = inputStart;
        soDSS.FindProperty("inputDataEnd").objectReferenceValue         = inputEnd;
        soDSS.FindProperty("btnRichiestaStorico").objectReferenceValue  = btnStorico;
        soDSS.FindProperty("testoMedia").objectReferenceValue           = testoMedia;
        soDSS.FindProperty("testoErroreStandard").objectReferenceValue  = testoErroreStandard;
        soDSS.FindProperty("testoCampioni").objectReferenceValue        = testoCampioni;
        soDSS.FindProperty("testoStatoStorico").objectReferenceValue    = testoStatoStorico;
        soDSS.FindProperty("btnToggleStorico").objectReferenceValue      = btnToggle;
        soDSS.FindProperty("pannelloSoglie").objectReferenceValue         = pannelloSoglie;
        soDSS.FindProperty("sliderSogliaIntermedia").objectReferenceValue = sliderInt;
        soDSS.FindProperty("sliderSogliaLimite").objectReferenceValue     = sliderLim;
        soDSS.FindProperty("testoValoreIntermedia").objectReferenceValue  = testoValInt;
        soDSS.FindProperty("testoValoreLimite").objectReferenceValue      = testoValLim;
        soDSS.FindProperty("btnImpostaSoglie").objectReferenceValue       = btnImpostaSoglie;
        soDSS.FindProperty("btnToggleSoglie").objectReferenceValue        = btnToggleSoglie;
        soDSS.ApplyModifiedProperties();

        UnityEditor.Events.UnityEventTools.AddPersistentListener(btnToggle.onClick,        dssCtrl.TogglePannelloStorico);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btnStorico.onClick,       dssCtrl.RichiestaStorico);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btnToggleSoglie.onClick,  dssCtrl.TogglePannelloSoglie);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btnImpostaSoglie.onClick, dssCtrl.InviaSoglie);

        Selection.activeGameObject = radice;
        SceneView.FrameLastActiveSceneView();
        EditorUtility.SetDirty(radice);

        string cfgMsg = dtConfig != null ? $"DTConfig: '{dtConfig.name}'" : "⚠ DTConfig non trovato.";
        EditorUtility.DisplayDialog("Pannello DSS Creato ✓",
            $"Pannello generato.\n• PUSH: stato_varco, visitatori, visitatori_vr\n• PULL: storico on-demand\n\n{cfgMsg}", "OK");
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
            if (sim != null) { Undo.DestroyObjectImmediate(sim); Undo.AddComponent<OVRInputModule>(es.gameObject); }
            else if (es.GetComponent<OVRInputModule>() == null) Undo.AddComponent<OVRInputModule>(es.gameObject);
        }
    }

    private static DTConfig TrovaDTConfig()
    {
        string[] guids = AssetDatabase.FindAssets("t:DTConfig");
        if (guids.Length > 0)
            return AssetDatabase.LoadAssetAtPath<DTConfig>(AssetDatabase.GUIDToAssetPath(guids[0]));
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

    private static void CreaLabel(GameObject parent, string nome, string testo, float y)
    {
        GameObject go = new GameObject(nome);
        go.transform.SetParent(parent.transform, false);
        RectTransform r = go.AddComponent<RectTransform>();
        r.sizeDelta = new Vector2(500, 18);
        r.anchoredPosition = new Vector2(-10f, y);
        r.pivot = new Vector2(0f, 0.5f);
        r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
        TextMeshProUGUI t = go.AddComponent<TextMeshProUGUI>();
        t.text = testo; t.fontSize = 8;
        t.color = ColoreAccent; t.fontStyle = FontStyles.Bold;
        t.alignment = TextAlignmentOptions.Left;
    }

    private static TextMeshProUGUI CreaTesto(GameObject parent, string nome,
        string contenuto, float size, Color colore, Vector2 pos,
        FontStyles stile = FontStyles.Normal)
    {
        GameObject go = new GameObject(nome);
        go.transform.SetParent(parent.transform, false);
        RectTransform r = go.AddComponent<RectTransform>();
        r.sizeDelta = new Vector2(500, 36); r.anchoredPosition = pos;
        TextMeshProUGUI t = go.AddComponent<TextMeshProUGUI>();
        t.text = contenuto; t.fontSize = size; t.color = colore;
        t.fontStyle = stile; t.alignment = TextAlignmentOptions.Center;
        t.enableWordWrapping = false;
        return t;
    }

    private static TextMeshProUGUI CreaRigaKV(GameObject parent, string nome,
        string label, string valore, Vector2 pos, Color coloreValore)
    {
        GameObject riga = new GameObject(nome);
        riga.transform.SetParent(parent.transform, false);
        riga.AddComponent<RectTransform>().sizeDelta        = new Vector2(500, 22);
        riga.GetComponent<RectTransform>().anchoredPosition = pos;

        GameObject goL = new GameObject("Label");
        goL.transform.SetParent(riga.transform, false);
        goL.AddComponent<RectTransform>().sizeDelta        = new Vector2(280, 22);
        goL.GetComponent<RectTransform>().anchoredPosition = new Vector2(-100, 0);
        TextMeshProUGUI tmpL = goL.AddComponent<TextMeshProUGUI>();
        tmpL.text = label; tmpL.fontSize = 11; tmpL.color = ColoreTestoLabel;
        tmpL.alignment = TextAlignmentOptions.Left; tmpL.enableWordWrapping = false;

        GameObject goV = new GameObject("Valore");
        goV.transform.SetParent(riga.transform, false);
        goV.AddComponent<RectTransform>().sizeDelta        = new Vector2(160, 22);
        goV.GetComponent<RectTransform>().anchoredPosition = new Vector2(175, 0);
        TextMeshProUGUI tmpV = goV.AddComponent<TextMeshProUGUI>();
        tmpV.text = valore; tmpV.fontSize = 17; tmpV.color = coloreValore;
        tmpV.fontStyle = FontStyles.Bold; tmpV.alignment = TextAlignmentOptions.Right;
        tmpV.enableWordWrapping = false;
        return tmpV;
    }

    private static TMP_InputField CreaInputField(GameObject parent, string nome,
        string placeholder, Vector2 pos)
    {
        GameObject go = CreaBox(parent, nome, ColoreInput, new Vector2(460, 34), pos);
        TMP_InputField input = go.AddComponent<TMP_InputField>();

        // Text area
        GameObject textArea = new GameObject("Text Area");
        textArea.transform.SetParent(go.transform, false);
        RectTransform rtTA = textArea.AddComponent<RectTransform>();
        rtTA.anchorMin = Vector2.zero; rtTA.anchorMax = Vector2.one;
        rtTA.offsetMin = new Vector2(8, 2); rtTA.offsetMax = new Vector2(-8, -2);
        textArea.AddComponent<RectMask2D>();

        // Placeholder
        GameObject goPlaceholder = new GameObject("Placeholder");
        goPlaceholder.transform.SetParent(textArea.transform, false);
        RectTransform rtPH = goPlaceholder.AddComponent<RectTransform>();
        rtPH.anchorMin = Vector2.zero; rtPH.anchorMax = Vector2.one;
        rtPH.sizeDelta = Vector2.zero;
        TextMeshProUGUI tmpPH = goPlaceholder.AddComponent<TextMeshProUGUI>();
        tmpPH.text = placeholder; tmpPH.fontSize = 10;
        tmpPH.color = new Color(0.5f, 0.55f, 0.65f, 1f);
        tmpPH.fontStyle = FontStyles.Italic;
        tmpPH.alignment = TextAlignmentOptions.Left;

        // Text
        GameObject goText = new GameObject("Text");
        goText.transform.SetParent(textArea.transform, false);
        RectTransform rtTX = goText.AddComponent<RectTransform>();
        rtTX.anchorMin = Vector2.zero; rtTX.anchorMax = Vector2.one;
        rtTX.sizeDelta = Vector2.zero;
        TextMeshProUGUI tmpTX = goText.AddComponent<TextMeshProUGUI>();
        tmpTX.text = ""; tmpTX.fontSize = 11;
        tmpTX.color = ColoreTesto;
        tmpTX.alignment = TextAlignmentOptions.Left;

        input.textViewport  = rtTA;
        input.textComponent = tmpTX;
        input.placeholder   = tmpPH;
        input.targetGraphic = go.GetComponent<Image>();

        return input;
    }

    private static Slider CreaSlider(GameObject parent, string nome,
        float min, float max, float value, Color fill, Vector2 pos, float larghezza)
    {
        GameObject goS = new GameObject(nome);
        goS.transform.SetParent(parent.transform, false);
        RectTransform rtS = goS.AddComponent<RectTransform>();
        rtS.sizeDelta = new Vector2(larghezza, 18); rtS.anchoredPosition = pos;

        Slider slider = goS.AddComponent<Slider>();
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = min; slider.maxValue = max;
        slider.value = value; slider.wholeNumbers = true;

        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(goS.transform, false);
        RectTransform rtBG = bg.AddComponent<RectTransform>();
        rtBG.anchorMin = Vector2.zero; rtBG.anchorMax = Vector2.one;
        rtBG.sizeDelta = Vector2.zero;
        bg.AddComponent<Image>().color = new Color(0.08f, 0.10f, 0.16f, 1f);

        GameObject fa = new GameObject("Fill Area");
        fa.transform.SetParent(goS.transform, false);
        RectTransform rtFA = fa.AddComponent<RectTransform>();
        rtFA.anchorMin = new Vector2(0f, 0.25f); rtFA.anchorMax = new Vector2(1f, 0.75f);
        rtFA.offsetMin = new Vector2(5f, 0f); rtFA.offsetMax = new Vector2(-5f, 0f);

        GameObject fillGO = new GameObject("Fill");
        fillGO.transform.SetParent(fa.transform, false);
        RectTransform rtFill = fillGO.AddComponent<RectTransform>();
        rtFill.anchorMin = new Vector2(0f, 0f); rtFill.anchorMax = new Vector2(0f, 1f);
        rtFill.sizeDelta = Vector2.zero; rtFill.pivot = new Vector2(0f, 0.5f);
        fillGO.AddComponent<Image>().color = fill;

        GameObject ha = new GameObject("Handle Slide Area");
        ha.transform.SetParent(goS.transform, false);
        RectTransform rtHA = ha.AddComponent<RectTransform>();
        rtHA.anchorMin = Vector2.zero; rtHA.anchorMax = Vector2.one;
        rtHA.offsetMin = new Vector2(10f, 0f); rtHA.offsetMax = new Vector2(-10f, 0f);

        GameObject handle = new GameObject("Handle");
        handle.transform.SetParent(ha.transform, false);
        RectTransform rtH = handle.AddComponent<RectTransform>();
        rtH.sizeDelta = new Vector2(24f, 24f);
        rtH.anchorMin = new Vector2(0f, 0.5f); rtH.anchorMax = new Vector2(0f, 0.5f);
        rtH.pivot = new Vector2(0.5f, 0.5f);
        Image imgH = handle.AddComponent<Image>();
        imgH.color = fill;

        slider.fillRect = rtFill;
        slider.handleRect = rtH;
        slider.targetGraphic = imgH;

        ColorBlock cb = slider.colors;
        cb.normalColor = Color.white;
        cb.highlightedColor = fill * 1.4f;
        cb.pressedColor = fill * 0.7f;
        cb.fadeDuration = 0.08f;
        slider.colors = cb;
        return slider;
    }

    private static Button CreaBottone(GameObject parent, string nome,
        string etichetta, Color colore, Vector2 pos, Vector2 size)
    {
        GameObject go = CreaBox(parent, nome, colore, size, pos);
        Button btn = go.AddComponent<Button>();
        btn.targetGraphic = go.GetComponent<Image>();
        ColorBlock cb = btn.colors;
        cb.normalColor = colore; cb.highlightedColor = colore * 1.35f;
        cb.pressedColor = colore * 0.65f; cb.fadeDuration = 0.08f;
        btn.colors = cb;
        CreaTesto(go, "Label", etichetta, 12, Color.white, Vector2.zero, FontStyles.Bold);
        return btn;
    }
}
#endif
