using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SilverTongue.Data;

namespace SilverTongue.BattleScene
{
    public class InventoryItemUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Image itemImage;
        [SerializeField] private TextMeshProUGUI itemNameText;

        private ItemSO _item;

        public ItemSO Item => _item;

        public void Setup(ItemSO item)
        {
            _item = item;
            itemNameText.text = item.itemName;
            if (item.itemImage != null)
                itemImage.sprite = item.itemImage;

            var draggable = GetComponent<DraggableItem>();
            if (draggable != null)
                draggable.InventoryItemUI = this;
        }
    }
}
