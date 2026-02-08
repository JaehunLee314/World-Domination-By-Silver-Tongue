using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using SilverTongue.Data;

namespace SilverTongue.BattleScene
{
    public class InventoryItemUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
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

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_item == null || ItemDetailPopup.Instance == null) return;
            ItemDetailPopup.Instance.Show(_item, GetComponent<RectTransform>());
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (ItemDetailPopup.Instance == null) return;
            ItemDetailPopup.Instance.Hide();
        }
    }
}
