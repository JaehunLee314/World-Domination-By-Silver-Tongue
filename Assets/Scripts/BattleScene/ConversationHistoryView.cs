using UnityEngine;
using UnityEngine.UI;

namespace SilverTongue.BattleScene
{
    public class ConversationHistoryView : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private Transform contentContainer;
        [SerializeField] private Button closeButton;
        [SerializeField] private GameObject chatBubblePrefab;

        private BattleSceneManager _manager;

        public void Initialize(BattleSceneManager manager)
        {
            _manager = manager;
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(OnClose);
            PopulateHistory();
        }

        private void PopulateHistory()
        {
            foreach (Transform child in contentContainer)
                Destroy(child.gameObject);

            foreach (var entry in _manager.ConversationHistory)
            {
                var bubble = Instantiate(chatBubblePrefab, contentContainer);
                var bubbleUI = bubble.GetComponent<ChatBubbleUI>();
                if (bubbleUI != null)
                    bubbleUI.Setup(entry);
            }

            StartCoroutine(ScrollToBottom());
        }

        private System.Collections.IEnumerator ScrollToBottom()
        {
            yield return null;
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0f;
        }

        private void OnClose()
        {
            _manager.HideConversationHistory();
        }
    }
}
