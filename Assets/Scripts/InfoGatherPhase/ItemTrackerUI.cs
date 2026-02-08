using UnityEngine;
using TMPro;

namespace InfoGatherPhase
{
    public class ItemTrackerUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI itemCountText;

        private void Start()
        {
            UpdateText();

            if (GameManager.Instance != null)
                GameManager.Instance.OnItemCollected += OnItemCollected;
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnItemCollected -= OnItemCollected;
        }

        private void OnItemCollected(ItemData item)
        {
            UpdateText();
        }

        private void UpdateText()
        {
            int count = GameManager.Instance != null ? GameManager.Instance.CollectedItems.Count : 0;
            itemCountText.text = $"{count}/8 ITEMS FOUND";
        }
    }
}
