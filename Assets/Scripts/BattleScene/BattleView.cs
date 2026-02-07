using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SilverTongue.BattleSystem;

namespace SilverTongue.BattleScene
{
    public class BattleView : MonoBehaviour
    {
        [Header("Top UI")]
        [SerializeField] private TextMeshProUGUI turnTrackerText;
        [SerializeField] private Button autoProgressButton;
        [SerializeField] private TextMeshProUGUI autoProgressButtonText;
        [SerializeField] private Button goToStrategyButton;
        [SerializeField] private Button logButton;

        [Header("Character Display")]
        [SerializeField] private Image playerCharacterImage;
        [SerializeField] private Image opponentCharacterImage;
        [SerializeField] private TextMeshProUGUI playerNameLabel;
        [SerializeField] private TextMeshProUGUI opponentNameLabel;

        [Header("Stage Animation")]
        [SerializeField] private RectTransform playerPanel;
        [SerializeField] private RectTransform opponentPanel;

        [Header("Dialogue Area")]
        [SerializeField] private TextMeshProUGUI speakerNameText;
        [SerializeField] private TextMeshProUGUI dialogueText;
        [SerializeField] private TextMeshProUGUI thoughtText;
        [SerializeField] private Button dialogueAreaButton;

        [Header("Sanity Bar")]
        [SerializeField] private Image sanityBarFill;
        [SerializeField] private TextMeshProUGUI sanityText;

        [Header("Visual Settings")]
        [SerializeField] private Color activeSpeakerColor = Color.white;
        [SerializeField] private Color inactiveSpeakerColor = new Color(0.3f, 0.3f, 0.3f, 1f);
        [SerializeField] private float panelMoveDuration = 0.4f;

        private BattleSceneManager _manager;
        private bool _isAutoProgress;
        private bool _isBattleActive;
        private bool _waitingForInput;

        // Agents
        private BattlerAgent _playerAgent;
        private BattlerAgent _opponentAgent;
        private JudgeAgent _judgeAgent;

        // Current phase label for timestamps
        private string _currentPhaseLabel = "";

        // Stage animation: original anchor positions
        private Vector2 _playerPanelOrigAnchorMin;
        private Vector2 _playerPanelOrigAnchorMax;
        private Vector2 _opponentPanelOrigAnchorMin;
        private Vector2 _opponentPanelOrigAnchorMax;

        // "Step forward" anchor positions (computed from originals, only X shifts)
        private Vector2 _playerForwardAnchorMin;
        private Vector2 _playerForwardAnchorMax;
        private Vector2 _opponentForwardAnchorMin;
        private Vector2 _opponentForwardAnchorMax;

        private const float StepForwardAmount = 0.10f;

        public void Initialize(BattleSceneManager manager)
        {
            _manager = manager;
            _isAutoProgress = false;
            _isBattleActive = true;
            _waitingForInput = false;

            // Create agents
            var player = manager.SelectedBattler;
            var opponent = manager.Opponent;

            string playerPrompt = BattlerAgent.BuildPlayerPrompt(player, opponent, manager.Strategy);
            string opponentPrompt = BattlerAgent.BuildOpponentPrompt(player, opponent);

            _playerAgent = new BattlerAgent(player, playerPrompt, BattlerAgent.GetThinkingEffort(player));
            _opponentAgent = new BattlerAgent(opponent, opponentPrompt, opponent.thinkingEffort.ToString().ToLower());
            _judgeAgent = new JudgeAgent();

            CacheOriginalPanelPositions();
            SetupDisplay();
            SetupButtons();
            UpdateTurnTracker();
            UpdateAutoProgressUI();
            UpdateSanityBar();

            SetSpeaker(false);
            RunAutoBattle();
        }

        public void Resume()
        {
            _isAutoProgress = false;
            _isBattleActive = true;
            _waitingForInput = false;
            UpdateTurnTracker();
            UpdateAutoProgressUI();
            UpdateSanityBar();

            // Rebuild player prompt in case strategy changed during pause
            _playerAgent.UpdateSystemPrompt(
                BattlerAgent.BuildPlayerPrompt(_manager.SelectedBattler, _manager.Opponent, _manager.Strategy));
            ContinueAutoBattle();
        }

        // ─── Setup ──────────────────────────────────────────────────────────

