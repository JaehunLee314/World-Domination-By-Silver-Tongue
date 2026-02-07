using System;
using UnityEngine;
using UnityEngine.UI;

namespace InfoGatherPhase
{
    public class BattleDecisionUI : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Button confrontButton;
        [SerializeField] private Button declineButton;

        public event Action OnConfront;
        public event Action OnDecline;

        public bool IsOpen { get; private set; }

        private void Awake()
        {
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
    }
}
