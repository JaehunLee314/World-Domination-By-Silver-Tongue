using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SilverTongue.Data;

namespace SilverTongue.DummyInfoGatheringScene
{
    public class DummyInfoGatheringView : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI itemListText;
        [SerializeField] private Button proceedButton;

        private DummyInfoGatheringSceneManager _manager;

        public void Initialize(DummyInfoGatheringSceneManager manager, ItemSO[] items)
        {
            _manager = manager;

            titleText.text = "Info Gathering Phase";

            string itemDisplay = "Items collected:\n\n";
            foreach (var item in items)
            {
                if (item != null)
                    itemDisplay += $"  - {item.itemName} ({item.itemType})\n";
            }
            itemListText.text = itemDisplay;

            proceedButton.onClick.RemoveAllListeners();
            proceedButton.onClick.AddListener(OnProceedClicked);
        }

        private void OnProceedClicked()
        {
            _manager.OnProceedToBattle();
        }
    }
}
