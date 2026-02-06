using UnityEngine;
using UnityEngine.EventSystems;

namespace SilverTongue.BattleScene
{
    public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private Canvas _rootCanvas;
        private RectTransform _rectTransform;
        private CanvasGroup _canvasGroup;
        private Vector3 _originalPosition;
        private Transform _originalParent;
        private int _originalSiblingIndex;

        public InventoryItemUI InventoryItemUI { get; set; }

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _originalPosition = _rectTransform.position;
            _originalParent = transform.parent;
            _originalSiblingIndex = transform.GetSiblingIndex();

            _rootCanvas = GetComponentInParent<Canvas>().rootCanvas;
            transform.SetParent(_rootCanvas.transform, true);

            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.alpha = 0.7f;
        }

        public void OnDrag(PointerEventData eventData)
        {
            _rectTransform.position = eventData.position;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _canvasGroup.blocksRaycasts = true;
            _canvasGroup.alpha = 1f;

            transform.SetParent(_originalParent, false);
            transform.SetSiblingIndex(_originalSiblingIndex);
            _rectTransform.position = _originalPosition;
        }
    }
}
