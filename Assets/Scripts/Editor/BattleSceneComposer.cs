using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using SilverTongue.BattleScene;
using SilverTongue.Data;

public class BattleSceneComposer
{
    [MenuItem("Tools/Compose Battle Scene")]
    public static void ComposeScene()
    {
        // Ensure we're in BattleScene
        var activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        Debug.Log($"[BattleSceneComposer] Composing in scene: {activeScene.name}");

        // === 1. Create GameManager ===
        var gmObj = GameObject.Find("GameManager");
        if (gmObj == null)
        {
            gmObj = new GameObject("GameManager");
        }
        var gm = gmObj.GetComponent<SilverTongue.GameManager>();
        if (gm == null)
            gm = gmObj.AddComponent<SilverTongue.GameManager>();

        // Load character SOs
        var kenta = AssetDatabase.LoadAssetAtPath<CharacterSO>("Assets/ScriptableObjects/Characters/Protagonist_Kenta.asset");
        var prophet = AssetDatabase.LoadAssetAtPath<CharacterSO>("Assets/ScriptableObjects/Characters/Opponent_Prophet.asset");
        var testChar = AssetDatabase.LoadAssetAtPath<CharacterSO>("Assets/ScriptableObjects/Characters/TestChar_1.asset");

        var so = new SerializedObject(gm);
        var charProp = so.FindProperty("availableCharacters");
        charProp.arraySize = 3;
        charProp.GetArrayElementAtIndex(0).objectReferenceValue = kenta;
        charProp.GetArrayElementAtIndex(1).objectReferenceValue = testChar;
        charProp.GetArrayElementAtIndex(2).objectReferenceValue = prophet;

        // Load item SOs (all 7 evidence items from spec)
        var manual = AssetDatabase.LoadAssetAtPath<ItemSO>("Assets/ScriptableObjects/Items/Item_OldManual.asset");
        var radio = AssetDatabase.LoadAssetAtPath<ItemSO>("Assets/ScriptableObjects/Items/Item_PortableRadio.asset");
        var report = AssetDatabase.LoadAssetAtPath<ItemSO>("Assets/ScriptableObjects/Items/Item_FailedReport.asset");
        var momLetter = AssetDatabase.LoadAssetAtPath<ItemSO>("Assets/ScriptableObjects/Items/Item_MomsLetter.asset");
        var dismissal = AssetDatabase.LoadAssetAtPath<ItemSO>("Assets/ScriptableObjects/Items/Item_DismissalNotice.asset");
        var smartphone = AssetDatabase.LoadAssetAtPath<ItemSO>("Assets/ScriptableObjects/Items/Item_5GSmartphone.asset");
        var coupon = AssetDatabase.LoadAssetAtPath<ItemSO>("Assets/ScriptableObjects/Items/Item_500wonCoupon.asset");

        var itemsProp = so.FindProperty("playerItems");
        itemsProp.arraySize = 7;
        itemsProp.GetArrayElementAtIndex(0).objectReferenceValue = manual;
        itemsProp.GetArrayElementAtIndex(1).objectReferenceValue = radio;
        itemsProp.GetArrayElementAtIndex(2).objectReferenceValue = report;
        itemsProp.GetArrayElementAtIndex(3).objectReferenceValue = momLetter;
        itemsProp.GetArrayElementAtIndex(4).objectReferenceValue = dismissal;
        itemsProp.GetArrayElementAtIndex(5).objectReferenceValue = smartphone;
        itemsProp.GetArrayElementAtIndex(6).objectReferenceValue = coupon;
        so.ApplyModifiedProperties();

        // === 2. Instantiate Canvas Prefabs ===
        var battlerSelectingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/BattleScene/BattlerSelectingCanvas.prefab");
        var strategySelectingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/BattleScene/StrategySelectingCanvas.prefab");
        var battlePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/BattleScene/BattleCanvas.prefab");
        var battleResultPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/BattleScene/BattleResultCanvas.prefab");

        // Remove old instances
        DestroyIfExists("BattlerSelectingCanvas");
        DestroyIfExists("StrategySelectingCanvas");
        DestroyIfExists("BattleCanvas");
        DestroyIfExists("BattleResultCanvas");
        DestroyIfExists("ConversationHistoryCanvas");
        DestroyIfExists("BattleSceneManager");

        var conversationHistoryPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/BattleScene/ConversationHistoryCanvas.prefab");

        var battlerSelectingObj = (GameObject)PrefabUtility.InstantiatePrefab(battlerSelectingPrefab);
        var strategySelectingObj = (GameObject)PrefabUtility.InstantiatePrefab(strategySelectingPrefab);
        var battleObj = (GameObject)PrefabUtility.InstantiatePrefab(battlePrefab);
        var battleResultObj = (GameObject)PrefabUtility.InstantiatePrefab(battleResultPrefab);
        var conversationHistoryObj = conversationHistoryPrefab != null
            ? (GameObject)PrefabUtility.InstantiatePrefab(conversationHistoryPrefab)
            : null;

        // Remove duplicate EventSystems (keep only one)
        var esSS = strategySelectingObj.transform.Find("EventSystem");
        if (esSS != null) Object.DestroyImmediate(esSS.gameObject);
        var esBattle = battleObj.transform.Find("EventSystem");
        if (esBattle != null) Object.DestroyImmediate(esBattle.gameObject);
        var esResult = battleResultObj.transform.Find("EventSystem");
        if (esResult != null) Object.DestroyImmediate(esResult.gameObject);

        // === 3. Create BattleSceneManager ===
        var bsmObj = new GameObject("BattleSceneManager");
        var bsm = bsmObj.AddComponent<BattleSceneManager>();

        var bsmSo = new SerializedObject(bsm);

        // Set canvas references
        bsmSo.FindProperty("battlerSelectingCanvas").objectReferenceValue = battlerSelectingObj;
        bsmSo.FindProperty("strategySelectingCanvas").objectReferenceValue = strategySelectingObj;
        bsmSo.FindProperty("battleCanvas").objectReferenceValue = battleObj;
        bsmSo.FindProperty("battleResultCanvas").objectReferenceValue = battleResultObj;

        // Set view references
        bsmSo.FindProperty("battlerSelectingView").objectReferenceValue = battlerSelectingObj.GetComponent<BattlerSelectingView>();
        bsmSo.FindProperty("strategySelectingView").objectReferenceValue = strategySelectingObj.GetComponent<StrategySelectingView>();
        bsmSo.FindProperty("battleView").objectReferenceValue = battleObj.GetComponent<BattleView>();
        bsmSo.FindProperty("battleResultView").objectReferenceValue = battleResultObj.GetComponent<BattleResultView>();

        // Conversation History overlay
        if (conversationHistoryObj != null)
        {
            bsmSo.FindProperty("conversationHistoryCanvas").objectReferenceValue = conversationHistoryObj;
            bsmSo.FindProperty("conversationHistoryView").objectReferenceValue = conversationHistoryObj.GetComponent<ConversationHistoryView>();
        }

        // Set opponent (default to Prophet)
        bsmSo.FindProperty("opponent").objectReferenceValue = prophet;
        bsmSo.FindProperty("maxTurns").intValue = 7;
        bsmSo.FindProperty("useMockLLM").boolValue = false;

        bsmSo.ApplyModifiedProperties();

        // === 4. Wire up View SerializedFields ===
        WireBattlerSelectingView(battlerSelectingObj);
        WireStrategySelectingView(strategySelectingObj);
        WireBattleView(battleObj);
        WireBattleResultView(battleResultObj);
        if (conversationHistoryObj != null)
            WireConversationHistoryView(conversationHistoryObj);

        // === 5. Save Scene ===
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(activeScene);
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(activeScene);

        Debug.Log("[BattleSceneComposer] Scene composed successfully!");
    }

