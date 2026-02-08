using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SilverTongue.Data;

namespace SilverTongue.BattleScene
{
    public class ItemDetailPopup : MonoBehaviour
    {
        [SerializeField] private Image detailIcon;
        [SerializeField] private TextMeshProUGUI detailName;
        [SerializeField] private TextMeshProUGUI detailType;
        [SerializeField] private TextMeshProUGUI detailDescription;
        [SerializeField] private Button closeButton;

        private RectTransform _rectTransform;
        private Canvas _parentCanvas;

        public static ItemDetailPopup Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
            _rectTransform = GetComponent<RectTransform>();
            _parentCanvas = GetComponentInParent<Canvas>();

            if (closeButton != null)
                closeButton.onClick.AddListener(Hide);

            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void Show(ItemSO item, RectTransform anchorRect)
        {
            if (item == null) return;

            if (item.itemImage != null)
                detailIcon.sprite = item.itemImage;

            detailName.text = item.itemName;
            detailType.text = "Item";
            detailDescription.text = item.description;

            PositionNear(anchorRect);
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void PositionNear(RectTransform anchorRect)
        {
            if (anchorRect == null || _parentCanvas == null) return;

            // Convert anchor world position to local position in popup's parent
            var parentRect = _rectTransform.parent as RectTransform;
            if (parentRect == null) return;

            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect,
                RectTransformUtility.WorldToScreenPoint(_parentCanvas.worldCamera, anchorRect.position),
                _parentCanvas.worldCamera,
                out localPoint);

            // Offset above the item
            localPoint.y += anchorRect.rect.height * 0.5f + _rectTransform.rect.height * 0.5f + 10f;

            _rectTransform.localPosition = localPoint;
        }
    }
}