        private void CacheOriginalPanelPositions()
        {
            if (playerPanel != null)
            {
                _playerPanelOrigAnchorMin = playerPanel.anchorMin;
                _playerPanelOrigAnchorMax = playerPanel.anchorMax;
                _playerForwardAnchorMin = new Vector2(_playerPanelOrigAnchorMin.x + StepForwardAmount, _playerPanelOrigAnchorMin.y);
                _playerForwardAnchorMax = new Vector2(_playerPanelOrigAnchorMax.x + StepForwardAmount, _playerPanelOrigAnchorMax.y);
            }
            if (opponentPanel != null)
            {
                _opponentPanelOrigAnchorMin = opponentPanel.anchorMin;
                _opponentPanelOrigAnchorMax = opponentPanel.anchorMax;
                _opponentForwardAnchorMin = new Vector2(_opponentPanelOrigAnchorMin.x - StepForwardAmount, _opponentPanelOrigAnchorMin.y);
                _opponentForwardAnchorMax = new Vector2(_opponentPanelOrigAnchorMax.x - StepForwardAmount, _opponentPanelOrigAnchorMax.y);
            }
        }

        private void SetupDisplay()
        {
            var player = _manager.SelectedBattler;
            var opponent = _manager.Opponent;

            playerNameLabel.text = player.characterName;
            opponentNameLabel.text = opponent.characterName;

            if (player.profileImage != null)
                playerCharacterImage.sprite = player.profileImage;
            if (opponent.profileImage != null)
                opponentCharacterImage.sprite = opponent.profileImage;

            dialogueText.text = "";
            speakerNameText.text = "";
            thoughtText.text = "";
        }

        private void SetupButtons()
        {
            autoProgressButton.onClick.RemoveAllListeners();
            autoProgressButton.onClick.AddListener(OnToggleAutoProgress);

            goToStrategyButton.onClick.RemoveAllListeners();
            goToStrategyButton.onClick.AddListener(OnGoToStrategy);

            if (logButton != null)
            {
                logButton.onClick.RemoveAllListeners();
                logButton.onClick.AddListener(() => _manager.ShowConversationHistory());
            }

            if (dialogueAreaButton != null)
            {
                dialogueAreaButton.onClick.RemoveAllListeners();
                dialogueAreaButton.onClick.AddListener(OnDialogueAreaClicked);
            }
        }

        private void UpdateTurnTracker(string phaseLabel = null)
        {
            _currentPhaseLabel = phaseLabel ?? "";
            if (string.IsNullOrEmpty(phaseLabel))
                turnTrackerText.text = $"Turn {_manager.CurrentTurn}/{_manager.MaxTurns}";
            else
                turnTrackerText.text = $"Turn {_manager.CurrentTurn}/{_manager.MaxTurns} ({phaseLabel})";
        }

        private void UpdateSanityBar()
        {
            if (sanityBarFill != null)
            {
                float ratio = (float)_manager.OpponentCurrentSanity / _manager.OpponentMaxSanity;
                sanityBarFill.fillAmount = ratio;
                sanityBarFill.color = Color.Lerp(Color.red, Color.green, ratio);
            }
            if (sanityText != null)
            {
                sanityText.text = $"Sanity: {_manager.OpponentCurrentSanity}/{_manager.OpponentMaxSanity}";
            }
        }

        private void UpdateAutoProgressUI()
        {
            if (autoProgressButtonText != null)
                autoProgressButtonText.text = _isAutoProgress ? "AUTO: ON" : "AUTO: OFF";

            var btnImg = autoProgressButton.GetComponent<Image>();
            if (btnImg != null)
                btnImg.color = _isAutoProgress
                    ? new Color(0.2f, 0.7f, 0.3f)
                    : new Color(0.5f, 0.5f, 0.2f);

            goToStrategyButton.interactable = !_isAutoProgress;
        }

        // ─── Button Handlers ────────────────────────────────────────────────

        private void OnToggleAutoProgress()
        {
            _isAutoProgress = !_isAutoProgress;
            UpdateAutoProgressUI();

            if (_isAutoProgress && _waitingForInput)
                _waitingForInput = false;
        }

        private void OnGoToStrategy()
        {
            if (_isAutoProgress) return;
            _isBattleActive = false;
            _waitingForInput = false;
            _manager.PauseToStrategy();
        }

