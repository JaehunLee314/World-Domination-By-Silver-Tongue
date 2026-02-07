using System.Collections.Generic;
using UnityEngine;
using TMPro;
using SilverTongue.Data;
using SilverTongue.BattleSystem;

namespace SilverTongue.BattleScene
{
    public class StrategyPanelUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private TMP_InputField strategyTextField;
        [SerializeField] private Transform itemSlotsContainer;

        [Header("Data")]
        public string strategyText;
        public List<ItemSO> assignedItems = new List<ItemSO>();

        private DropTarget[] _dropTargets;

        public void Setup()
        {
            strategyTextField.characterLimit = 500;
            strategyTextField.onValueChanged.RemoveAllListeners();
            strategyTextField.onValueChanged.AddListener(text => strategyText = text);

            _dropTargets = itemSlotsContainer.GetComponentsInChildren<DropTarget>();
            foreach (var dt in _dropTargets)
            {
                dt.OnItemDropped = OnItemDroppedToSlot;
                dt.OnItemRemoved = OnItemRemovedFromSlot;
                dt.CanAcceptItem = item => !assignedItems.Contains(item);
            }

            ClearAll();
        }

        private void OnItemDroppedToSlot(ItemSO item)
        {
            if (!assignedItems.Contains(item))
                assignedItems.Add(item);
        }

        private void OnItemRemovedFromSlot(ItemSO item)
        {
            assignedItems.Remove(item);
        }

        public void ClearAll()
        {
            strategyText = "";
            strategyTextField.text = "";
            assignedItems.Clear();
            if (_dropTargets != null)
            {
                foreach (var dt in _dropTargets)
                    dt.Clear();
            }
        }

        public StrategyEntry CollectStrategy()
        {
            return new StrategyEntry
            {
                StrategyText = strategyText,
                Items = new List<ItemSO>(assignedItems)
            };
        }
    }
}
