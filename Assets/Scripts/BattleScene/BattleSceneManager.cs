using System.Collections.Generic;
using UnityEngine;
using SilverTongue.Data;
using SilverTongue.LLM;

namespace SilverTongue.BattleScene
{
    public enum BattlePhase
    {
        BattlerSelecting,
        StrategySelecting,
        Battle,
        BattleResult
    }

    public struct StrategyEntry
    {
        public string StrategyText;
        public List<ItemSO> Items;
    }

    public class BattleSceneManager : MonoBehaviour
    {
        [Header("Canvas References")]
        [SerializeField] private GameObject battlerSelectingCanvas;
        [SerializeField] private GameObject strategySelectingCanvas;
        [SerializeField] private GameObject battleCanvas;
        [SerializeField] private GameObject battleResultCanvas;

        [Header("Overlay References")]
        [SerializeField] private GameObject conversationHistoryCanvas;
        [SerializeField] private ConversationHistoryView conversationHistoryView;

        [Header("View References")]
        [SerializeField] private BattlerSelectingView battlerSelectingView;
        [SerializeField] private StrategySelectingView strategySelectingView;
        [SerializeField] private BattleView battleView;
        [SerializeField] private BattleResultView battleResultView;

        [Header("Battle Config")]
        [SerializeField] private CharacterSO opponent;
        [SerializeField] private int maxTurns = 7;
        [SerializeField] private bool useMockLLM = true;

        private BattlePhase _currentPhase;
        private ILLMService _llmService;
        private int _currentTurn;
        private bool _isPausedFromBattle;
        private readonly List<ConversationEntry> _conversationHistory = new List<ConversationEntry>();
        private StrategyEntry _strategy;

        public CharacterSO SelectedBattler { get; private set; }
        public CharacterSO Opponent => opponent;
        public int CurrentTurn => _currentTurn;
        public int MaxTurns => maxTurns;
        public ILLMService LLMService => _llmService;
        public bool IsPausedFromBattle => _isPausedFromBattle;
        public IReadOnlyList<ConversationEntry> ConversationHistory => _conversationHistory;
        public StrategyEntry Strategy => _strategy;

        public void SetStrategy(StrategyEntry strategy)
        {
            _strategy = strategy;
        }

        private void Start()
        {
            InitializeLLMService();

            // Use opponent from GameManager if set (game loop flow),
            // otherwise fall back to serialized field (standalone testing)
            if (GameManager.Instance != null && GameManager.Instance.currentOpponent != null)
                opponent = GameManager.Instance.currentOpponent;

            if (conversationHistoryCanvas != null)
                conversationHistoryCanvas.SetActive(false);
            SwitchPhase(BattlePhase.BattlerSelecting);
        }

        private void InitializeLLMService()
        {
            if (useMockLLM || !ConfigLoader.HasGeminiApiKey())
            {
                _llmService = new MockLLMService();
                Debug.Log("[BattleSceneManager] Using MockLLMService");
            }
            else
            {
                _llmService = new GeminiService(ConfigLoader.GetGeminiApiKey(), ConfigLoader.GetGeminiModel());
                Debug.Log("[BattleSceneManager] Using GeminiService");
            }
        }

        public void SwitchPhase(BattlePhase phase)
        {
            _currentPhase = phase;

            battlerSelectingCanvas.SetActive(phase == BattlePhase.BattlerSelecting);
            strategySelectingCanvas.SetActive(phase == BattlePhase.StrategySelecting);
            battleCanvas.SetActive(phase == BattlePhase.Battle);
            battleResultCanvas.SetActive(phase == BattlePhase.BattleResult);

            switch (phase)
            {
                case BattlePhase.BattlerSelecting:
                    battlerSelectingView.Initialize(this);
                    break;
                case BattlePhase.StrategySelecting:
                    _currentTurn = 1;
                    strategySelectingView.Initialize(this);
                    break;
                case BattlePhase.Battle:
                    battleView.Initialize(this);
                    break;
                case BattlePhase.BattleResult:
                    battleResultView.Initialize(this, _currentTurn > maxTurns);
                    break;
            }
        }

        public void OnBattlerSelected(CharacterSO battler)
        {
            SelectedBattler = battler;
            SwitchPhase(BattlePhase.StrategySelecting);
        }

        public void OnStrategyConfirmed()
        {
            SwitchPhase(BattlePhase.Battle);
        }

        public void OnBattleFinished(bool playerWon)
        {
            SwitchPhase(BattlePhase.BattleResult);
        }

        public void AdvanceTurn()
        {
            _currentTurn++;
        }

        public void ReturnToBattlerSelection()
        {
            SwitchPhase(BattlePhase.BattlerSelecting);
        }

        // ─── Conversation History ────────────────────────────────────────

        public void AddConversationEntry(ConversationEntry entry)
        {
            _conversationHistory.Add(entry);
        }

        public void ShowConversationHistory()
        {
            if (conversationHistoryCanvas == null) return;
            conversationHistoryCanvas.SetActive(true);
            conversationHistoryView.Initialize(this);
        }

        public void HideConversationHistory()
        {
            if (conversationHistoryCanvas == null) return;
            conversationHistoryCanvas.SetActive(false);
        }

        // ─── Pause / Resume (Battle <-> Strategy) ───────────────────────

        public void PauseToStrategy()
        {
            _isPausedFromBattle = true;
            battleCanvas.SetActive(false);
            strategySelectingCanvas.SetActive(true);
            strategySelectingView.Initialize(this);
        }

        public void ResumeFromStrategy()
        {
            _isPausedFromBattle = false;
            strategySelectingCanvas.SetActive(false);
            battleCanvas.SetActive(true);
            battleView.Resume();
        }
    }
}
