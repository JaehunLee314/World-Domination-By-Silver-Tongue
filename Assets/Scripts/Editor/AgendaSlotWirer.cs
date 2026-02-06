using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using SilverTongue.BattleScene;

public class AgendaSlotWirer
{
    [MenuItem("Tools/Wire Agenda Slots")]
    public static void WireAgendaSlots()
    {
        // Wire agenda slots in the scene
        var strategyObj = GameObject.Find("StrategySelectingCanvas");
        if (strategyObj == null)
        {
            Debug.LogError("[AgendaSlotWirer] StrategySelectingCanvas not found in scene");
            return;
        }

        for (int i = 1; i <= 3; i++)
        {
            var slotTransform = strategyObj.transform.Find($"Canvas/BottomSection/AgendaPanel/AgendaSlot{i}");
            if (slotTransform == null)
            {
                Debug.LogWarning($"[AgendaSlotWirer] AgendaSlot{i} not found");
                continue;
            }

            var slot = slotTransform.GetComponent<AgendaSlotUI>();
            if (slot == null) continue;

            var so = new SerializedObject(slot);

            so.FindProperty("slotNumberText").objectReferenceValue = slotTransform.Find("SlotNumberText")?.GetComponent<TextMeshProUGUI>();
            so.FindProperty("pointTextField").objectReferenceValue = slotTransform.Find("PointTextField")?.GetComponent<TMP_InputField>();
            so.FindProperty("skillSlotImage").objectReferenceValue = slotTransform.Find("SkillSlot/SkillSlotImage")?.GetComponent<Image>();
            so.FindProperty("itemSlotImage").objectReferenceValue = slotTransform.Find("ItemSlot/ItemSlotImage")?.GetComponent<Image>();
            so.FindProperty("skillSlotLabel").objectReferenceValue = slotTransform.Find("SkillSlot/SkillSlotLabel")?.GetComponent<TextMeshProUGUI>();
            so.FindProperty("itemSlotLabel").objectReferenceValue = slotTransform.Find("ItemSlot/ItemSlotLabel")?.GetComponent<TextMeshProUGUI>();

            so.ApplyModifiedProperties();
            Debug.Log($"[AgendaSlotWirer] Wired AgendaSlot{i}");
        }

        // Also wire the CharacterCard prefab's serialized fields
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

        // Save
        var activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(activeScene);
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(activeScene);
        AssetDatabase.SaveAssets();

        Debug.Log("[AgendaSlotWirer] All slots and prefabs wired successfully!");
    }
}
