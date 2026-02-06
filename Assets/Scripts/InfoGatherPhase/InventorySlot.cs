using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace InfoGatherPhase
{
    public class InventorySlot : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI nameLabel;
        [SerializeField] private Button slotButton;

        private ItemData itemData;
        private Action<ItemData> onClick;

        public void Setup(ItemData data, Action<ItemData> clickCallback)
        {
            itemData = data;
            onClick = clickCallback;

            iconImage.sprite = data.icon;
            nameLabel.text = data.itemName;

            slotButton.onClick.RemoveAllListeners();
            slotButton.onClick.AddListener(() => onClick?.Invoke(itemData));
        }
    }
}
