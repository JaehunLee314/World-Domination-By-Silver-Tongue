using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SilverTongue.Data;

namespace SilverTongue.BattleScene
{
    public class StrategySelectingView : MonoBehaviour
    {
        [Header("Top Bar")]
        [SerializeField] private Button backToSelectionButton;
        [SerializeField] private TextMeshProUGUI turnCounterText;
        [SerializeField] private Button logButton;

        [Header("Character Display")]
        [SerializeField] private Image playerImage;
        [SerializeField] private TextMeshProUGUI playerNameText;
        [SerializeField] private Image opponentImage;
        [SerializeField] private TextMeshProUGUI opponentNameText;
        [SerializeField] private TextMeshProUGUI playerLoseConditionsText;
        [SerializeField] private TextMeshProUGUI opponentLoseConditionsText;

        [Header("Strategy Panel")]
        [SerializeField] private StrategyPanelUI strategyPanel;

        [Header("Inventory Panel")]
        [SerializeField] private InventoryView inventoryView;

        [Header("Action")]
        [SerializeField] private Button confirmStrategyButton;

        private BattleSceneManager _manager;

        public void Initialize(BattleSceneManager manager)
        {
            _manager = manager;

            SetupCharacterDisplay();
            UpdateTurnCounter();
            PopulateInventory();
            SetupButtons();
            strategyPanel.Setup();
        }

        private void SetupCharacterDisplay()
        {
            var player = _manager.SelectedBattler;
            var opponent = _manager.Opponent;

            playerNameText.text = player.characterName;
            opponentNameText.text = opponent.characterName;

            if (player.profileImage != null)
                playerImage.sprite = player.profileImage;
            if (opponent.profileImage != null)
                opponentImage.sprite = opponent.profileImage;

            // Always show lose conditions
            playerLoseConditionsText.text = FormatLoseConditions(player.loseConditions);
            opponentLoseConditionsText.text = FormatLoseConditions(opponent.loseConditions);
        }

        private string FormatLoseConditions(string[] conditions)
        {
            if (conditions == null || conditions.Length == 0) return "";
            string text = "<b>Must-Lose Conditions:</b>\n";
            foreach (var cond in conditions)
                text += $"- {cond}\n";
            return text.TrimEnd();
        }

        private void UpdateTurnCounter()
        {
            turnCounterText.text = $"Turn {_manager.CurrentTurn}/{_manager.MaxTurns}";
        }

        private void SetupButtons()
        {
            backToSelectionButton.onClick.RemoveAllListeners();
            backToSelectionButton.onClick.AddListener(OnBackToSelection);

            confirmStrategyButton.onClick.RemoveAllListeners();
            confirmStrategyButton.onClick.AddListener(OnConfirmStrategy);

            if (logButton != null)
            {
                logButton.onClick.RemoveAllListeners();
                logButton.onClick.AddListener(() => _manager.ShowConversationHistory());
            }

            backToSelectionButton.interactable = !_manager.IsPausedFromBattle;

            var confirmLabel = confirmStrategyButton.GetComponentInChildren<TextMeshProUGUI>();
            if (confirmLabel != null)
                confirmLabel.text = _manager.IsPausedFromBattle ? "RESUME BATTLE" : "CONFIRM STRATEGY";
        }

        private void PopulateInventory()
        {
            if (inventoryView != null)
                inventoryView.PopulateFromGameManager();
        }

        private void OnBackToSelection()
        {
            _manager.ReturnToBattlerSelection();
        }

        private void CollectAndSetStrategy()
        {
            _manager.SetStrategy(strategyPanel.CollectStrategy());
        }

        private void OnConfirmStrategy()
        {
            CollectAndSetStrategy();

            if (_manager.IsPausedFromBattle)
                _manager.ResumeFromStrategy();
            else
                _manager.OnStrategyConfirmed();
        }
    }
}