        private void OnDialogueAreaClicked()
        {
            if (!_isAutoProgress && _waitingForInput)
                _waitingForInput = false;
        }

        private async System.Threading.Tasks.Task WaitForInput()
        {
            if (_isAutoProgress) return;
            _waitingForInput = true;
            while (_waitingForInput && _isBattleActive)
                await Awaitable.NextFrameAsync();
        }

        // ─── Speaker Visual State ──────────────────────────────────────────

        public void SetSpeaker(bool isPlayer)
        {
            playerCharacterImage.color = isPlayer ? activeSpeakerColor : inactiveSpeakerColor;
            opponentCharacterImage.color = isPlayer ? inactiveSpeakerColor : activeSpeakerColor;
            speakerNameText.text = isPlayer
                ? _manager.SelectedBattler.characterName
                : _manager.Opponent.characterName;
        }

        private void SetJudgeActive()
        {
            playerCharacterImage.color = inactiveSpeakerColor;
            opponentCharacterImage.color = inactiveSpeakerColor;
            speakerNameText.text = "JUDGE";
        }

        // ─── Smooth Panel Movement (Async) ──────────────────────────────────

        private async System.Threading.Tasks.Task SmoothMovePanelAsync(RectTransform panel, Vector2 targetMin, Vector2 targetMax, float duration)
        {
            Vector2 startMin = panel.anchorMin;
            Vector2 startMax = panel.anchorMax;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                panel.anchorMin = Vector2.Lerp(startMin, targetMin, t);
                panel.anchorMax = Vector2.Lerp(startMax, targetMax, t);
                panel.offsetMin = Vector2.zero;
                panel.offsetMax = Vector2.zero;
                await Awaitable.NextFrameAsync();
            }

            panel.anchorMin = targetMin;
            panel.anchorMax = targetMax;
            panel.offsetMin = Vector2.zero;
            panel.offsetMax = Vector2.zero;
        }

        private async System.Threading.Tasks.Task MovePanelForwardAsync(bool isPlayer)
        {
            if (isPlayer && playerPanel != null)
                await SmoothMovePanelAsync(playerPanel, _playerForwardAnchorMin, _playerForwardAnchorMax, panelMoveDuration);
            else if (!isPlayer && opponentPanel != null)
                await SmoothMovePanelAsync(opponentPanel, _opponentForwardAnchorMin, _opponentForwardAnchorMax, panelMoveDuration);
        }

        private async System.Threading.Tasks.Task MovePanelBackAsync(bool isPlayer)
        {
            if (isPlayer && playerPanel != null)
                await SmoothMovePanelAsync(playerPanel, _playerPanelOrigAnchorMin, _playerPanelOrigAnchorMax, panelMoveDuration);
            else if (!isPlayer && opponentPanel != null)
                await SmoothMovePanelAsync(opponentPanel, _opponentPanelOrigAnchorMin, _opponentPanelOrigAnchorMax, panelMoveDuration);
        }

        // ─── Auto-Battle Loop ───────────────────────────────────────────────

        private async void RunAutoBattle()
        {
            // Opponent opening
            UpdateTurnTracker("Opening");
            _opponentAgent.AddToHistory("user", "The debate begins. State your opening position.");

            SetSpeaker(false);
            await MovePanelForwardAsync(false);
            if (!_isBattleActive) return;

            ShowThinking(_manager.Opponent.characterName);
            var opponentResponse = await _opponentAgent.GenerateResponse(_manager.LLMService);
            if (!_isBattleActive) return;

            if (opponentResponse.Success)
            {
                string content = BattlerAgent.CleanDialogue(opponentResponse.Content);
                _opponentAgent.AddToHistory("model", content);
                _playerAgent.AddToHistory("user", content);
                ShowDialogue(_manager.Opponent.characterName, content, opponentResponse.ThoughtSummary);
            }

            await MovePanelBackAsync(false);
            if (!_isBattleActive) return;

            await WaitForInput();
            if (!_isBattleActive) return;

            await RunBattleLoop();
        }

        private async void ContinueAutoBattle()
        {
            await RunBattleLoop();
        }

