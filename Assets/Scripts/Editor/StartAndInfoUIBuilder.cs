using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;

public class StartAndInfoUIBuilder
{
    [MenuItem("Tools/Build Start & Info UI Prefabs")]
    public static void BuildAll()
    {
        BuildStartCanvas();
        BuildDummyInfoGatheringCanvas();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[StartAndInfoUIBuilder] All prefabs built successfully!");
    }

    // ─── Helpers (same pattern as BattleUIBuilder) ───────────────────────

    static GameObject CreateUIObject(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        go.layer = 5;
        return go;
    }

    static void StretchFill(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    static void SetAnchors(RectTransform rt, Vector2 min, Vector2 max, Vector2 pivot)
    {
        rt.anchorMin = min;
        rt.anchorMax = max;
        rt.pivot = pivot;
    }

    static TextMeshProUGUI AddTMP(GameObject go, string text, int fontSize = 24,
        TextAlignmentOptions align = TextAlignmentOptions.Center, Color? color = null)
    {
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = align;
        tmp.color = color ?? Color.white;
        tmp.enableWordWrapping = true;
        tmp.overflowMode = TextOverflowModes.Ellipsis;
        return tmp;
    }

    static Button CreateButton(string name, Transform parent, string label, Vector2 size,
        Color bgColor = default)
    {
        var btnObj = CreateUIObject(name, parent);
        var rt = btnObj.GetComponent<RectTransform>();
        rt.sizeDelta = size;
        var img = btnObj.AddComponent<Image>();
        img.color = bgColor == default ? new Color(0.2f, 0.6f, 1f, 1f) : bgColor;
        var btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = img;

        var txtObj = CreateUIObject("Text", btnObj.transform);
        var txtRt = txtObj.GetComponent<RectTransform>();
        StretchFill(txtRt);
        AddTMP(txtObj, label, 24);

        return btn;
    }

    // ─── StartCanvas Prefab ─────────────────────────────────────────────

    static void BuildStartCanvas()
    {
        string prefabPath = "Assets/Prefabs/StartScene/StartCanvas.prefab";

        var root = new GameObject("StartCanvas");

        // Canvas child (matches pattern of other canvas prefabs)
        var canvasObj = new GameObject("Canvas");
        canvasObj.transform.SetParent(root.transform, false);
        canvasObj.layer = 5;
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();

        // EventSystem
        var esObj = new GameObject("EventSystem");
        esObj.transform.SetParent(canvasObj.transform, false);
        esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
        esObj.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

        // Background (stretch-fill, dark)
        var bgObj = CreateUIObject("Background", canvasObj.transform);
        StretchFill(bgObj.GetComponent<RectTransform>());
        bgObj.AddComponent<Image>().color = new Color(0.05f, 0.05f, 0.1f, 1f);

        // Title text (center of screen)
        var titleObj = CreateUIObject("TitleText", canvasObj.transform);
        var titleRt = titleObj.GetComponent<RectTransform>();
        SetAnchors(titleRt, new Vector2(0.05f, 0.45f), new Vector2(0.95f, 0.7f), new Vector2(0.5f, 0.5f));
        titleRt.offsetMin = Vector2.zero;
        titleRt.offsetMax = Vector2.zero;
        var titleTmp = AddTMP(titleObj, "World Domination\nBy Silver Tongue", 52, TextAlignmentOptions.Center, Color.yellow);
        titleTmp.fontStyle = FontStyles.Bold;

        // Start button (below title, centered)
        var startBtn = CreateButton("StartButton", canvasObj.transform, "START", new Vector2(250, 60),
            new Color(0.2f, 0.6f, 1f));
        var startBtnRt = startBtn.GetComponent<RectTransform>();
        SetAnchors(startBtnRt, new Vector2(0.5f, 0.25f), new Vector2(0.5f, 0.25f), new Vector2(0.5f, 0.5f));
        startBtnRt.sizeDelta = new Vector2(250, 60);

        // Add StartView component to root
        root.AddComponent<SilverTongue.StartScene.StartView>();

        PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        Object.DestroyImmediate(root);
        Debug.Log($"[StartAndInfoUIBuilder] Created: {prefabPath}");
    }

    // ─── DummyInfoGatheringCanvas Prefab ────────────────────────────────

    static void BuildDummyInfoGatheringCanvas()
    {
        string prefabPath = "Assets/Prefabs/DummyInfoGatheringScene/DummyInfoGatheringCanvas.prefab";

        var root = new GameObject("DummyInfoGatheringCanvas");

        // Canvas child
        var canvasObj = new GameObject("Canvas");
        canvasObj.transform.SetParent(root.transform, false);
        canvasObj.layer = 5;
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();

        // EventSystem
        var esObj = new GameObject("EventSystem");
        esObj.transform.SetParent(canvasObj.transform, false);
        esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
        esObj.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

        // Background (stretch-fill, dark)
        var bgObj = CreateUIObject("Background", canvasObj.transform);
        StretchFill(bgObj.GetComponent<RectTransform>());
        bgObj.AddComponent<Image>().color = new Color(0.05f, 0.05f, 0.1f, 1f);

        // Title text (top area)
        var titleObj = CreateUIObject("TitleText", canvasObj.transform);
        var titleRt = titleObj.GetComponent<RectTransform>();
        SetAnchors(titleRt, new Vector2(0.1f, 0.8f), new Vector2(0.9f, 0.95f), new Vector2(0.5f, 0.5f));
        titleRt.offsetMin = Vector2.zero;
        titleRt.offsetMax = Vector2.zero;
        var titleTmp = AddTMP(titleObj, "Info Gathering Phase", 40, TextAlignmentOptions.Center, Color.yellow);
        titleTmp.fontStyle = FontStyles.Bold;

        // Subtitle (brief description)
        var subtitleObj = CreateUIObject("SubtitleText", canvasObj.transform);
        var subtitleRt = subtitleObj.GetComponent<RectTransform>();
        SetAnchors(subtitleRt, new Vector2(0.1f, 0.72f), new Vector2(0.9f, 0.8f), new Vector2(0.5f, 0.5f));
        subtitleRt.offsetMin = Vector2.zero;
        subtitleRt.offsetMax = Vector2.zero;
        AddTMP(subtitleObj, "(Exploration not implemented - items auto-collected)", 20,
            TextAlignmentOptions.Center, new Color(0.6f, 0.6f, 0.6f));

        // Item list text (middle area, left-aligned)
        var itemListObj = CreateUIObject("ItemListText", canvasObj.transform);
        var itemListRt = itemListObj.GetComponent<RectTransform>();
        SetAnchors(itemListRt, new Vector2(0.15f, 0.2f), new Vector2(0.85f, 0.7f), new Vector2(0.5f, 1f));
        itemListRt.offsetMin = Vector2.zero;
        itemListRt.offsetMax = Vector2.zero;
        AddTMP(itemListObj, "Items collected:\n  - Loading...", 22,
            TextAlignmentOptions.TopLeft, Color.white);

        // Proceed button (bottom center)
        var proceedBtn = CreateButton("ProceedButton", canvasObj.transform, "PROCEED TO BATTLE", new Vector2(300, 60),
            new Color(0.8f, 0.3f, 0.1f));
        var proceedBtnRt = proceedBtn.GetComponent<RectTransform>();
        SetAnchors(proceedBtnRt, new Vector2(0.5f, 0.08f), new Vector2(0.5f, 0.08f), new Vector2(0.5f, 0.5f));
        proceedBtnRt.sizeDelta = new Vector2(300, 60);

        // Add View component to root
        root.AddComponent<SilverTongue.DummyInfoGatheringScene.DummyInfoGatheringView>();

        PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        Object.DestroyImmediate(root);
        Debug.Log($"[StartAndInfoUIBuilder] Created: {prefabPath}");
    }
}
