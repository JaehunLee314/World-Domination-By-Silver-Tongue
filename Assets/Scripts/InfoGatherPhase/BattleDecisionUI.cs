using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace InfoGatherPhase
{
    public class BattleDecisionUI : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Button confrontButton;
        [SerializeField] private Button declineButton;
        [SerializeField] private TextMeshProUGUI declineLabel;

        public event Action OnConfront;
        public event Action OnDecline;

        public bool IsOpen { get; private set; }

        private int totalStageItems;

        private void Awake()
        {
            totalStageItems = FindObjectsByType<ClickableItem>(FindObjectsSortMode.None).Length;
            confrontButton.onClick.AddListener(HandleConfront);
            declineButton.onClick.AddListener(HandleDecline);
            panel.SetActive(false);
        }

        private void OnDestroy()
        {
            confrontButton.onClick.RemoveListener(HandleConfront);
            declineButton.onClick.RemoveListener(HandleDecline);
        }

        public void Show()
        {
            UpdateDeclineLabel();
            panel.SetActive(true);
            IsOpen = true;
        }

        public void Hide()
        {
            panel.SetActive(false);
            IsOpen = false;
        }

        private void HandleConfront()
        {
            Hide();
            OnConfront?.Invoke();
        }

        private void HandleDecline()
        {
            Hide();
            OnDecline?.Invoke();
        }

        private void UpdateDeclineLabel()
        {
            if (declineLabel == null) return;

            int collected = GameManager.Instance != null
                ? GameManager.Instance.CollectedItems.Count
                : 0;

            bool allCollected = collected >= totalStageItems;

            declineLabel.text = allCollected
                ? $"Not yet... ({collected}/{totalStageItems} found)"
                : $"Not yet...\n<color=red>You haven't found all items! ({collected}/{totalStageItems} found)</color>";
        }
    }
}