        private async System.Threading.Tasks.Task RunBattleLoop()
        {
            while (_isBattleActive && _manager.CurrentTurn <= _manager.MaxTurns)
            {
                // ── Player's turn ──
                UpdateTurnTracker("My Turn");
                SetSpeaker(true);
                await MovePanelForwardAsync(true);
                if (!_isBattleActive) return;

                ShowThinking(_manager.SelectedBattler.characterName);
                var playerResponse = await _playerAgent.GenerateResponse(_manager.LLMService);
                if (!_isBattleActive) return;

                if (playerResponse.Success)
                {
                    string playerContent = BattlerAgent.CleanDialogue(playerResponse.Content);
                    _playerAgent.AddToHistory("model", playerContent);
                    _opponentAgent.AddToHistory("user", playerContent);
                    ShowDialogue(_manager.SelectedBattler.characterName, playerContent, playerResponse.ThoughtSummary);

                    if (JudgeAgent.CheckLoseConditions(_manager.SelectedBattler.loseConditions, playerContent))
                    {
                        _isBattleActive = false;
                        ShowDialogue("JUDGE", "Player triggered a lose condition!", null);
                        _manager.SetLastJudgeResult(new JudgeResult
                        {
                            damage_type = "Trap Trigger",
                            status = "PROPHET_WINS",
                            reasoning = "Player triggered a lose condition."
                        });
                        _manager.OnBattleFinished(false);
                        return;
                    }
                }

                await MovePanelBackAsync(true);
                if (!_isBattleActive) return;

                await WaitForInput();
                if (!_isBattleActive) return;

                // ── Opponent's turn ──
                UpdateTurnTracker("Opponent's Turn");
                SetSpeaker(false);
                await MovePanelForwardAsync(false);
                if (!_isBattleActive) return;

                ShowThinking(_manager.Opponent.characterName);
                var opponentResponse = await _opponentAgent.GenerateResponse(_manager.LLMService);
                if (!_isBattleActive) return;

                if (opponentResponse.Success)
                {
                    string opponentContent = BattlerAgent.CleanDialogue(opponentResponse.Content);
                    _opponentAgent.AddToHistory("model", opponentContent);
                    _playerAgent.AddToHistory("user", opponentContent);
                    ShowDialogue(_manager.Opponent.characterName, opponentContent, opponentResponse.ThoughtSummary);

                    if (JudgeAgent.CheckLoseConditions(_manager.Opponent.loseConditions, opponentContent))
                    {
                        _isBattleActive = false;
                        ShowDialogue("JUDGE", "Opponent triggered a lose condition! Player wins!", null);
                        _manager.ApplyDamage(_manager.OpponentCurrentSanity);
                        UpdateSanityBar();
                        _manager.SetLastJudgeResult(new JudgeResult
                        {
                            damage_type = "Critical Hit",
                            status = "KENTA_WINS",
                            reasoning = "Opponent triggered a lose condition."
                        });
                        _manager.OnBattleFinished(true);
                        return;
                    }
                }

                await MovePanelBackAsync(false);
                if (!_isBattleActive) return;

                await WaitForInput();
                if (!_isBattleActive) return;

                // ── Judge Evaluation (visible phase) ──
                UpdateTurnTracker("Judge");
                SetJudgeActive();

                ShowThinking("JUDGE");
                var judgeResult = await _judgeAgent.Evaluate(
                    _manager.LLMService,
                    _manager.ConversationHistory,
                    _manager.Opponent,
                    _manager.OpponentCurrentSanity,
                    _manager.OpponentMaxSanity);
                if (!_isBattleActive) return;

                _manager.SetLastJudgeResult(judgeResult);

                // Apply damage based on judge verdict
                int actualDamage = judgeResult.damage_dealt;
                if (judgeResult.damage_type == "Trap Trigger")
                    actualDamage = -Mathf.Abs(actualDamage); // Heal opponent
                else
                    actualDamage = Mathf.Abs(actualDamage); // Damage opponent

                _manager.ApplyDamage(actualDamage);
                UpdateSanityBar();

                // Show judge feedback
                string dmgSign = actualDamage >= 0 ? "-" : "+";
                string judgeFeedback = $"{judgeResult.damage_type}! ({dmgSign}{Mathf.Abs(actualDamage)} Sanity) " +
                    $"[{_manager.OpponentCurrentSanity}/{_manager.OpponentMaxSanity}]";
                ShowJudgeDialogue(judgeFeedback, judgeResult);

                // Check win/loss from judge
                if (judgeResult.status == "KENTA_WINS" || _manager.OpponentCurrentSanity <= 0)
                {
                    _isBattleActive = false;
                    ShowDialogue("JUDGE", $"CONFIRMED: {_manager.Opponent.characterName} BROKEN!", null);
                    _manager.OnBattleFinished(true);
                    return;
                }
                if (judgeResult.status == "PROPHET_WINS")
                {
                    _isBattleActive = false;
                    ShowDialogue("JUDGE", "CONTRACT SIGNED: YOU LOSE!", null);
                    _manager.OnBattleFinished(false);
                    return;
                }

                await WaitForInput();
                if (!_isBattleActive) return;

                _manager.AdvanceTurn();
            }

            // Max turns reached — final verdict based on sanity
            if (_isBattleActive)
            {
                await RunFinalVerdict();
            }
        }

