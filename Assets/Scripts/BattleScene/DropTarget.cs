using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using SilverTongue.Data;

namespace SilverTongue.BattleScene
{
    public enum DropSlotType { Skill, Item }

    public class DropTarget : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private DropSlotType slotType;

        private Image _slotImage;
        private Color _originalColor;
        private AgendaSlotUI _parentSlot;

        private void Awake()
        {
            _slotImage = GetComponentInChildren<Image>();
            _originalColor = _slotImage != null ? _slotImage.color : Color.white;
            _parentSlot = GetComponentInParent<AgendaSlotUI>();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (eventData.pointerDrag == null) return;
            var draggable = eventData.pointerDrag.GetComponent<DraggableItem>();
            if (draggable == null || draggable.InventoryItemUI == null) return;

            var item = draggable.InventoryItemUI.Item;
            bool valid = IsValidDrop(item);
            _slotImage.color = valid
                ? new Color(0.3f, 1f, 0.3f, 0.7f)
                : new Color(1f, 0.3f, 0.3f, 0.7f);
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
            if (!IsValidDrop(item)) return;

            if (slotType == DropSlotType.Skill)
                _parentSlot.AssignSkill(item);
            else
                _parentSlot.AssignItem(item);
        }

        private bool IsValidDrop(ItemSO item)
        {
            return (slotType == DropSlotType.Skill && item.itemType == ItemType.SkillBook)
                || (slotType == DropSlotType.Item && item.itemType == ItemType.Evidence);
        }
    }
}
