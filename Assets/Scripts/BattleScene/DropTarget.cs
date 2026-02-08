using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using SilverTongue.Data;

namespace SilverTongue.BattleScene
{
    public class DropTarget : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        private Image _slotImage;
        private TextMeshProUGUI _slotLabel;
        private Color _originalColor;
        private Color _emptyImageColor;

        public System.Action<ItemSO> OnItemDropped;
        public System.Action<ItemSO> OnItemRemoved;
        public System.Func<ItemSO, bool> CanAcceptItem;
        public ItemSO AssignedItem { get; private set; }

        private void Awake()
        {
            // The slot's own Image is the raycast target/background
            _slotImage = GetComponent<Image>();
            _originalColor = _slotImage != null ? _slotImage.color : Color.white;

            // Child SlotImage shows the item sprite
            var childImg = transform.Find("SlotImage");
            if (childImg != null)
            {
                var img = childImg.GetComponent<Image>();
                _emptyImageColor = img != null ? img.color : Color.white;
            }

            // Child SlotLabel shows item name
            var childLabel = transform.Find("SlotLabel");
            if (childLabel != null)
                _slotLabel = childLabel.GetComponent<TextMeshProUGUI>();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (eventData.pointerDrag == null) return;
            var draggable = eventData.pointerDrag.GetComponent<DraggableItem>();
            if (draggable == null || draggable.InventoryItemUI == null) return;
            if (_slotImage != null)
                _slotImage.color = new Color(0.3f, 1f, 0.3f, 0.7f);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_slotImage != null)
                _slotImage.color = _originalColor;
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (_slotImage != null)
                _slotImage.color = _originalColor;
            var draggable = eventData.pointerDrag?.GetComponent<DraggableItem>();
            if (draggable == null || draggable.InventoryItemUI == null) return;

            var item = draggable.InventoryItemUI.Item;
            if (CanAcceptItem != null && !CanAcceptItem(item)) return;

            if (AssignedItem != null)
                OnItemRemoved?.Invoke(AssignedItem);

            AssignedItem = item;
            UpdateVisuals(item);
            OnItemDropped?.Invoke(item);
        }

        private void UpdateVisuals(ItemSO item)
        {
            // Update child SlotImage with item sprite
            var childImg = transform.Find("SlotImage");
            if (childImg != null)
            {
                var img = childImg.GetComponent<Image>();
                if (img != null && item.itemImage != null)
                {
                    img.sprite = item.itemImage;
                    img.color = Color.white;
                }
            }

            // Update child SlotLabel with item name
            if (_slotLabel != null)
                _slotLabel.text = item.itemName;

        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (AssignedItem == null) return;
            var removed = AssignedItem;
            Clear();
            OnItemRemoved?.Invoke(removed);
        }

        public void Clear()
        {
            AssignedItem = null;

            var childImg = transform.Find("SlotImage");
            if (childImg != null)
            {
                var img = childImg.GetComponent<Image>();
                if (img != null)
                {
                    img.sprite = null;
                    img.color = _emptyImageColor;
                }
            }

            if (_slotLabel != null)
                _slotLabel.text = "Item";

            if (_slotImage != null)
                _slotImage.color = _originalColor;
        }
    }
}