        private async System.Threading.Tasks.Task RunFinalVerdict()
        {
            UpdateTurnTracker("Final Verdict");
            SetJudgeActive();
            ShowDialogue("JUDGE", "Maximum turns reached. Delivering final verdict...", null);
            ShowThinking("JUDGE");

            var judgeResult = await _judgeAgent.Evaluate(
                _manager.LLMService,
                _manager.ConversationHistory,
                _manager.Opponent,
                _manager.OpponentCurrentSanity,
                _manager.OpponentMaxSanity);
            if (!_isBattleActive) return;

            _manager.SetLastJudgeResult(judgeResult);

            if (judgeResult.damage_dealt != 0)
            {
                int actualDamage = judgeResult.damage_type == "Trap Trigger"
                    ? -Mathf.Abs(judgeResult.damage_dealt)
                    : Mathf.Abs(judgeResult.damage_dealt);
                _manager.ApplyDamage(actualDamage);
                UpdateSanityBar();
            }

            // Below 50% sanity = player wins on timeout
            bool playerWon = _manager.OpponentCurrentSanity <= 0
                || judgeResult.status == "KENTA_WINS"
                || _manager.OpponentCurrentSanity < _manager.OpponentMaxSanity / 2;

            string verdict = playerWon
                ? $"CONFIRMED: {_manager.Opponent.characterName} BROKEN!"
                : "CONTRACT SIGNED: YOU LOSE!";
            ShowDialogue("JUDGE", verdict, judgeResult.reasoning);

            _isBattleActive = false;
            _manager.OnBattleFinished(playerWon);
        }

        // ─── Dialogue Display ───────────────────────────────────────────────

        private void ShowThinking(string speaker)
        {
            speakerNameText.text = speaker;
            dialogueText.text = "<i>Thinking...</i>";
            thoughtText.text = "";
        }

        public void ShowDialogue(string speaker, string text, string thought)
        {
            speakerNameText.text = speaker;
            dialogueText.text = text;
            thoughtText.text = !string.IsNullOrEmpty(thought) ? thought : "";

            string timestamp = string.IsNullOrEmpty(_currentPhaseLabel)
                ? $"Turn {_manager.CurrentTurn}"
                : $"Turn {_manager.CurrentTurn} ({_currentPhaseLabel})";

            _manager.AddConversationEntry(new ConversationEntry
            {
                SpeakerName = speaker,
                SpeechText = text,
                Timestamp = timestamp,
                IsPlayer = speaker == _manager.SelectedBattler.characterName,
                EvidenceUsed = BattlerAgent.ParseTag(text, "evidence_used")
            });
        }

        private void ShowJudgeDialogue(string text, JudgeResult result)
        {
            speakerNameText.text = "JUDGE";
            dialogueText.text = text;
            thoughtText.text = !string.IsNullOrEmpty(result.reasoning) ? result.reasoning : "";

            string timestamp = string.IsNullOrEmpty(_currentPhaseLabel)
                ? $"Turn {_manager.CurrentTurn}"
                : $"Turn {_manager.CurrentTurn} ({_currentPhaseLabel})";

            _manager.AddConversationEntry(new ConversationEntry
            {
                SpeakerName = "JUDGE",
                SpeechText = text,
                Timestamp = timestamp,
                IsPlayer = false,
                DamageType = result.damage_type,
                DamageDealt = result.damage_dealt
            });
        }
    }
}