    static void DestroyIfExists(string name)
    {
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        var rootObjects = scene.GetRootGameObjects();
        foreach (var root in rootObjects)
        {
            if (root.name == name)
                Object.DestroyImmediate(root);
        }
    }

    static T FindInChildren<T>(GameObject root, string path) where T : Component
    {
        var tr = root.transform.Find(path);
        if (tr == null)
        {
            Debug.LogWarning($"[BattleSceneComposer] Could not find: {path} in {root.name}");
            return null;
        }
        return tr.GetComponent<T>();
    }

    static GameObject FindChild(GameObject root, string path)
    {
        var tr = root.transform.Find(path);
        if (tr == null)
        {
            Debug.LogWarning($"[BattleSceneComposer] Could not find child: {path} in {root.name}");
            return null;
        }
        return tr.gameObject;
    }

    // ─── Wire BattlerSelectingView ──────────────────────────────────────
    static void WireBattlerSelectingView(GameObject root)
    {
        var view = root.GetComponent<BattlerSelectingView>();
        if (view == null) return;
        var so = new SerializedObject(view);

        var cardPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/BattleScene/UI/CharacterCard.prefab");
        so.FindProperty("characterCardPrefab").objectReferenceValue = cardPrefab;

        var content = FindChild(root, "Canvas/ScrollView/Viewport/Content");
        so.FindProperty("characterCardContainer").objectReferenceValue = content != null ? content.transform : null;

        so.FindProperty("scrollRect").objectReferenceValue = FindInChildren<ScrollRect>(root, "Canvas/ScrollView");

        so.FindProperty("confirmationPopup").objectReferenceValue = FindChild(root, "Canvas/ConfirmationPopup");
        so.FindProperty("confirmPopupImage").objectReferenceValue = FindInChildren<Image>(root, "Canvas/ConfirmationPopup/PopupPanel/PopupImage");
        so.FindProperty("confirmPopupName").objectReferenceValue = FindInChildren<TMPro.TextMeshProUGUI>(root, "Canvas/ConfirmationPopup/PopupPanel/PopupNameText");
        so.FindProperty("confirmYesButton").objectReferenceValue = FindInChildren<Button>(root, "Canvas/ConfirmationPopup/PopupPanel/ButtonRow/YesButton");
        so.FindProperty("confirmNoButton").objectReferenceValue = FindInChildren<Button>(root, "Canvas/ConfirmationPopup/PopupPanel/ButtonRow/NoButton");

        so.ApplyModifiedProperties();
        Debug.Log("[BattleSceneComposer] Wired BattlerSelectingView");
    }

