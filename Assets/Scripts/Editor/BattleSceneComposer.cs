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
        var kai = AssetDatabase.LoadAssetAtPath<CharacterSO>("Assets/ScriptableObjects/Characters/Protagonist_Kai.asset");
        var reina = AssetDatabase.LoadAssetAtPath<CharacterSO>("Assets/ScriptableObjects/Characters/Rival_Reina.asset");
        var zarvoth = AssetDatabase.LoadAssetAtPath<CharacterSO>("Assets/ScriptableObjects/Characters/Boss_DemonKing.asset");

        var so = new SerializedObject(gm);
        var charProp = so.FindProperty("availableCharacters");
        charProp.arraySize = 3;
        charProp.GetArrayElementAtIndex(0).objectReferenceValue = kai;
        charProp.GetArrayElementAtIndex(1).objectReferenceValue = reina;
        charProp.GetArrayElementAtIndex(2).objectReferenceValue = zarvoth;

        // Load item SOs
        var diary = AssetDatabase.LoadAssetAtPath<ItemSO>("Assets/ScriptableObjects/Items/Evidence_SecretDiary.asset");
        var photo = AssetDatabase.LoadAssetAtPath<ItemSO>("Assets/ScriptableObjects/Items/Evidence_PhotoAlbum.asset");
        var ramen = AssetDatabase.LoadAssetAtPath<ItemSO>("Assets/ScriptableObjects/Items/Evidence_RamenRecipe.asset");
        var tsundere = AssetDatabase.LoadAssetAtPath<ItemSO>("Assets/ScriptableObjects/Items/Skill_TsundereLogic.asset");
        var logic = AssetDatabase.LoadAssetAtPath<ItemSO>("Assets/ScriptableObjects/Items/Skill_LogicalDeduction.asset");

        var itemsProp = so.FindProperty("playerItems");
        itemsProp.arraySize = 5;
        itemsProp.GetArrayElementAtIndex(0).objectReferenceValue = diary;
        itemsProp.GetArrayElementAtIndex(1).objectReferenceValue = photo;
        itemsProp.GetArrayElementAtIndex(2).objectReferenceValue = ramen;
        itemsProp.GetArrayElementAtIndex(3).objectReferenceValue = tsundere;
        itemsProp.GetArrayElementAtIndex(4).objectReferenceValue = logic;
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
        // The first canvas has EventSystem, remove from others
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

        // Set canvas references (the root GameObjects that get activated/deactivated)
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

        // Set opponent (default to Demon King)
        bsmSo.FindProperty("opponent").objectReferenceValue = zarvoth;
        bsmSo.FindProperty("maxTurns").intValue = 7;
        bsmSo.FindProperty("useMockLLM").boolValue = true;

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
        // Destroy ALL root objects with this name (including inactive ones)
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

        // Character card prefab
        var cardPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/BattleScene/UI/CharacterCard.prefab");
        so.FindProperty("characterCardPrefab").objectReferenceValue = cardPrefab;

        // Container = ScrollView/Viewport/Content
        var content = FindChild(root, "Canvas/ScrollView/Viewport/Content");
        so.FindProperty("characterCardContainer").objectReferenceValue = content != null ? content.transform : null;

        // ScrollRect reference
        so.FindProperty("scrollRect").objectReferenceValue = FindInChildren<ScrollRect>(root, "Canvas/ScrollView");

        // Confirmation popup
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

        // Agenda slots
        var agendaProp = so.FindProperty("agendaSlots");
        agendaProp.arraySize = 3;
        for (int i = 0; i < 3; i++)
        {
            var slot = FindInChildren<AgendaSlotUI>(root, $"Canvas/BottomSection/AgendaPanel/AgendaSlot{i + 1}");
            agendaProp.GetArrayElementAtIndex(i).objectReferenceValue = slot;
        }

        // Inventory
        var invItemPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/BattleScene/UI/InventoryItem.prefab");
        so.FindProperty("inventoryItemPrefab").objectReferenceValue = invItemPrefab;

        var invGrid = FindChild(root, "Canvas/BottomSection/InventoryPanel/InventoryScroll/Viewport/Content");
        so.FindProperty("inventoryGrid").objectReferenceValue = invGrid != null ? invGrid.transform : null;

        so.FindProperty("filterAllButton").objectReferenceValue = FindInChildren<Button>(root, "Canvas/BottomSection/InventoryPanel/FilterRow/FilterAllButton");
        so.FindProperty("filterSkillButton").objectReferenceValue = FindInChildren<Button>(root, "Canvas/BottomSection/InventoryPanel/FilterRow/FilterSkillButton");
        so.FindProperty("filterItemButton").objectReferenceValue = FindInChildren<Button>(root, "Canvas/BottomSection/InventoryPanel/FilterRow/FilterItemButton");

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

        // Top UI
        so.FindProperty("turnTrackerText").objectReferenceValue = FindInChildren<TMPro.TextMeshProUGUI>(root, "Canvas/TopUI/TurnTrackerText");
        so.FindProperty("autoProgressButton").objectReferenceValue = FindInChildren<Button>(root, "Canvas/DialogueArea/AutoProgressButton");
        so.FindProperty("autoProgressButtonText").objectReferenceValue = FindInChildren<TMPro.TextMeshProUGUI>(root, "Canvas/DialogueArea/AutoProgressButton/Text");
        so.FindProperty("goToStrategyButton").objectReferenceValue = FindInChildren<Button>(root, "Canvas/TopUI/GoToStrategyButton");
        so.FindProperty("logButton").objectReferenceValue = FindInChildren<Button>(root, "Canvas/TopUI/LogButton");

        // Characters
        so.FindProperty("playerCharacterImage").objectReferenceValue = FindInChildren<Image>(root, "Canvas/StageArea/PlayerCharacterPanel/PlayerCharacterImage");
        so.FindProperty("opponentCharacterImage").objectReferenceValue = FindInChildren<Image>(root, "Canvas/StageArea/OpponentCharacterPanel/OpponentCharacterImage");
        so.FindProperty("playerNameLabel").objectReferenceValue = FindInChildren<TMPro.TextMeshProUGUI>(root, "Canvas/StageArea/PlayerCharacterPanel/PlayerNameLabel");
        so.FindProperty("opponentNameLabel").objectReferenceValue = FindInChildren<TMPro.TextMeshProUGUI>(root, "Canvas/StageArea/OpponentCharacterPanel/OpponentNameLabel");

        // Stage Animation (panel RectTransforms for smooth movement)
        var playerPanelTr = root.transform.Find("Canvas/StageArea/PlayerCharacterPanel");
        so.FindProperty("playerPanel").objectReferenceValue = playerPanelTr != null ? playerPanelTr.GetComponent<RectTransform>() : null;
        var opponentPanelTr = root.transform.Find("Canvas/StageArea/OpponentCharacterPanel");
        so.FindProperty("opponentPanel").objectReferenceValue = opponentPanelTr != null ? opponentPanelTr.GetComponent<RectTransform>() : null;

        // Dialogue
        so.FindProperty("speakerNameText").objectReferenceValue = FindInChildren<TMPro.TextMeshProUGUI>(root, "Canvas/DialogueArea/SpeakerNameText");
        so.FindProperty("dialogueText").objectReferenceValue = FindInChildren<TMPro.TextMeshProUGUI>(root, "Canvas/DialogueArea/DialogueText");
        so.FindProperty("thoughtText").objectReferenceValue = FindInChildren<TMPro.TextMeshProUGUI>(root, "Canvas/DialogueArea/ThoughtText");
        so.FindProperty("dialogueAreaButton").objectReferenceValue = FindInChildren<Button>(root, "Canvas/DialogueArea");

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
