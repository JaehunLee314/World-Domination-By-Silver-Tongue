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

        [Header("Character Display")]
        [SerializeField] private Image playerImage;
        [SerializeField] private TextMeshProUGUI playerNameText;
        [SerializeField] private Image opponentImage;
        [SerializeField] private TextMeshProUGUI opponentNameText;
        [SerializeField] private TextMeshProUGUI playerLoseConditionsText;
        [SerializeField] private TextMeshProUGUI opponentLoseConditionsText;

        [Header("Agenda Slots")]
        [SerializeField] private AgendaSlotUI[] agendaSlots = new AgendaSlotUI[3];

        [Header("Inventory Panel")]
        [SerializeField] private Transform inventoryGrid;
        [SerializeField] private GameObject inventoryItemPrefab;
        [SerializeField] private Button filterAllButton;
        [SerializeField] private Button filterSkillButton;
        [SerializeField] private Button filterItemButton;

        [Header("Action")]
        [SerializeField] private Button confirmStrategyButton;

        private BattleSceneManager _manager;
        private bool _showPlayerConditions;
        private bool _showOpponentConditions;

        public void Initialize(BattleSceneManager manager)
        {
            _manager = manager;
            _showPlayerConditions = false;
            _showOpponentConditions = false;

            SetupCharacterDisplay();
            UpdateTurnCounter();
            PopulateInventory();
            SetupButtons();
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

            playerLoseConditionsText.text = "";
            opponentLoseConditionsText.text = "";
            playerLoseConditionsText.gameObject.SetActive(false);
            opponentLoseConditionsText.gameObject.SetActive(false);
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

            filterAllButton.onClick.RemoveAllListeners();
            filterAllButton.onClick.AddListener(() => FilterInventory(null));
            filterSkillButton.onClick.RemoveAllListeners();
            filterSkillButton.onClick.AddListener(() => FilterInventory(ItemType.SkillBook));
            filterItemButton.onClick.RemoveAllListeners();
            filterItemButton.onClick.AddListener(() => FilterInventory(ItemType.Evidence));
        }

        public void OnPlayerCharacterClicked()
        {
            _showPlayerConditions = !_showPlayerConditions;
            playerLoseConditionsText.gameObject.SetActive(_showPlayerConditions);
            if (_showPlayerConditions)
            {
                var conditions = _manager.SelectedBattler.loseConditions;
                string text = "";
                for (int i = 0; i < conditions.Length; i++)
                    text += $"- {conditions[i]}\n";
                playerLoseConditionsText.text = text.TrimEnd();
            }
        }

        public void OnOpponentCharacterClicked()
        {
            _showOpponentConditions = !_showOpponentConditions;
            opponentLoseConditionsText.gameObject.SetActive(_showOpponentConditions);
            if (_showOpponentConditions)
            {
                var conditions = _manager.Opponent.loseConditions;
                string text = "";
                for (int i = 0; i < conditions.Length; i++)
                    text += $"- {conditions[i]}\n";
                opponentLoseConditionsText.text = text.TrimEnd();
            }
        }

        private void PopulateInventory(ItemType? filter = null)
        {
            foreach (Transform child in inventoryGrid)
                Destroy(child.gameObject);

            if (GameManager.Instance == null) return;

            var items = filter.HasValue
                ? GameManager.Instance.GetItemsByType(filter.Value)
                : GameManager.Instance.playerItems;

            foreach (var item in items)
            {
                var itemObj = Instantiate(inventoryItemPrefab, inventoryGrid);
                var itemUI = itemObj.GetComponent<InventoryItemUI>();
                if (itemUI != null)
                    itemUI.Setup(item);
            }
        }

        private void FilterInventory(ItemType? type)
        {
            PopulateInventory(type);
        }

        private void OnBackToSelection()
        {
            _manager.ReturnToBattlerSelection();
        }

        private void OnConfirmStrategy()
        {
            _manager.OnStrategyConfirmed();
        }
    }
}
