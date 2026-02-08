using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SilverTongue.BattleScene
{
    public class BattleResultView : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI resultText;
        [SerializeField] private TextMeshProUGUI resultDescriptionText;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button conversationLogButton;

        private BattleSceneManager _manager;

        public void Initialize(BattleSceneManager manager, bool playerWon)
        {
            _manager = manager;

            resultText.text = playerWon ? "WIN" : "LOSE";
            resultText.color = playerWon
                ? new Color(0.973f, 0.961f, 0.275f) // Accent yellow
                : Color.red;

            string opponentName = manager.Opponent != null ? manager.Opponent.characterName : "OPPONENT";
            resultDescriptionText.text = playerWon
                ? $"CONFIRMED: {opponentName} BROKEN"
                : "CONTRACT SIGNED: YOU LOSE";

            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(OnCloseClicked);

            if (conversationLogButton != null)
            {
                conversationLogButton.onClick.RemoveAllListeners();
                conversationLogButton.onClick.AddListener(() => _manager.ShowConversationHistory());
            }
        }

        private void OnCloseClicked()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("DummyInfoGatheringScene");
        }
    }
}