    // ─── Wire StrategySelectingView ─────────────────────────────────────
    static void WireStrategySelectingView(GameObject root)
    {
        var view = root.GetComponent<StrategySelectingView>();
        if (view == null) return;
        var so = new SerializedObject(view);

        // Top bar
        so.FindProperty("backToSelectionButton").objectReferenceValue = FindInChildren<Button>(root, "Canvas/TopBar/BackButton");
        so.FindProperty("turnCounterText").objectReferenceValue = FindInChildren<TMPro.TextMeshProUGUI>(root, "Canvas/TopBar/TurnCounterText");

        // Character display
        so.FindProperty("playerImage").objectReferenceValue = FindInChildren<Image>(root, "Canvas/MiddleSection/PlayerPanel/PlayerImage");
        so.FindProperty("playerNameText").objectReferenceValue = FindInChildren<TMPro.TextMeshProUGUI>(root, "Canvas/MiddleSection/PlayerPanel/PlayerNameText");
        so.FindProperty("opponentImage").objectReferenceValue = FindInChildren<Image>(root, "Canvas/MiddleSection/OpponentPanel/OpponentImage");
        so.FindProperty("opponentNameText").objectReferenceValue = FindInChildren<TMPro.TextMeshProUGUI>(root, "Canvas/MiddleSection/OpponentPanel/OpponentNameText");
        so.FindProperty("playerLoseConditionsText").objectReferenceValue = FindInChildren<TMPro.TextMeshProUGUI>(root, "Canvas/MiddleSection/PlayerPanel/PlayerLoseConditionsText");
        so.FindProperty("opponentLoseConditionsText").objectReferenceValue = FindInChildren<TMPro.TextMeshProUGUI>(root, "Canvas/MiddleSection/OpponentPanel/OpponentLoseConditionsText");

        // Strategy panel
        so.FindProperty("strategyPanel").objectReferenceValue = FindInChildren<StrategyPanelUI>(root, "Canvas/BottomSection/StrategyPanel");

        // Inventory
        var invItemPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/BattleScene/UI/InventoryItem.prefab");
        so.FindProperty("inventoryItemPrefab").objectReferenceValue = invItemPrefab;

        var invGrid = FindChild(root, "Canvas/BottomSection/InventoryPanel/InventoryScroll/Viewport/Content");
        so.FindProperty("inventoryGrid").objectReferenceValue = invGrid != null ? invGrid.transform : null;

        so.FindProperty("confirmStrategyButton").objectReferenceValue = FindInChildren<Button>(root, "Canvas/ConfirmStrategyButton");

        // Log button
        so.FindProperty("logButton").objectReferenceValue = FindInChildren<Button>(root, "Canvas/TopBar/LogButton");

        so.ApplyModifiedProperties();
        Debug.Log("[BattleSceneComposer] Wired StrategySelectingView");
    }

