using System.Collections.Generic;
using System.Threading.Tasks;
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

        // Battle loop state (owned by manager, not view)
        private BattlerAgent _playerAgent;
        private BattlerAgent _opponentAgent;
        private JudgeAgent _judgeAgent;
        private bool _isBattleActive;
        private string _currentPhaseLabel = "";

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
                    StartBattle();
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
            _isBattleActive = false;
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
            _isBattleActive = false;
            battleView.SetBattleActive(false);
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
            ResumeBattle();
        }

        // ─── Battle Logic (migrated from BattleView) ────────────────────

        private void StartBattle()
        {
            var player = SelectedBattler;
            var opp = opponent;

            string playerPrompt = BattlerAgent.BuildPlayerPrompt(player, opp, _state.Strategy);
            string opponentPrompt = BattlerAgent.BuildOpponentPrompt(player, opp);

            _playerAgent = new BattlerAgent(player, playerPrompt, BattlerAgent.GetThinkingEffort(player));
            _opponentAgent = new BattlerAgent(opp, opponentPrompt, opp.thinkingEffort.ToString().ToLower());
            _judgeAgent = new JudgeAgent();
            _isBattleActive = true;

            battleView.Initialize(this);
            RunAutoBattle();
        }

        private void ResumeBattle()
        {
            _isBattleActive = true;
            _playerAgent.UpdateSystemPrompt(
                BattlerAgent.BuildPlayerPrompt(SelectedBattler, opponent, _state.Strategy));

            battleView.Resume();
            ContinueAutoBattle();
        }

        private void RecordDialogue(string speaker, string text, bool isPlayer,
            string evidenceUsed = null, string damageType = null, int damageDealt = 0)
        {
            string timestamp = string.IsNullOrEmpty(_currentPhaseLabel)
                ? $"Turn {CurrentTurn}"
                : $"Turn {CurrentTurn} ({_currentPhaseLabel})";

            AddConversationEntry(new ConversationEntry
            {
                SpeakerName = speaker,
                SpeechText = text,
                Timestamp = timestamp,
                IsPlayer = isPlayer,
                EvidenceUsed = evidenceUsed,
                DamageType = damageType,
                DamageDealt = damageDealt
            });
        }

        private void DisplayAndRecord(string speaker, string text, string thought, bool isPlayer)
        {
            battleView.ShowDialogue(speaker, text, thought);
            RecordDialogue(speaker, text, isPlayer,
                evidenceUsed: BattlerAgent.ParseTag(text, "evidence_used"));
        }

        private int CalculateActualDamage(JudgeResult result)
        {
            if (result.damage_type == "Trap Trigger")
                return -Mathf.Abs(result.damage_dealt);
            return Mathf.Abs(result.damage_dealt);
        }

        // ─── Auto-Battle Loop ───────────────────────────────────────────

        private async void RunAutoBattle()
        {
            // Opponent opening
            _currentPhaseLabel = "Opening";
            battleView.UpdateTurnTracker("Opening");
            _opponentAgent.AddToHistory("user", "The debate begins. State your opening position.");

            battleView.SetSpeaker(false);
            await battleView.MovePanelForwardAsync(false);
            if (!_isBattleActive) return;

            battleView.ShowThinking(opponent.characterName);
            var opponentResponse = await _opponentAgent.GenerateResponse(_llmService);
            if (!_isBattleActive) return;

            if (opponentResponse.Success)
            {
                string content = BattlerAgent.CleanDialogue(opponentResponse.Content);
                _opponentAgent.AddToHistory("model", content);
                _playerAgent.AddToHistory("user", content);
                DisplayAndRecord(opponent.characterName, content, opponentResponse.ThoughtSummary, false);
            }

            await battleView.MovePanelBackAsync(false);
            if (!_isBattleActive) return;

            await battleView.WaitForInputAsync();
            if (!_isBattleActive) return;

            await RunBattleLoop();
        }

        private async void ContinueAutoBattle()
        {
            await RunBattleLoop();
        }

        private async Task RunBattleLoop()
        {
            while (_isBattleActive && CurrentTurn <= MaxTurns)
            {
                // ── Player's turn ──
                _currentPhaseLabel = "My Turn";
                battleView.UpdateTurnTracker("My Turn");
                battleView.SetSpeaker(true);
                await battleView.MovePanelForwardAsync(true);
                if (!_isBattleActive) return;

                battleView.ShowThinking(SelectedBattler.characterName);
                var playerResponse = await _playerAgent.GenerateResponse(_llmService);
                if (!_isBattleActive) return;

                if (playerResponse.Success)
                {
                    string playerContent = BattlerAgent.CleanDialogue(playerResponse.Content);
                    _playerAgent.AddToHistory("model", playerContent);
                    _opponentAgent.AddToHistory("user", playerContent);
                    DisplayAndRecord(SelectedBattler.characterName, playerContent, playerResponse.ThoughtSummary, true);

                    if (JudgeAgent.CheckLoseConditions(SelectedBattler.loseConditions, playerContent))
                    {
                        battleView.ShowDialogue("JUDGE", "Player triggered a lose condition!", null);
                        RecordDialogue("JUDGE", "Player triggered a lose condition!", false);
                        SetLastJudgeResult(new JudgeResult
                        {
                            damage_type = "Trap Trigger",
                            status = BattleStatus.OpponentWins,
                            reasoning = "Player triggered a lose condition."
                        });
                        OnBattleFinished(false);
                        return;
                    }
                }

                await battleView.MovePanelBackAsync(true);
                if (!_isBattleActive) return;

                await battleView.WaitForInputAsync();
                if (!_isBattleActive) return;

                // ── Opponent's turn ──
                _currentPhaseLabel = "Opponent's Turn";
                battleView.UpdateTurnTracker("Opponent's Turn");
                battleView.SetSpeaker(false);
                await battleView.MovePanelForwardAsync(false);
                if (!_isBattleActive) return;

                battleView.ShowThinking(opponent.characterName);
                var opponentResponse = await _opponentAgent.GenerateResponse(_llmService);
                if (!_isBattleActive) return;

                if (opponentResponse.Success)
                {
                    string opponentContent = BattlerAgent.CleanDialogue(opponentResponse.Content);
                    _opponentAgent.AddToHistory("model", opponentContent);
                    _playerAgent.AddToHistory("user", opponentContent);
                    DisplayAndRecord(opponent.characterName, opponentContent, opponentResponse.ThoughtSummary, false);

                    if (JudgeAgent.CheckLoseConditions(opponent.loseConditions, opponentContent))
                    {
                        battleView.ShowDialogue("JUDGE", "Opponent triggered a lose condition! Player wins!", null);
                        RecordDialogue("JUDGE", "Opponent triggered a lose condition! Player wins!", false);
                        ApplyDamage(OpponentCurrentSanity);
                        battleView.UpdateSanityBar();
                        SetLastJudgeResult(new JudgeResult
                        {
                            damage_type = "Critical Hit",
                            status = BattleStatus.PlayerWins,
                            reasoning = "Opponent triggered a lose condition."
                        });
                        OnBattleFinished(true);
                        return;
                    }
                }

                await battleView.MovePanelBackAsync(false);
                if (!_isBattleActive) return;

                await battleView.WaitForInputAsync();
                if (!_isBattleActive) return;

                // ── Judge Evaluation ──
                _currentPhaseLabel = "Judge";
                battleView.UpdateTurnTracker("Judge");
                battleView.SetJudgeSpeaker();

                battleView.ShowThinking("JUDGE");
                var judgeResult = await _judgeAgent.Evaluate(
                    _llmService,
                    ConversationHistory,
                    opponent,
                    OpponentCurrentSanity,
                    OpponentMaxSanity);
                if (!_isBattleActive) return;

                SetLastJudgeResult(judgeResult);

                int actualDamage = CalculateActualDamage(judgeResult);
                ApplyDamage(actualDamage);
                battleView.UpdateSanityBar();

                // Show judge feedback
                string dmgSign = actualDamage >= 0 ? "-" : "+";
                string judgeFeedback = $"{judgeResult.damage_type}! ({dmgSign}{Mathf.Abs(actualDamage)} Sanity) " +
                    $"[{OpponentCurrentSanity}/{OpponentMaxSanity}]";
                battleView.ShowJudgeDialogue(judgeFeedback, judgeResult);
                RecordDialogue("JUDGE", judgeFeedback, false,
                    damageType: judgeResult.damage_type, damageDealt: judgeResult.damage_dealt);

                // Check win/loss from judge
                if (judgeResult.status == BattleStatus.PlayerWins || OpponentCurrentSanity <= 0)
                {
                    battleView.ShowDialogue("JUDGE", $"CONFIRMED: {opponent.characterName} BROKEN!", null);
                    RecordDialogue("JUDGE", $"CONFIRMED: {opponent.characterName} BROKEN!", false);
                    OnBattleFinished(true);
                    return;
                }
                if (judgeResult.status == BattleStatus.OpponentWins)
                {
                    battleView.ShowDialogue("JUDGE", "CONTRACT SIGNED: YOU LOSE!", null);
                    RecordDialogue("JUDGE", "CONTRACT SIGNED: YOU LOSE!", false);
                    OnBattleFinished(false);
                    return;
                }

                await battleView.WaitForInputAsync();
                if (!_isBattleActive) return;

                AdvanceTurn();
            }

            // Max turns reached — final verdict
            if (_isBattleActive)
            {
                await RunFinalVerdict();
            }
        }

        private async Task RunFinalVerdict()
        {
            _currentPhaseLabel = "Final Verdict";
            battleView.UpdateTurnTracker("Final Verdict");
            battleView.SetJudgeSpeaker();
            battleView.ShowDialogue("JUDGE", "Maximum turns reached. Delivering final verdict...", null);
            RecordDialogue("JUDGE", "Maximum turns reached. Delivering final verdict...", false);
            battleView.ShowThinking("JUDGE");

            var judgeResult = await _judgeAgent.Evaluate(
                _llmService,
                ConversationHistory,
                opponent,
                OpponentCurrentSanity,
                OpponentMaxSanity);
            if (!_isBattleActive) return;

            SetLastJudgeResult(judgeResult);

            if (judgeResult.damage_dealt != 0)
            {
                int actualDamage = CalculateActualDamage(judgeResult);
                ApplyDamage(actualDamage);
                battleView.UpdateSanityBar();
            }

            // Below 50% sanity = player wins on timeout
            bool playerWon = OpponentCurrentSanity <= 0
                || judgeResult.status == BattleStatus.PlayerWins
                || OpponentCurrentSanity < OpponentMaxSanity / 2;

            string verdict = playerWon
                ? $"CONFIRMED: {opponent.characterName} BROKEN!"
                : "CONTRACT SIGNED: YOU LOSE!";
            battleView.ShowDialogue("JUDGE", verdict, judgeResult.reasoning);
            RecordDialogue("JUDGE", verdict, false);

            OnBattleFinished(playerWon);
        }
    }
}
