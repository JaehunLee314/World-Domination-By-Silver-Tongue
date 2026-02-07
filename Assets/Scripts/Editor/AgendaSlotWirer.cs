using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using SilverTongue.BattleScene;

public class AgendaSlotWirer
{
    [MenuItem("Tools/Wire Strategy Panel")]
    public static void WireStrategyPanel()
    {
        // Wire strategy panel in the scene
        var strategyObj = GameObject.Find("StrategySelectingCanvas");
        if (strategyObj == null)
        {
            Debug.LogError("[AgendaSlotWirer] StrategySelectingCanvas not found in scene");
            return;
        }

        // Wire StrategyPanelUI
        var panelTransform = strategyObj.transform.Find("Canvas/BottomSection/StrategyPanel");
        if (panelTransform != null)
        {
            var panel = panelTransform.GetComponent<StrategyPanelUI>();
            if (panel != null)
            {
                var panelSo = new SerializedObject(panel);
                panelSo.FindProperty("strategyTextField").objectReferenceValue = panelTransform.Find("StrategyTextField")?.GetComponent<TMP_InputField>();
                panelSo.FindProperty("itemSlotsContainer").objectReferenceValue = panelTransform.Find("ItemSlotsContainer");
                panelSo.ApplyModifiedProperties();
                Debug.Log("[AgendaSlotWirer] Wired StrategyPanelUI");
            }
        }
        else
        {
            Debug.LogWarning("[AgendaSlotWirer] StrategyPanel not found");
        }

        // Wire the CharacterCard prefab's serialized fields
        string cardPath = "Assets/Prefabs/BattleScene/UI/CharacterCard.prefab";
        var cardPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(cardPath);
        if (cardPrefab != null)
        {
            var cardUI = cardPrefab.GetComponent<CharacterCardUI>();
            if (cardUI != null)
            {
                var cardSo = new SerializedObject(cardUI);
                cardSo.FindProperty("profileImage").objectReferenceValue = cardPrefab.transform.Find("ProfileImage")?.GetComponent<Image>();
                cardSo.FindProperty("nameText").objectReferenceValue = cardPrefab.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
                cardSo.FindProperty("personalityText").objectReferenceValue = cardPrefab.transform.Find("PersonalityText")?.GetComponent<TextMeshProUGUI>();
                cardSo.FindProperty("intelligenceText").objectReferenceValue = cardPrefab.transform.Find("IntelligenceText")?.GetComponent<TextMeshProUGUI>();
                cardSo.FindProperty("skillsText").objectReferenceValue = cardPrefab.transform.Find("SkillsText")?.GetComponent<TextMeshProUGUI>();
                cardSo.FindProperty("loseConditionsText").objectReferenceValue = cardPrefab.transform.Find("LoseConditionsText")?.GetComponent<TextMeshProUGUI>();
                cardSo.FindProperty("selectButton").objectReferenceValue = cardPrefab.transform.Find("SelectButton")?.GetComponent<Button>();
                cardSo.ApplyModifiedProperties();
                Debug.Log("[AgendaSlotWirer] Wired CharacterCard prefab");
            }
        }

        // Wire InventoryItem prefab
        string invPath = "Assets/Prefabs/BattleScene/UI/InventoryItem.prefab";
        var invPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(invPath);
        if (invPrefab != null)
        {
            var invUI = invPrefab.GetComponent<InventoryItemUI>();
            if (invUI != null)
            {
                var invSo = new SerializedObject(invUI);
                invSo.FindProperty("itemImage").objectReferenceValue = invPrefab.transform.Find("ItemImage")?.GetComponent<Image>();
                invSo.FindProperty("itemNameText").objectReferenceValue = invPrefab.transform.Find("ItemNameText")?.GetComponent<TextMeshProUGUI>();
                invSo.ApplyModifiedProperties();
                Debug.Log("[AgendaSlotWirer] Wired InventoryItem prefab");
            }
        }

        // Wire ChatBubble prefab
        string chatBubblePath = "Assets/Prefabs/BattleScene/UI/ChatBubble.prefab";
        var chatBubblePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(chatBubblePath);
        if (chatBubblePrefab != null)
        {
            var bubbleUI = chatBubblePrefab.GetComponent<ChatBubbleUI>();
            if (bubbleUI != null)
            {
                var bubbleSo = new SerializedObject(bubbleUI);
                bubbleSo.FindProperty("speakerNameText").objectReferenceValue = chatBubblePrefab.transform.Find("BubbleBackground/SpeakerNameText")?.GetComponent<TextMeshProUGUI>();
                bubbleSo.FindProperty("speechText").objectReferenceValue = chatBubblePrefab.transform.Find("BubbleBackground/SpeechText")?.GetComponent<TextMeshProUGUI>();
                bubbleSo.FindProperty("timestampText").objectReferenceValue = chatBubblePrefab.transform.Find("BubbleBackground/TimestampText")?.GetComponent<TextMeshProUGUI>();
                bubbleSo.FindProperty("indicatorContainer").objectReferenceValue = chatBubblePrefab.transform.Find("BubbleBackground/IndicatorContainer")?.gameObject;
                bubbleSo.FindProperty("indicatorText").objectReferenceValue = chatBubblePrefab.transform.Find("BubbleBackground/IndicatorContainer/IndicatorText")?.GetComponent<TextMeshProUGUI>();
                bubbleSo.FindProperty("rootLayout").objectReferenceValue = chatBubblePrefab.GetComponent<HorizontalLayoutGroup>();
                bubbleSo.FindProperty("bubbleBackground").objectReferenceValue = chatBubblePrefab.transform.Find("BubbleBackground")?.GetComponent<Image>();
                bubbleSo.ApplyModifiedProperties();
                Debug.Log("[AgendaSlotWirer] Wired ChatBubble prefab");
            }
        }

        // Save
        var activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(activeScene);
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(activeScene);
        AssetDatabase.SaveAssets();

        Debug.Log("[AgendaSlotWirer] All panels and prefabs wired successfully!");
    }
}
