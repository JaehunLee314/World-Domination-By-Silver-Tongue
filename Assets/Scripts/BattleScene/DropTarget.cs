using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using SilverTongue.Data;

namespace SilverTongue.BattleScene
{
    public class DropTarget : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
    {
        private Image _slotImage;
        private Color _originalColor;

        public System.Action<ItemSO> OnItemDropped;
        public ItemSO AssignedItem { get; private set; }

        private void Awake()
        {
            _slotImage = GetComponentInChildren<Image>();
            _originalColor = _slotImage != null ? _slotImage.color : Color.white;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (eventData.pointerDrag == null) return;
            var draggable = eventData.pointerDrag.GetComponent<DraggableItem>();
            if (draggable == null || draggable.InventoryItemUI == null) return;
            _slotImage.color = new Color(0.3f, 1f, 0.3f, 0.7f);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _slotImage.color = _originalColor;
        }

        public void OnDrop(PointerEventData eventData)
        {
            _slotImage.color = _originalColor;
            var draggable = eventData.pointerDrag?.GetComponent<DraggableItem>();
            if (draggable == null || draggable.InventoryItemUI == null) return;

            var item = draggable.InventoryItemUI.Item;
            AssignedItem = item;
            if (_slotImage != null)
                _slotImage.color = Color.white;
            OnItemDropped?.Invoke(item);
        }

        public void Clear()
        {
            AssignedItem = null;
            if (_slotImage != null)
                _slotImage.color = _originalColor;
        }
    }
}
