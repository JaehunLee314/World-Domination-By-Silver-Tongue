using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;

public class BattleUIBuilder
{
    // Default resources for DefaultControls
    static DefaultControls.Resources s_defaultRes;

    [MenuItem("Tools/Build Battle UI Prefabs")]
    public static void BuildAll()
    {
        // Cache default resources (built-in sprites)
        s_defaultRes = new DefaultControls.Resources();

        BuildCharacterCardPrefab();
        BuildInventoryItemPrefab();
        BuildBattlerSelectingCanvas();
        BuildStrategySelectingCanvas();
        BuildBattleCanvas();
        BuildBattleResultCanvas();
        BuildChatBubblePrefab();
        BuildConversationHistoryCanvas();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[BattleUIBuilder] All prefabs built successfully!");
    }

    // ─── Helper: create a UI child with RectTransform ────────────────────
    static GameObject CreateUIObject(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        go.layer = 5; // UI layer
        return go;
    }

    // ─── Helper: create a proper ScrollView using Unity's DefaultControls ──
    static (ScrollRect scrollRect, RectTransform content) CreateScrollView(string name, Transform parent, bool horizontal, bool vertical)
    {
        var scrollGo = DefaultControls.CreateScrollView(s_defaultRes);
        scrollGo.name = name;
        scrollGo.transform.SetParent(parent, false);

        // Set all children to UI layer
        foreach (var t in scrollGo.GetComponentsInChildren<Transform>(true))
            t.gameObject.layer = 5;

        var scrollRect = scrollGo.GetComponent<ScrollRect>();
        scrollRect.horizontal = horizontal;
        scrollRect.vertical = vertical;

        // Hide scrollbars we don't need
        if (!horizontal)
        {
            var hBar = scrollGo.transform.Find("Scrollbar Horizontal");
            if (hBar != null) Object.DestroyImmediate(hBar.gameObject);
            scrollRect.horizontalScrollbar = null;
        }
        if (!vertical)
        {
            var vBar = scrollGo.transform.Find("Scrollbar Vertical");
            if (vBar != null) Object.DestroyImmediate(vBar.gameObject);
            scrollRect.verticalScrollbar = null;
        }

        var content = scrollRect.content;
        return (scrollRect, content);
    }

