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

        // Battle loop state
        private BattlerAgent _playerAgent;
        private BattlerAgent _opponentAgent;
        private JudgeAgent _judgeAgent;
        private bool _isBattleActive;
        private bool _playerWon;
        private string _currentPhaseLabel = "";
        private Task<JudgeResult> _pendingJudgeTask;
        private Task<LLMResponse> _pendingDialogueResponse;

        public BattleState State => _state;
        public CharacterSO SelectedBattler { get; private set; }
        public CharacterSO Opponent => opponent;
        public int CurrentTurn => _state.CurrentTurn;
        public int MaxTurns => _state.MaxTurns;
        public ILLMService LLMService => _llmService;
        public bool IsPausedFromBattle => _isPausedFromBattle;
        public IReadOnlyList<ConversationEntry> ConversationHistory => _state.ConversationHistory;
        public StrategyEntry Strategy => _state.Strategy;

        public void SetStrategy(StrategyEntry strategy) => _state.SetStrategy(strategy);
        public void AdvanceTurn() => _state.AdvanceTurn();
        public void AddConversationEntry(ConversationEntry entry) => _state.AddConversationEntry(entry);

        private void Start()
        {
            InitializeLLMService();

            if (GameManager.Instance != null && GameManager.Instance.currentOpponent != null)
                opponent = GameManager.Instance.currentOpponent;

            _state.Reset(maxTurns, opponentMaxSanity, null, null);

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
                    battleResultView.Initialize(this, _playerWon);
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
            _playerWon = playerWon;
            SwitchPhase(BattlePhase.BattleResult);
        }

        public void ReturnToBattlerSelection()
        {
            _state.Reset(maxTurns, opponentMaxSanity, null, null);
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

        // ─── Battle Logic ────────────────────────────────────────────────

        private void StartBattle()
        {
            var player = SelectedBattler;
            var opp = opponent;
            var strategy = _state.Strategy;

            _state.Reset(maxTurns, opponentMaxSanity, player.loseConditions, opp.loseConditions);
            _state.SetStrategy(strategy);

            string playerPrompt = BattlerAgent.BuildPlayerPrompt(player, opp, _state.Strategy);
            string opponentPrompt = BattlerAgent.BuildOpponentPrompt(player, opp);

            _playerAgent = new BattlerAgent(player, playerPrompt, BattlerAgent.GetThinkingEffort(player));
            _opponentAgent = new BattlerAgent(opp, opponentPrompt, opp.thinkingEffort.ToString().ToLower());
            _judgeAgent = new JudgeAgent();
            _isBattleActive = true;
            _pendingJudgeTask = null;
            _pendingDialogueResponse = null;

            battleView.Initialize(this);
            battleView.InitializeConditionPanel(_state);
            RunAutoBattle();
        }

        private void ResumeBattle()
        {
            _isBattleActive = true;
            _playerAgent.UpdateSystemPrompt(
                BattlerAgent.BuildPlayerPrompt(SelectedBattler, opponent, _state.Strategy));

            battleView.Resume();

            // Pre-start player generation so it runs while user reads/clicks
            _pendingDialogueResponse = _playerAgent.GenerateResponse(_llmService);
            ContinueAutoBattle();
        }

        /// <summary>
        /// Strips XML tags, displays clean text, records both clean and raw to history.
        /// Returns parsed tags so caller can check AcceptDefeat.
        /// </summary>
        private ParsedTags DisplayAndRecord(string speaker, string rawText, string thought, bool isPlayer)
        {
            string cleanText = BattlerAgent.StripXmlTags(rawText, out var tags);

            string displayThought = !string.IsNullOrEmpty(thought) ? thought : tags.InnerThought;

            battleView.ShowDialogue(speaker, cleanText, displayThought);
            battleView.UpdateEmotionSprite(isPlayer, tags.Emotion);

            string timestamp = string.IsNullOrEmpty(_currentPhaseLabel)
                ? $"Turn {CurrentTurn}"
                : $"Turn {CurrentTurn} ({_currentPhaseLabel})";

            AddConversationEntry(new ConversationEntry
            {
                SpeakerName = speaker,
                SpeechText = cleanText,
                RawText = rawText,
                Timestamp = timestamp,
                IsPlayer = isPlayer,
                EvidenceUsed = tags.EvidenceUsed,
                Emotion = tags.Emotion
            });

            return tags;
        }

        // ─── Auto-Battle Loop ────────────────────────────────────────────

        private async void RunAutoBattle()
        {
            // Opponent opening — start generation before user clicks
            _opponentAgent.AddToHistory("user", "The debate begins. State your opening position.");

            var opponentTask = _opponentAgent.GenerateResponse(_llmService);

            _currentPhaseLabel = "Opening";
            battleView.UpdateTurnTracker("Opening");
            battleView.SetSpeaker(false);
            await battleView.MovePanelForwardAsync(false);
            if (!_isBattleActive) return;

            if (!opponentTask.IsCompleted)
                battleView.ShowThinking(opponent.characterName);

            var opponentResponse = await opponentTask;
            if (!_isBattleActive) return;

            if (opponentResponse.Success)
            {
                string content = BattlerAgent.CleanDialogue(opponentResponse.Content);
                _opponentAgent.AddToHistory("model", content);
                _playerAgent.AddToHistory("user", content);
                var tags = DisplayAndRecord(opponent.characterName, content, opponentResponse.ThoughtSummary, false);

                if (tags.AcceptDefeat)
                {
                    OnBattleFinished(true);
                    return;
                }
            }

            // Pre-start player generation for first turn
            _pendingDialogueResponse = _playerAgent.GenerateResponse(_llmService);

            await RunBattleLoop();
        }

        private async void ContinueAutoBattle()
        {
            await RunBattleLoop();
        }

        private async Task RunBattleLoop()
        {
            // Pre-condition: _pendingDialogueResponse holds player's in-progress generation
            // Pre-condition: previous speaker (opponent) panel is forward

            while (_isBattleActive && CurrentTurn <= MaxTurns)
            {
                // ── Apply any completed background judge from previous turn ──
                TryApplyPendingJudgeResult();
                if (!_isBattleActive) return;

                // ── Player's turn (generation already in progress) ──
                await battleView.WaitForInputAsync();
                if (!_isBattleActive) return;

                _currentPhaseLabel = "My Turn";
                battleView.UpdateTurnTracker("My Turn");
                battleView.SetSpeaker(true);
                await battleView.TransitionSpeakersAsync(false); // opponent back + player forward
                if (!_isBattleActive) return;

                if (!_pendingDialogueResponse.IsCompleted)
                    battleView.ShowThinking(SelectedBattler.characterName);

                var playerResponse = await _pendingDialogueResponse;
                if (!_isBattleActive) return;

                if (playerResponse.Success)
                {
                    string playerContent = BattlerAgent.CleanDialogue(playerResponse.Content);
                    _playerAgent.AddToHistory("model", playerContent);
                    _opponentAgent.AddToHistory("user", playerContent);
                    var playerTags = DisplayAndRecord(
                        SelectedBattler.characterName, playerContent, playerResponse.ThoughtSummary, true);

                    if (playerTags.AcceptDefeat)
                    {
                        OnBattleFinished(false);
                        return;
                    }
                }

                // Start opponent generation immediately (runs while user reads)
                _pendingDialogueResponse = _opponentAgent.GenerateResponse(_llmService);

                // ── Opponent's turn (generation already in progress) ──
                await battleView.WaitForInputAsync();
                if (!_isBattleActive) return;

                _currentPhaseLabel = "Opponent's Turn";
                battleView.UpdateTurnTracker("Opponent's Turn");
                battleView.SetSpeaker(false);
                await battleView.TransitionSpeakersAsync(true); // player back + opponent forward
                if (!_isBattleActive) return;

                if (!_pendingDialogueResponse.IsCompleted)
                    battleView.ShowThinking(opponent.characterName);

                var opponentResponse = await _pendingDialogueResponse;
                if (!_isBattleActive) return;

                if (opponentResponse.Success)
                {
                    string opponentContent = BattlerAgent.CleanDialogue(opponentResponse.Content);
                    _opponentAgent.AddToHistory("model", opponentContent);
                    _playerAgent.AddToHistory("user", opponentContent);
                    var opponentTags = DisplayAndRecord(
                        opponent.characterName, opponentContent, opponentResponse.ThoughtSummary, false);

                    if (opponentTags.AcceptDefeat)
                    {
                        OnBattleFinished(true);
                        return;
                    }
                }

                // ── Background Judge Evaluation ──
                bool isLastTurn = CurrentTurn >= MaxTurns;

                if (isLastTurn)
                {
                    // Last turn: must resolve everything before ending
                    if (_pendingJudgeTask != null)
                    {
                        await _pendingJudgeTask;
                        TryApplyPendingJudgeResult();
                        if (!_isBattleActive) return;
                    }
                    // Fire judge for this turn and await it
                    FireJudgeInBackground();
                    await _pendingJudgeTask;
                    TryApplyPendingJudgeResult();
                    if (!_isBattleActive) return;
                }
                else if (_pendingJudgeTask == null)
                {
                    // No judge running — fire one in background
                    FireJudgeInBackground();
                }
                // else: judge already running from previous turn, skip

                // Start player generation for next turn (runs during judge + user reading)
                if (!isLastTurn)
                    _pendingDialogueResponse = _playerAgent.GenerateResponse(_llmService);

                AdvanceTurn();
            }

            // Max turns reached — resolve any pending judge before timeout decision
            if (_isBattleActive && _pendingJudgeTask != null)
            {
                await _pendingJudgeTask;
                TryApplyPendingJudgeResult();
            }
            if (_isBattleActive)
            {
                OnBattleFinished(false);
            }
        }

        // ─── Async Judge Helpers ────────────────────────────────────────

        private void FireJudgeInBackground()
        {
            _pendingJudgeTask = _judgeAgent.Evaluate(
                _llmService,
                ConversationHistory,
                _state.GetUnmetConditions(true),
                _state.GetUnmetConditions(false),
                SelectedBattler.characterName,
                opponent.characterName,
                _state.OpponentCurrentSanity,
                _state.OpponentMaxSanity);
        }

        private void TryApplyPendingJudgeResult()
        {
            if (_pendingJudgeTask == null || !_pendingJudgeTask.IsCompleted)
                return;

            if (_pendingJudgeTask.IsFaulted)
            {
                Debug.LogWarning($"[Judge] Background judge failed: {_pendingJudgeTask.Exception}");
                _pendingJudgeTask = null;
                return;
            }

            var judgeResult = _pendingJudgeTask.Result;
            _pendingJudgeTask = null;
            ApplyJudgeResult(judgeResult);
        }

        private void ApplyJudgeResult(JudgeResult judgeResult)
        {
            _state.SetLastJudgeResult(judgeResult);
            _state.UpdateConditions(judgeResult.playerEval, judgeResult.opponentEval);

            if (judgeResult.damage > 0)
                _state.ApplyDamage(judgeResult.damage);

            Debug.Log($"[Judge] damage={judgeResult.damage}, " +
                $"sanity={_state.OpponentCurrentSanity}/{_state.OpponentMaxSanity}, " +
                $"playerCondMet={_state.MetConditionCount(true)}/{_state.TotalConditionCount(true)}, " +
                $"oppCondMet={_state.MetConditionCount(false)}/{_state.TotalConditionCount(false)}");

            battleView.RefreshConditionPanel(_state);

            if (_state.OpponentCurrentSanity <= 0)
            {
                OnBattleFinished(true);
                return;
            }
            if (_state.AllConditionsMet(true))
            {
                OnBattleFinished(false);
                return;
            }
        }
    }
}
