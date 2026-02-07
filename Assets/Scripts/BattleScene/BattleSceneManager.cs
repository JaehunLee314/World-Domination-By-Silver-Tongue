using System.Collections.Generic;
using UnityEngine;
using SilverTongue.Data;
using SilverTongue.LLM;
using SilverTongue.BattleSystem;

namespace SilverTongue.BattleScene
{
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
        [SerializeField] private int opponentMaxSanity = 100;
        [SerializeField] private bool useMockLLM = false;

        private BattlePhase _currentPhase;
        private ILLMService _llmService;
        private bool _isPausedFromBattle;
        private readonly BattleState _state = new BattleState();

        public BattleState State => _state;
        public CharacterSO SelectedBattler { get; private set; }
        public CharacterSO Opponent => opponent;
        public int CurrentTurn => _state.CurrentTurn;
        public int MaxTurns => _state.MaxTurns;
        public int OpponentMaxSanity => _state.OpponentMaxSanity;
        public int OpponentCurrentSanity => _state.OpponentCurrentSanity;
        public ILLMService LLMService => _llmService;
        public bool IsPausedFromBattle => _isPausedFromBattle;
        public IReadOnlyList<ConversationEntry> ConversationHistory => _state.ConversationHistory;
        public StrategyEntry Strategy => _state.Strategy;
        public JudgeResult LastJudgeResult => _state.LastJudgeResult;

        public void SetStrategy(StrategyEntry strategy) => _state.SetStrategy(strategy);
        public int ApplyDamage(int damage) => _state.ApplyDamage(damage);
        public void SetLastJudgeResult(JudgeResult result) => _state.SetLastJudgeResult(result);
        public void AdvanceTurn() => _state.AdvanceTurn();
        public void AddConversationEntry(ConversationEntry entry) => _state.AddConversationEntry(entry);

        private void Start()
        {
            InitializeLLMService();

            // Use opponent from GameManager if set (game loop flow),
            // otherwise fall back to serialized field (standalone testing)
            if (GameManager.Instance != null && GameManager.Instance.currentOpponent != null)
                opponent = GameManager.Instance.currentOpponent;

            _state.Reset(maxTurns, opponentMaxSanity);

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
                    strategySelectingView.Initialize(this);
                    break;
                case BattlePhase.Battle:
                    battleView.Initialize(this);
                    break;
                case BattlePhase.BattleResult:
                    battleResultView.Initialize(this, _state.OpponentCurrentSanity <= 0);
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

        public void ReturnToBattlerSelection()
        {
            _state.Reset(maxTurns, opponentMaxSanity);
            SwitchPhase(BattlePhase.BattlerSelecting);
        }

        // ─── Conversation History ────────────────────────────────────────

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