    // ─── Wire BattleView ────────────────────────────────────────────────
    static void WireBattleView(GameObject root)
    {
        var view = root.GetComponent<BattleView>();
        if (view == null) return;
        var so = new SerializedObject(view);

        so.FindProperty("turnTrackerText").objectReferenceValue = FindInChildren<TMPro.TextMeshProUGUI>(root, "Canvas/TopUI/TurnTrackerText");
        so.FindProperty("autoProgressButton").objectReferenceValue = FindInChildren<Button>(root, "Canvas/DialogueArea/AutoProgressButton");
        so.FindProperty("autoProgressButtonText").objectReferenceValue = FindInChildren<TMPro.TextMeshProUGUI>(root, "Canvas/DialogueArea/AutoProgressButton/Text");
        so.FindProperty("goToStrategyButton").objectReferenceValue = FindInChildren<Button>(root, "Canvas/TopUI/GoToStrategyButton");
        so.FindProperty("logButton").objectReferenceValue = FindInChildren<Button>(root, "Canvas/TopUI/LogButton");

        so.FindProperty("playerCharacterImage").objectReferenceValue = FindInChildren<Image>(root, "Canvas/StageArea/PlayerCharacterPanel/PlayerCharacterImage");
        so.FindProperty("opponentCharacterImage").objectReferenceValue = FindInChildren<Image>(root, "Canvas/StageArea/OpponentCharacterPanel/OpponentCharacterImage");
        so.FindProperty("playerNameLabel").objectReferenceValue = FindInChildren<TMPro.TextMeshProUGUI>(root, "Canvas/StageArea/PlayerCharacterPanel/PlayerNameLabel");
        so.FindProperty("opponentNameLabel").objectReferenceValue = FindInChildren<TMPro.TextMeshProUGUI>(root, "Canvas/StageArea/OpponentCharacterPanel/OpponentNameLabel");

        var playerPanelTr = root.transform.Find("Canvas/StageArea/PlayerCharacterPanel");
        so.FindProperty("playerPanel").objectReferenceValue = playerPanelTr != null ? playerPanelTr.GetComponent<RectTransform>() : null;
        var opponentPanelTr = root.transform.Find("Canvas/StageArea/OpponentCharacterPanel");
        so.FindProperty("opponentPanel").objectReferenceValue = opponentPanelTr != null ? opponentPanelTr.GetComponent<RectTransform>() : null;

        so.FindProperty("speakerNameText").objectReferenceValue = FindInChildren<TMPro.TextMeshProUGUI>(root, "Canvas/DialogueArea/SpeakerNameText");
        so.FindProperty("dialogueText").objectReferenceValue = FindInChildren<TMPro.TextMeshProUGUI>(root, "Canvas/DialogueArea/DialogueText");
        so.FindProperty("thoughtText").objectReferenceValue = FindInChildren<TMPro.TextMeshProUGUI>(root, "Canvas/DialogueArea/ThoughtText");
        so.FindProperty("dialogueAreaButton").objectReferenceValue = FindInChildren<Button>(root, "Canvas/DialogueArea");

        // Sanity bar
        so.FindProperty("sanityBarFill").objectReferenceValue = FindInChildren<Image>(root, "Canvas/TopUI/SanityBarBg/SanityBarFill");
        so.FindProperty("sanityText").objectReferenceValue = FindInChildren<TMPro.TextMeshProUGUI>(root, "Canvas/TopUI/SanityBarBg/SanityText");

        so.ApplyModifiedProperties();
        Debug.Log("[BattleSceneComposer] Wired BattleView");
    }

    // ─── Wire BattleResultView ──────────────────────────────────────────
    static void WireBattleResultView(GameObject root)
    {
        var view = root.GetComponent<BattleResultView>();
        if (view == null) return;
        var so = new SerializedObject(view);

        so.FindProperty("resultText").objectReferenceValue = FindInChildren<TMPro.TextMeshProUGUI>(root, "Canvas/ResultText");
        so.FindProperty("resultDescriptionText").objectReferenceValue = FindInChildren<TMPro.TextMeshProUGUI>(root, "Canvas/ResultDescriptionText");
        so.FindProperty("closeButton").objectReferenceValue = FindInChildren<Button>(root, "Canvas/CloseButton");

        so.ApplyModifiedProperties();
        Debug.Log("[BattleSceneComposer] Wired BattleResultView");
    }

    // ─── Wire ConversationHistoryView ────────────────────────────────────
    static void WireConversationHistoryView(GameObject root)
    {
        var view = root.GetComponent<ConversationHistoryView>();
        if (view == null) return;
        var so = new SerializedObject(view);

        so.FindProperty("scrollRect").objectReferenceValue = FindInChildren<ScrollRect>(root, "Canvas/ContentPanel/ChatScrollView");

        var content = FindChild(root, "Canvas/ContentPanel/ChatScrollView/Viewport/Content");
        so.FindProperty("contentContainer").objectReferenceValue = content != null ? content.transform : null;

        so.FindProperty("closeButton").objectReferenceValue = FindInChildren<Button>(root, "Canvas/ContentPanel/Header/CloseButton");

        var chatBubblePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/BattleScene/UI/ChatBubble.prefab");
        so.FindProperty("chatBubblePrefab").objectReferenceValue = chatBubblePrefab;

        so.ApplyModifiedProperties();
        Debug.Log("[BattleSceneComposer] Wired ConversationHistoryView");
    }
}