    // ─── Helper: clear all children under a canvas (for rebuilds) ────────
    static void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Object.DestroyImmediate(parent.GetChild(i).gameObject);
        }
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
        AddTMP(txtObj, label, 20);

        return btn;
    }

    // ─── CharacterCard Prefab ────────────────────────────────────────────
    static void BuildCharacterCardPrefab()
    {
        var root = CreateUIObject("CharacterCard", null);
        var rootRt = root.GetComponent<RectTransform>();
        rootRt.sizeDelta = new Vector2(420, 900);

        // Background
        var bg = root.AddComponent<Image>();
        bg.color = new Color(0.15f, 0.15f, 0.2f, 0.95f);

        // Layout element so parent HorizontalLayoutGroup respects our size
        var le = root.AddComponent<LayoutElement>();
        le.preferredWidth = 420;
        le.preferredHeight = 900;
        le.minWidth = 420;
        le.minHeight = 900;

        // Vertical layout
        var vlg = root.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(15, 15, 15, 15);
        vlg.spacing = 10;
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        // Profile image
        var profileObj = CreateUIObject("ProfileImage", root.transform);
        var profileRt = profileObj.GetComponent<RectTransform>();
        profileRt.sizeDelta = new Vector2(390, 390);
        var profileImg = profileObj.AddComponent<Image>();
        profileImg.color = new Color(0.4f, 0.4f, 0.5f, 1f);

        // Name
        var nameObj = CreateUIObject("NameText", root.transform);
        nameObj.GetComponent<RectTransform>().sizeDelta = new Vector2(260, 30);
        AddTMP(nameObj, "Character Name", 22, TextAlignmentOptions.Center, Color.yellow);

        // Personality
        var persObj = CreateUIObject("PersonalityText", root.transform);
        persObj.GetComponent<RectTransform>().sizeDelta = new Vector2(260, 25);
        AddTMP(persObj, "Personality", 16, TextAlignmentOptions.Center, new Color(0.8f, 0.8f, 0.8f));

        // Intelligence
        var intObj = CreateUIObject("IntelligenceText", root.transform);
        intObj.GetComponent<RectTransform>().sizeDelta = new Vector2(260, 25);
        AddTMP(intObj, "INT: 5", 16, TextAlignmentOptions.Center, new Color(0.5f, 1f, 0.5f));

        // Skills
        var skillsObj = CreateUIObject("SkillsText", root.transform);
        skillsObj.GetComponent<RectTransform>().sizeDelta = new Vector2(390, 100);
        AddTMP(skillsObj, "- Skill 1: Description\n- Skill 2: Description", 14,
            TextAlignmentOptions.TopLeft, new Color(0.6f, 0.8f, 1f));

        // Lose Conditions
        var condObj = CreateUIObject("LoseConditionsText", root.transform);
        condObj.GetComponent<RectTransform>().sizeDelta = new Vector2(390, 120);
        AddTMP(condObj, "- Condition 1\n- Condition 2\n- Condition 3", 14,
            TextAlignmentOptions.TopLeft, new Color(1f, 0.6f, 0.6f));

        // Select Button
        var selectBtn = CreateButton("SelectButton", root.transform, "SELECT", new Vector2(390, 50),
            new Color(0.2f, 0.7f, 0.3f));
        var selectBtnRt = selectBtn.GetComponent<RectTransform>();
        selectBtnRt.sizeDelta = new Vector2(390, 50);

        // Add the CharacterCardUI component
        root.AddComponent<SilverTongue.BattleScene.CharacterCardUI>();

        // Save as prefab
        string path = "Assets/Prefabs/BattleScene/UI/CharacterCard.prefab";
        PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        Debug.Log($"[BattleUIBuilder] Created: {path}");
    }

    // ─── InventoryItem Prefab ────────────────────────────────────────────
    static void BuildInventoryItemPrefab()
    {
        var root = CreateUIObject("InventoryItem", null);
        var rootRt = root.GetComponent<RectTransform>();
        rootRt.sizeDelta = new Vector2(100, 130);

        var bg = root.AddComponent<Image>();
        bg.color = new Color(0.2f, 0.2f, 0.25f, 0.9f);

        var invLe = root.AddComponent<LayoutElement>();
        invLe.preferredWidth = 100;
        invLe.preferredHeight = 130;

        var vlg = root.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(5, 5, 5, 5);
        vlg.spacing = 3;
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        // Item image
        var imgObj = CreateUIObject("ItemImage", root.transform);
        imgObj.GetComponent<RectTransform>().sizeDelta = new Vector2(80, 80);
        var img = imgObj.AddComponent<Image>();
        img.color = new Color(0.5f, 0.5f, 0.6f, 1f);

        // Item name
        var nameObj = CreateUIObject("ItemNameText", root.transform);
        nameObj.GetComponent<RectTransform>().sizeDelta = new Vector2(90, 30);
        AddTMP(nameObj, "Item", 14, TextAlignmentOptions.Center);

        root.AddComponent<CanvasGroup>();
        root.AddComponent<SilverTongue.BattleScene.DraggableItem>();
        root.AddComponent<SilverTongue.BattleScene.InventoryItemUI>();

        string path = "Assets/Prefabs/BattleScene/UI/InventoryItem.prefab";
        PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        Debug.Log($"[BattleUIBuilder] Created: {path}");
    }

    // ─── BattlerSelectingCanvas ──────────────────────────────────────────
    static void BuildBattlerSelectingCanvas()
    {
        string prefabPath = "Assets/Prefabs/BattleScene/BattlerSelectingCanvas.prefab";
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
        var canvas = instance.transform.Find("Canvas");
        ClearChildren(canvas);
        var canvasRt = canvas.GetComponent<RectTransform>();

        // -- Background panel
        var bgObj = CreateUIObject("Background", canvas);
        var bgRt = bgObj.GetComponent<RectTransform>();
        StretchFill(bgRt);
        var bgImg = bgObj.AddComponent<Image>();
        bgImg.color = new Color(0.08f, 0.08f, 0.12f, 1f);

        // -- Title
        var titleObj = CreateUIObject("TitleText", canvas);
        var titleRt = titleObj.GetComponent<RectTransform>();
        SetAnchors(titleRt, new Vector2(0, 0.9f), new Vector2(1, 1), new Vector2(0.5f, 1));
        titleRt.offsetMin = new Vector2(0, 0);
        titleRt.offsetMax = new Vector2(0, 0);
        AddTMP(titleObj, "SELECT YOUR BATTLER", 36, TextAlignmentOptions.Center, Color.yellow);

        // -- ScrollView (using DefaultControls for proper setup)
        var (scrollRect, contentRt) = CreateScrollView("ScrollView", canvas, horizontal: true, vertical: false);
        var scrollRt = scrollRect.GetComponent<RectTransform>();
        SetAnchors(scrollRt, new Vector2(0.02f, 0.05f), new Vector2(0.98f, 0.88f), new Vector2(0.5f, 0.5f));
        scrollRt.offsetMin = Vector2.zero;
        scrollRt.offsetMax = Vector2.zero;
        scrollRect.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.15f, 0.5f);

        // Configure Content for horizontal card layout
        var hlg = contentRt.gameObject.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 20;
        hlg.padding = new RectOffset(20, 20, 20, 20);
        hlg.childAlignment = TextAnchor.UpperLeft;
        hlg.childControlWidth = false;
        hlg.childControlHeight = false;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;
        var csf = contentRt.gameObject.AddComponent<ContentSizeFitter>();
        csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        csf.verticalFit = ContentSizeFitter.FitMode.Unconstrained;

        // -- Confirmation Popup
        var popupOverlay = CreateUIObject("ConfirmationPopup", canvas);
        var popupOverlayRt = popupOverlay.GetComponent<RectTransform>();
        StretchFill(popupOverlayRt);
        var overlayImg = popupOverlay.AddComponent<Image>();
        overlayImg.color = new Color(0, 0, 0, 0.7f);
        popupOverlay.SetActive(false);

        var popupPanel = CreateUIObject("PopupPanel", popupOverlay.transform);
        var popupRt = popupPanel.GetComponent<RectTransform>();
        popupRt.anchorMin = new Vector2(0.25f, 0.2f);
        popupRt.anchorMax = new Vector2(0.75f, 0.8f);
        popupRt.offsetMin = Vector2.zero;
        popupRt.offsetMax = Vector2.zero;
        var panelImg = popupPanel.AddComponent<Image>();
        panelImg.color = new Color(0.15f, 0.15f, 0.2f, 1f);

        var popupVlg = popupPanel.AddComponent<VerticalLayoutGroup>();
        popupVlg.padding = new RectOffset(20, 20, 20, 20);
        popupVlg.spacing = 15;
        popupVlg.childAlignment = TextAnchor.MiddleCenter;
        popupVlg.childControlWidth = false;
        popupVlg.childControlHeight = false;
        popupVlg.childForceExpandWidth = false;
        popupVlg.childForceExpandHeight = false;

        // Popup image
        var popImgObj = CreateUIObject("PopupImage", popupPanel.transform);
        popImgObj.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 400);
        var popImg = popImgObj.AddComponent<Image>();
        popImg.color = new Color(0.4f, 0.4f, 0.5f, 1f);

        // Popup name
        var popNameObj = CreateUIObject("PopupNameText", popupPanel.transform);
        popNameObj.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 40);
        AddTMP(popNameObj, "Select Character?", 28);

        // Button row
        var btnRow = CreateUIObject("ButtonRow", popupPanel.transform);
        btnRow.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 50);
        var btnHlg = btnRow.AddComponent<HorizontalLayoutGroup>();
        btnHlg.spacing = 20;
        btnHlg.childAlignment = TextAnchor.MiddleCenter;
        btnHlg.childControlWidth = false;
        btnHlg.childControlHeight = false;
        btnHlg.childForceExpandWidth = false;
        btnHlg.childForceExpandHeight = false;

        CreateButton("YesButton", btnRow.transform, "YES", new Vector2(120, 45), new Color(0.2f, 0.7f, 0.3f));
        CreateButton("NoButton", btnRow.transform, "NO", new Vector2(120, 45), new Color(0.7f, 0.2f, 0.2f));

        // Ensure exactly one BattlerSelectingView component on the root
        foreach (var old in instance.GetComponents<SilverTongue.BattleScene.BattlerSelectingView>())
            Object.DestroyImmediate(old);
        instance.AddComponent<SilverTongue.BattleScene.BattlerSelectingView>();

        PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
        Object.DestroyImmediate(instance);
        Debug.Log($"[BattleUIBuilder] Updated: {prefabPath}");
    }

    // ─── StrategySelectingCanvas ─────────────────────────────────────────
    static void BuildStrategySelectingCanvas()
    {
        string prefabPath = "Assets/Prefabs/BattleScene/StrategySelectingCanvas.prefab";
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
        var canvas = instance.transform.Find("Canvas");
        ClearChildren(canvas);

        // Background
        var bgObj = CreateUIObject("Background", canvas);
        StretchFill(bgObj.GetComponent<RectTransform>());
        bgObj.AddComponent<Image>().color = new Color(0.08f, 0.08f, 0.12f, 1f);

        // === TOP BAR (top 8%) ===
        var topBar = CreateUIObject("TopBar", canvas);
        var topBarRt = topBar.GetComponent<RectTransform>();
        SetAnchors(topBarRt, new Vector2(0, 0.92f), Vector2.one, new Vector2(0.5f, 1));
        topBarRt.offsetMin = Vector2.zero;
        topBarRt.offsetMax = Vector2.zero;
        topBar.AddComponent<Image>().color = new Color(0.12f, 0.12f, 0.18f, 1f);

        // Turn counter (center)
        var turnObj = CreateUIObject("TurnCounterText", topBar.transform);
        var turnRt = turnObj.GetComponent<RectTransform>();
        StretchFill(turnRt);
        AddTMP(turnObj, "Turn 1/7", 28);

        // Back button (right)
        var backBtn = CreateButton("BackButton", topBar.transform, "BACK", new Vector2(100, 40),
            new Color(0.7f, 0.3f, 0.3f));
        var backRt = backBtn.GetComponent<RectTransform>();
        SetAnchors(backRt, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f));
        backRt.anchoredPosition = new Vector2(-15, 0);
        backRt.sizeDelta = new Vector2(100, 40);

        // Log button (left)
        var logBtn = CreateButton("LogButton", topBar.transform, "LOG", new Vector2(80, 40),
            new Color(0.3f, 0.3f, 0.5f));
        var logBtnRt = logBtn.GetComponent<RectTransform>();
        SetAnchors(logBtnRt, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f));
        logBtnRt.anchoredPosition = new Vector2(15, 0);
        logBtnRt.sizeDelta = new Vector2(80, 40);

        // === MIDDLE SECTION (50%) ===
        var midSection = CreateUIObject("MiddleSection", canvas);
        var midRt = midSection.GetComponent<RectTransform>();
        SetAnchors(midRt, new Vector2(0, 0.42f), new Vector2(1, 0.92f), new Vector2(0.5f, 0.5f));
        midRt.offsetMin = Vector2.zero;
        midRt.offsetMax = Vector2.zero;

        // Player panel (left)
        var playerPanel = CreateUIObject("PlayerPanel", midSection.transform);
        var playerPanelRt = playerPanel.GetComponent<RectTransform>();
        SetAnchors(playerPanelRt, Vector2.zero, new Vector2(0.5f, 1), new Vector2(0, 0));
        playerPanelRt.offsetMin = new Vector2(20, 10);
        playerPanelRt.offsetMax = new Vector2(-10, -10);

        var playerVlg = playerPanel.AddComponent<VerticalLayoutGroup>();
        playerVlg.spacing = 5;
        playerVlg.padding = new RectOffset(10, 10, 5, 5);
        playerVlg.childAlignment = TextAnchor.MiddleCenter;

        var playerImgObj = CreateUIObject("PlayerImage", playerPanel.transform);
        playerImgObj.GetComponent<RectTransform>().sizeDelta = new Vector2(350, 350);
        var playerImg = playerImgObj.AddComponent<Image>();
        playerImg.color = new Color(0.4f, 0.4f, 0.5f, 1f);

        var playerNameObj = CreateUIObject("PlayerNameText", playerPanel.transform);
        playerNameObj.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 28);
        AddTMP(playerNameObj, "Player", 22, TextAlignmentOptions.Center, Color.cyan);

        var playerCondObj = CreateUIObject("PlayerLoseConditionsText", playerPanel.transform);
        playerCondObj.GetComponent<RectTransform>().sizeDelta = new Vector2(350, 80);
        AddTMP(playerCondObj, "", 13, TextAlignmentOptions.TopLeft, new Color(1f, 0.6f, 0.6f));

        // Opponent panel (right)
        var oppPanel = CreateUIObject("OpponentPanel", midSection.transform);
        var oppPanelRt = oppPanel.GetComponent<RectTransform>();
        SetAnchors(oppPanelRt, new Vector2(0.5f, 0), Vector2.one, new Vector2(1, 0));
        oppPanelRt.offsetMin = new Vector2(10, 10);
        oppPanelRt.offsetMax = new Vector2(-20, -10);

        var oppVlg = oppPanel.AddComponent<VerticalLayoutGroup>();
        oppVlg.spacing = 5;
        oppVlg.padding = new RectOffset(10, 10, 5, 5);
        oppVlg.childAlignment = TextAnchor.MiddleCenter;

        var oppImgObj = CreateUIObject("OpponentImage", oppPanel.transform);
        oppImgObj.GetComponent<RectTransform>().sizeDelta = new Vector2(350, 350);
        var oppImg = oppImgObj.AddComponent<Image>();
        oppImg.color = new Color(0.5f, 0.3f, 0.3f, 1f);

        var oppNameObj = CreateUIObject("OpponentNameText", oppPanel.transform);
        oppNameObj.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 28);
        AddTMP(oppNameObj, "Opponent", 22, TextAlignmentOptions.Center, Color.red);

        var oppCondObj = CreateUIObject("OpponentLoseConditionsText", oppPanel.transform);
        oppCondObj.GetComponent<RectTransform>().sizeDelta = new Vector2(350, 80);
        AddTMP(oppCondObj, "", 13, TextAlignmentOptions.TopLeft, new Color(1f, 0.6f, 0.6f));

        // === BOTTOM SECTION (42%) ===
        var botSection = CreateUIObject("BottomSection", canvas);
        var botRt = botSection.GetComponent<RectTransform>();
        SetAnchors(botRt, Vector2.zero, new Vector2(1, 0.42f), new Vector2(0.5f, 0));
        botRt.offsetMin = Vector2.zero;
        botRt.offsetMax = Vector2.zero;

        // -- Strategy Panel (left 55%)
        var strategyPanel = CreateUIObject("StrategyPanel", botSection.transform);
        var strategyPanelRt = strategyPanel.GetComponent<RectTransform>();
        SetAnchors(strategyPanelRt, Vector2.zero, new Vector2(0.55f, 1), new Vector2(0, 0));
        strategyPanelRt.offsetMin = new Vector2(10, 10);
        strategyPanelRt.offsetMax = new Vector2(-5, -10);

        var stratVlg = strategyPanel.AddComponent<VerticalLayoutGroup>();
        stratVlg.spacing = 5;
        stratVlg.padding = new RectOffset(5, 5, 5, 5);
        stratVlg.childControlWidth = true;
        stratVlg.childControlHeight = false;
        stratVlg.childForceExpandWidth = true;

        // "Strategy" title
        var stratTitleObj = CreateUIObject("StrategyTitleText", strategyPanel.transform);
        stratTitleObj.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 25);
        AddTMP(stratTitleObj, "Strategy", 20, TextAlignmentOptions.Left, Color.yellow);

        // Strategy text input (multi-line)
        var stratInputObj = CreateUIObject("StrategyTextField", strategyPanel.transform);
        var stratInputRt = stratInputObj.GetComponent<RectTransform>();
        stratInputRt.sizeDelta = new Vector2(0, 100);
        var stratInputImg = stratInputObj.AddComponent<Image>();
        stratInputImg.color = new Color(0.1f, 0.1f, 0.15f, 1f);
        var stratInputField = stratInputObj.AddComponent<TMP_InputField>();
        stratInputField.lineType = TMP_InputField.LineType.MultiLineNewline;
        stratInputField.characterLimit = 500;

        var stratTextArea = CreateUIObject("Text Area", stratInputObj.transform);
        StretchFill(stratTextArea.GetComponent<RectTransform>());
        stratTextArea.GetComponent<RectTransform>().offsetMin = new Vector2(5, 5);
        stratTextArea.GetComponent<RectTransform>().offsetMax = new Vector2(-5, -5);

        var stratText = CreateUIObject("Text", stratTextArea.transform);
        StretchFill(stratText.GetComponent<RectTransform>());
        var stratTmp = AddTMP(stratText, "", 14, TextAlignmentOptions.TopLeft);
        stratInputField.textComponent = stratTmp;

        var stratPlaceholder = CreateUIObject("Placeholder", stratTextArea.transform);
        StretchFill(stratPlaceholder.GetComponent<RectTransform>());
        var stratPhTmp = AddTMP(stratPlaceholder, "Describe your strategy...", 14, TextAlignmentOptions.TopLeft,
            new Color(0.5f, 0.5f, 0.5f));
        stratInputField.placeholder = stratPhTmp;
        stratInputField.textViewport = stratTextArea.GetComponent<RectTransform>();

        // "Items" subtitle
        var itemsTitleObj = CreateUIObject("ItemsTitleText", strategyPanel.transform);
        itemsTitleObj.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 22);
        AddTMP(itemsTitleObj, "Items", 16, TextAlignmentOptions.Left, new Color(0.8f, 0.8f, 0.8f));

        // Item slots grid container
        var itemSlotsContainer = CreateUIObject("ItemSlotsContainer", strategyPanel.transform);
        var itemSlotsRt = itemSlotsContainer.GetComponent<RectTransform>();
        itemSlotsRt.sizeDelta = new Vector2(0, 100);
        var itemSlotsGrid = itemSlotsContainer.AddComponent<GridLayoutGroup>();
        itemSlotsGrid.cellSize = new Vector2(90, 90);
        itemSlotsGrid.spacing = new Vector2(8, 8);
        itemSlotsGrid.padding = new RectOffset(5, 5, 5, 5);
        itemSlotsGrid.childAlignment = TextAnchor.MiddleLeft;

        // Create 4 item drop slots
        for (int i = 1; i <= 4; i++)
        {
            var slotObj = CreateUIObject($"ItemSlot{i}", itemSlotsContainer.transform);
            slotObj.GetComponent<RectTransform>().sizeDelta = new Vector2(90, 90);
            slotObj.AddComponent<Image>().color = new Color(0.25f, 0.25f, 0.3f, 0.6f);

            var slotVlg = slotObj.AddComponent<VerticalLayoutGroup>();
            slotVlg.spacing = 2;
            slotVlg.childAlignment = TextAnchor.MiddleCenter;
            slotVlg.childControlWidth = true;
            slotVlg.childControlHeight = false;
            slotVlg.childForceExpandWidth = true;

            var slotImgObj = CreateUIObject("SlotImage", slotObj.transform);
            slotImgObj.GetComponent<RectTransform>().sizeDelta = new Vector2(65, 65);
            slotImgObj.AddComponent<Image>().color = new Color(0.8f, 0.8f, 0.8f, 0.5f);

            var slotLblObj = CreateUIObject("SlotLabel", slotObj.transform);
            slotLblObj.GetComponent<RectTransform>().sizeDelta = new Vector2(80, 18);
            AddTMP(slotLblObj, "Item", 12);

            slotObj.AddComponent<SilverTongue.BattleScene.DropTarget>();
        }

        // Add StrategyPanelUI component
        strategyPanel.AddComponent<SilverTongue.BattleScene.StrategyPanelUI>();

        // -- Inventory Panel (right 45%)
        var invPanel = CreateUIObject("InventoryPanel", botSection.transform);
        var invPanelRt = invPanel.GetComponent<RectTransform>();
        SetAnchors(invPanelRt, new Vector2(0.55f, 0), Vector2.one, new Vector2(1, 0));
        invPanelRt.offsetMin = new Vector2(5, 10);
        invPanelRt.offsetMax = new Vector2(-10, -10);

        var invVlg = invPanel.AddComponent<VerticalLayoutGroup>();
        invVlg.spacing = 5;
        invVlg.padding = new RectOffset(5, 5, 5, 5);
        invVlg.childControlWidth = true;
        invVlg.childControlHeight = false;
        invVlg.childForceExpandWidth = true;

        // "Inventory" title
        var invTitleObj = CreateUIObject("InventoryTitleText", invPanel.transform);
        invTitleObj.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 25);
        AddTMP(invTitleObj, "Inventory", 20, TextAlignmentOptions.Left, Color.yellow);

        // Inventory grid (scrollable)
        var (invScrollRect, invContentRt) = CreateScrollView("InventoryScroll", invPanel.transform, horizontal: false, vertical: true);
        var invScrollRt = invScrollRect.GetComponent<RectTransform>();
        invScrollRt.sizeDelta = new Vector2(0, 400);
        invScrollRect.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.14f, 0.5f);

        var invGridRt = invContentRt;
        var gridLayout = invGridRt.gameObject.AddComponent<GridLayoutGroup>();
        gridLayout.cellSize = new Vector2(100, 130);
        gridLayout.spacing = new Vector2(8, 8);
        gridLayout.padding = new RectOffset(5, 5, 5, 5);
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = 8;
        var gridCsf = invGridRt.gameObject.AddComponent<ContentSizeFitter>();
        gridCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Confirm Strategy Button at bottom
        var confirmBtn = CreateButton("ConfirmStrategyButton", canvas, "CONFIRM STRATEGY",
            new Vector2(250, 45), new Color(0.2f, 0.7f, 0.3f));
        var confirmRt = confirmBtn.GetComponent<RectTransform>();
        SetAnchors(confirmRt, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0));
        confirmRt.anchoredPosition = new Vector2(0, 30);
        confirmRt.sizeDelta = new Vector2(250, 45);

        foreach (var old in instance.GetComponents<SilverTongue.BattleScene.StrategySelectingView>())
            Object.DestroyImmediate(old);
        instance.AddComponent<SilverTongue.BattleScene.StrategySelectingView>();

        PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
        Object.DestroyImmediate(instance);
        Debug.Log($"[BattleUIBuilder] Updated: {prefabPath}");
    }

    // ─── BattleCanvas ────────────────────────────────────────────────────
    static void BuildBattleCanvas()
    {
        string prefabPath = "Assets/Prefabs/BattleScene/BattleCanvas.prefab";
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
        var canvas = instance.transform.Find("Canvas");
        ClearChildren(canvas);

        // Background
        var bgObj = CreateUIObject("Background", canvas);
        StretchFill(bgObj.GetComponent<RectTransform>());
        bgObj.AddComponent<Image>().color = new Color(0.05f, 0.05f, 0.1f, 1f);

        // === TOP UI (top 10%) ===
        var topUI = CreateUIObject("TopUI", canvas);
        var topRt = topUI.GetComponent<RectTransform>();
        SetAnchors(topRt, new Vector2(0, 0.9f), Vector2.one, new Vector2(0.5f, 1));
        topRt.offsetMin = Vector2.zero;
        topRt.offsetMax = Vector2.zero;

        // Turn tracker (center top)
        var turnObj = CreateUIObject("TurnTrackerText", topUI.transform);
        var turnRt = turnObj.GetComponent<RectTransform>();
        SetAnchors(turnRt, new Vector2(0.3f, 0.3f), new Vector2(0.7f, 1f), new Vector2(0.5f, 0.5f));
        turnRt.offsetMin = Vector2.zero;
        turnRt.offsetMax = Vector2.zero;
        AddTMP(turnObj, "Turn 1/7", 28);

        // Go-to-strategy button
        var strategyBtn = CreateButton("GoToStrategyButton", topUI.transform, "STRATEGY", new Vector2(120, 30),
            new Color(0.7f, 0.3f, 0.3f));
        var strategyRt = strategyBtn.GetComponent<RectTransform>();
        SetAnchors(strategyRt, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0));
        strategyRt.anchoredPosition = new Vector2(0, 2);
        strategyRt.sizeDelta = new Vector2(120, 30);

        // Log button (left)
        var logBtnB = CreateButton("LogButton", topUI.transform, "LOG", new Vector2(80, 35),
            new Color(0.3f, 0.3f, 0.5f));
        var logBtnBRt = logBtnB.GetComponent<RectTransform>();
        SetAnchors(logBtnBRt, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f));
        logBtnBRt.anchoredPosition = new Vector2(15, 0);
        logBtnBRt.sizeDelta = new Vector2(80, 35);

        // === STAGE AREA (middle 55%) ===
        var stageArea = CreateUIObject("StageArea", canvas);
        var stageRt = stageArea.GetComponent<RectTransform>();
        SetAnchors(stageRt, new Vector2(0, 0.35f), new Vector2(1, 0.9f), new Vector2(0.5f, 0.5f));
        stageRt.offsetMin = Vector2.zero;
        stageRt.offsetMax = Vector2.zero;

        // Player character (left)
        var playerPanel = CreateUIObject("PlayerCharacterPanel", stageArea.transform);
        var playerPanelRt = playerPanel.GetComponent<RectTransform>();
        SetAnchors(playerPanelRt, new Vector2(0.05f, 0.1f), new Vector2(0.35f, 0.95f), new Vector2(0.5f, 0.5f));
        playerPanelRt.offsetMin = Vector2.zero;
        playerPanelRt.offsetMax = Vector2.zero;

        var playerImgObj = CreateUIObject("PlayerCharacterImage", playerPanel.transform);
        var playerImgRt = playerImgObj.GetComponent<RectTransform>();
        SetAnchors(playerImgRt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        playerImgRt.sizeDelta = new Vector2(1000, 1000);
        playerImgRt.localPosition = new Vector3(0, -215, 0); // Lower the player image a bit
        playerImgObj.AddComponent<Image>().color = new Color(0.3f, 0.3f, 0.3f, 1f);

        var playerNameObj = CreateUIObject("PlayerNameLabel", playerPanel.transform);
        var playerNameRt = playerNameObj.GetComponent<RectTransform>();
        SetAnchors(playerNameRt, Vector2.zero, new Vector2(1, 0.15f), new Vector2(0.5f, 0));
        playerNameRt.offsetMin = Vector2.zero;
        playerNameRt.offsetMax = Vector2.zero;
        AddTMP(playerNameObj, "Player", 20, TextAlignmentOptions.Center, Color.cyan);

        // Opponent character (right)
        var oppPanel = CreateUIObject("OpponentCharacterPanel", stageArea.transform);
        var oppPanelRt = oppPanel.GetComponent<RectTransform>();
        SetAnchors(oppPanelRt, new Vector2(0.65f, 0.1f), new Vector2(0.95f, 0.95f), new Vector2(0.5f, 0.5f));
        oppPanelRt.offsetMin = Vector2.zero;
        oppPanelRt.offsetMax = Vector2.zero;

        var oppImgObj = CreateUIObject("OpponentCharacterImage", oppPanel.transform);
        var oppImgRt = oppImgObj.GetComponent<RectTransform>();
        SetAnchors(oppImgRt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        oppImgRt.sizeDelta = new Vector2(1000, 1000);
        oppImgRt.localPosition = new Vector3(0, -215, 0); // Lower the opponent image a bit
        oppImgObj.AddComponent<Image>().color = new Color(0.5f, 0.3f, 0.3f, 1f);

        var oppNameObj = CreateUIObject("OpponentNameLabel", oppPanel.transform);
        var oppNameRt = oppNameObj.GetComponent<RectTransform>();
        SetAnchors(oppNameRt, Vector2.zero, new Vector2(1, 0.15f), new Vector2(0.5f, 0));
        oppNameRt.offsetMin = Vector2.zero;
        oppNameRt.offsetMax = Vector2.zero;
        AddTMP(oppNameObj, "Opponent", 20, TextAlignmentOptions.Center, Color.red);

        // === DIALOGUE AREA (bottom 35%) ===
        var dialogueArea = CreateUIObject("DialogueArea", canvas);
        var dialogueRt = dialogueArea.GetComponent<RectTransform>();
        SetAnchors(dialogueRt, Vector2.zero, new Vector2(1, 0.35f), new Vector2(0.5f, 0));
        dialogueRt.offsetMin = Vector2.zero;
        dialogueRt.offsetMax = Vector2.zero;
        dialogueArea.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.15f, 0.95f);

        var dialogVlg = dialogueArea.AddComponent<VerticalLayoutGroup>();
        dialogVlg.padding = new RectOffset(15, 15, 10, 10);
        dialogVlg.spacing = 5;
        dialogVlg.childControlWidth = false;
        dialogVlg.childControlHeight = false;
        dialogVlg.childForceExpandWidth = false;
        dialogVlg.childForceExpandHeight = false;

        // Speaker name
        var speakerObj = CreateUIObject("SpeakerNameText", dialogueArea.transform);
        speakerObj.GetComponent<RectTransform>().sizeDelta = new Vector2(930, 30);
        AddTMP(speakerObj, "Speaker", 22, TextAlignmentOptions.Left, Color.yellow);

        // Dialogue text
        var dialogTextObj = CreateUIObject("DialogueText", dialogueArea.transform);
        dialogTextObj.GetComponent<RectTransform>().sizeDelta = new Vector2(930, 80);
        AddTMP(dialogTextObj, "", 18, TextAlignmentOptions.TopLeft);

        // Thought text (small, gray)
        var thoughtTextObj = CreateUIObject("ThoughtText", dialogueArea.transform);
        thoughtTextObj.GetComponent<RectTransform>().sizeDelta = new Vector2(930, 50);
        var thoughtTmp = AddTMP(thoughtTextObj, "", 13, TextAlignmentOptions.TopLeft, new Color(0.55f, 0.55f, 0.55f));
        thoughtTmp.fontStyle = TMPro.FontStyles.Italic;

        // Auto-progress button (top-right of dialogue area, ignores layout)
        var autoBtn = CreateButton("AutoProgressButton", dialogueArea.transform, "AUTO: OFF", new Vector2(110, 28),
            new Color(0.5f, 0.5f, 0.2f));
        var autoRt = autoBtn.GetComponent<RectTransform>();
        SetAnchors(autoRt, new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1));
        autoRt.anchoredPosition = new Vector2(-5, -5);
        autoRt.sizeDelta = new Vector2(110, 28);
        autoBtn.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;

        // Make dialogue area clickable for manual advance
        var dialogueBtn = dialogueArea.AddComponent<Button>();
        dialogueBtn.targetGraphic = dialogueArea.GetComponent<Image>();
        dialogueBtn.transition = Selectable.Transition.None;

        foreach (var old in instance.GetComponents<SilverTongue.BattleScene.BattleView>())
            Object.DestroyImmediate(old);
        instance.AddComponent<SilverTongue.BattleScene.BattleView>();

        PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
        Object.DestroyImmediate(instance);
        Debug.Log($"[BattleUIBuilder] Updated: {prefabPath}");
    }

    // ─── BattleResultCanvas ──────────────────────────────────────────────
    static void BuildBattleResultCanvas()
    {
        string prefabPath = "Assets/Prefabs/BattleScene/BattleResultCanvas.prefab";
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
        var canvas = instance.transform.Find("Canvas");
        ClearChildren(canvas);

        // Background
        var bgObj = CreateUIObject("Background", canvas);
        StretchFill(bgObj.GetComponent<RectTransform>());
        bgObj.AddComponent<Image>().color = new Color(0.05f, 0.05f, 0.1f, 1f);

        // Result text (top)
        var resultObj = CreateUIObject("ResultText", canvas);
        var resultRt = resultObj.GetComponent<RectTransform>();
        SetAnchors(resultRt, new Vector2(0, 0.55f), new Vector2(1, 0.85f), new Vector2(0.5f, 0.5f));
        resultRt.offsetMin = Vector2.zero;
        resultRt.offsetMax = Vector2.zero;
        AddTMP(resultObj, "WIN", 72, TextAlignmentOptions.Center, Color.yellow);

        // Description text (middle)
        var descObj = CreateUIObject("ResultDescriptionText", canvas);
        var descRt = descObj.GetComponent<RectTransform>();
        SetAnchors(descRt, new Vector2(0.1f, 0.3f), new Vector2(0.9f, 0.55f), new Vector2(0.5f, 0.5f));
        descRt.offsetMin = Vector2.zero;
        descRt.offsetMax = Vector2.zero;
        AddTMP(descObj, "Your words reached their heart!", 24, TextAlignmentOptions.Center);

        // Close button (bottom)
        var closeBtn = CreateButton("CloseButton", canvas, "CLOSE", new Vector2(200, 50),
            new Color(0.2f, 0.6f, 1f));
        var closeRt = closeBtn.GetComponent<RectTransform>();
        SetAnchors(closeRt, new Vector2(0.5f, 0.1f), new Vector2(0.5f, 0.1f), new Vector2(0.5f, 0.5f));
        closeRt.sizeDelta = new Vector2(200, 50);

        foreach (var old in instance.GetComponents<SilverTongue.BattleScene.BattleResultView>())
            Object.DestroyImmediate(old);
        instance.AddComponent<SilverTongue.BattleScene.BattleResultView>();

        PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
        Object.DestroyImmediate(instance);
        Debug.Log($"[BattleUIBuilder] Updated: {prefabPath}");
    }

    // ─── ChatBubble Prefab ────────────────────────────────────────────────
    static void BuildChatBubblePrefab()
    {
        var root = CreateUIObject("ChatBubble", null);
        var rootRt = root.GetComponent<RectTransform>();
        rootRt.sizeDelta = new Vector2(0, 120);

        var hlg = root.AddComponent<HorizontalLayoutGroup>();
        hlg.childAlignment = TextAnchor.UpperLeft;
        hlg.childControlWidth = false;
        hlg.childControlHeight = false;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;
        hlg.padding = new RectOffset(10, 10, 5, 5);

        var le = root.AddComponent<LayoutElement>();
        le.preferredHeight = 120;
        le.flexibleWidth = 1;

        // Bubble background
        var bubble = CreateUIObject("BubbleBackground", root.transform);
        var bubbleRt = bubble.GetComponent<RectTransform>();
        bubbleRt.sizeDelta = new Vector2(600, 110);
        var bubbleBg = bubble.AddComponent<Image>();
        bubbleBg.color = new Color(0.2f, 0.4f, 0.6f, 0.9f);

        var bubbleVlg = bubble.AddComponent<VerticalLayoutGroup>();
        bubbleVlg.padding = new RectOffset(10, 10, 5, 5);
        bubbleVlg.spacing = 3;
        bubbleVlg.childControlWidth = true;
        bubbleVlg.childControlHeight = false;
        bubbleVlg.childForceExpandWidth = true;

        var speakerObj = CreateUIObject("SpeakerNameText", bubble.transform);
        speakerObj.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 22);
        AddTMP(speakerObj, "Speaker", 16, TextAlignmentOptions.Left, Color.yellow);

        var speechObj = CreateUIObject("SpeechText", bubble.transform);
        speechObj.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 50);
        AddTMP(speechObj, "", 14, TextAlignmentOptions.TopLeft);

        var tsObj = CreateUIObject("TimestampText", bubble.transform);
        tsObj.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 18);
        AddTMP(tsObj, "Turn 1", 11, TextAlignmentOptions.Right, new Color(0.6f, 0.6f, 0.6f));

        var indicatorContainer = CreateUIObject("IndicatorContainer", bubble.transform);
        indicatorContainer.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 18);
        indicatorContainer.SetActive(false);

        var indicatorText = CreateUIObject("IndicatorText", indicatorContainer.transform);
        StretchFill(indicatorText.GetComponent<RectTransform>());
        AddTMP(indicatorText, "", 11, TextAlignmentOptions.Left, new Color(0.4f, 0.8f, 0.4f));

        root.AddComponent<SilverTongue.BattleScene.ChatBubbleUI>();

        string path = "Assets/Prefabs/BattleScene/UI/ChatBubble.prefab";
        PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        Debug.Log($"[BattleUIBuilder] Created: {path}");
    }

    // ─── ConversationHistoryCanvas ────────────────────────────────────────
    static void BuildConversationHistoryCanvas()
    {
        var root = new GameObject("ConversationHistoryCanvas");

        var canvasObj = new GameObject("Canvas");
        canvasObj.transform.SetParent(root.transform, false);
        canvasObj.layer = 5;
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();

        // Dark overlay background
        var overlay = CreateUIObject("DarkOverlay", canvasObj.transform);
        StretchFill(overlay.GetComponent<RectTransform>());
        overlay.AddComponent<Image>().color = new Color(0, 0, 0, 0.8f);

        // Content panel
        var panel = CreateUIObject("ContentPanel", canvasObj.transform);
        var panelRt = panel.GetComponent<RectTransform>();
        panelRt.anchorMin = new Vector2(0.1f, 0.05f);
        panelRt.anchorMax = new Vector2(0.9f, 0.95f);
        panelRt.offsetMin = Vector2.zero;
        panelRt.offsetMax = Vector2.zero;
        panel.AddComponent<Image>().color = new Color(0.12f, 0.12f, 0.18f, 0.98f);

        var panelVlg = panel.AddComponent<VerticalLayoutGroup>();
        panelVlg.padding = new RectOffset(10, 10, 10, 10);
        panelVlg.spacing = 10;
        panelVlg.childControlWidth = true;
        panelVlg.childControlHeight = false;
        panelVlg.childForceExpandWidth = true;

        // Header row
        var header = CreateUIObject("Header", panel.transform);
        header.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 50);
        var headerHlg = header.AddComponent<HorizontalLayoutGroup>();
        headerHlg.childAlignment = TextAnchor.MiddleCenter;
        headerHlg.childControlWidth = false;
        headerHlg.childControlHeight = true;
        headerHlg.childForceExpandWidth = false;

        var titleObj = CreateUIObject("TitleText", header.transform);
        titleObj.GetComponent<RectTransform>().sizeDelta = new Vector2(500, 50);
        AddTMP(titleObj, "Conversation History", 30, TextAlignmentOptions.Center, Color.yellow);

        CreateButton("CloseButton", header.transform, "X", new Vector2(50, 40),
            new Color(0.7f, 0.2f, 0.2f));

        // Chat ScrollView
        var (scrollRect, contentRt) = CreateScrollView("ChatScrollView", panel.transform, false, true);
        scrollRect.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 600);
        scrollRect.GetComponent<Image>().color = new Color(0.08f, 0.08f, 0.12f, 0.5f);

        var contentVlg = contentRt.gameObject.AddComponent<VerticalLayoutGroup>();
        contentVlg.spacing = 10;
        contentVlg.padding = new RectOffset(10, 10, 10, 10);
        contentVlg.childControlWidth = true;
        contentVlg.childControlHeight = false;
        contentVlg.childForceExpandWidth = true;
        contentVlg.childForceExpandHeight = false;
        var contentCsf = contentRt.gameObject.AddComponent<ContentSizeFitter>();
        contentCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        root.AddComponent<SilverTongue.BattleScene.ConversationHistoryView>();

        string path = "Assets/Prefabs/BattleScene/ConversationHistoryCanvas.prefab";
        PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        Debug.Log($"[BattleUIBuilder] Created: {path}");
    }
}
