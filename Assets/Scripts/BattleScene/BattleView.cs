using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SilverTongue.Data;
using SilverTongue.LLM;

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

        [Header("Visual Settings")]
        [SerializeField] private Color activeSpeakerColor = Color.white;
        [SerializeField] private Color inactiveSpeakerColor = new Color(0.3f, 0.3f, 0.3f, 1f);
        [SerializeField] private float panelMoveDuration = 0.4f;

        private BattleSceneManager _manager;
        private bool _isAutoProgress;
        private bool _isBattleActive;
        private bool _waitingForInput;

        // Cumulative LLM conversation histories (mirrored perspectives)
        private List<LLMMessage> _playerHistory = new List<LLMMessage>();
        private List<LLMMessage> _opponentHistory = new List<LLMMessage>();
        private string _playerSystemPrompt;
        private string _opponentSystemPrompt;

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
            _playerHistory.Clear();
            _opponentHistory.Clear();

            CacheOriginalPanelPositions();
            SetupDisplay();
            SetupButtons();
            UpdateTurnTracker();
            UpdateAutoProgressUI();

            _playerSystemPrompt = BuildPlayerSystemPrompt();
            _opponentSystemPrompt = BuildOpponentSystemPrompt();

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

            // Rebuild player prompt in case strategy changed during pause
            _playerSystemPrompt = BuildPlayerSystemPrompt();
            ContinueAutoBattle();
        }

        // ─── Setup ──────────────────────────────────────────────────────────

        private void CacheOriginalPanelPositions()
        {
            if (playerPanel != null)
            {
                _playerPanelOrigAnchorMin = playerPanel.anchorMin;
                _playerPanelOrigAnchorMax = playerPanel.anchorMax;
                // Step forward = shift X right, keep Y
                _playerForwardAnchorMin = new Vector2(_playerPanelOrigAnchorMin.x + StepForwardAmount, _playerPanelOrigAnchorMin.y);
                _playerForwardAnchorMax = new Vector2(_playerPanelOrigAnchorMax.x + StepForwardAmount, _playerPanelOrigAnchorMax.y);
            }
            if (opponentPanel != null)
            {
                _opponentPanelOrigAnchorMin = opponentPanel.anchorMin;
                _opponentPanelOrigAnchorMax = opponentPanel.anchorMax;
                // Step forward = shift X left, keep Y
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

        private void UpdateTurnTracker()
        {
            turnTrackerText.text = $"Turn {_manager.CurrentTurn}/{_manager.MaxTurns}";
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

        // ─── Speaker Visual State ──────────────────────────────────────────

        public void SetSpeaker(bool isPlayer)
        {
            playerCharacterImage.color = isPlayer ? activeSpeakerColor : inactiveSpeakerColor;
            opponentCharacterImage.color = isPlayer ? inactiveSpeakerColor : activeSpeakerColor;
            speakerNameText.text = isPlayer
                ? _manager.SelectedBattler.characterName
                : _manager.Opponent.characterName;
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

        // ─── Prompt Construction ────────────────────────────────────────────

        private string BuildPlayerSystemPrompt()
        {
            var player = _manager.SelectedBattler;
            var opponent = _manager.Opponent;
            var sb = new StringBuilder();

            sb.AppendLine($"You are {player.characterName}, a skilled debater.");
            sb.AppendLine(player.systemPromptLore);
            sb.AppendLine($"Your speaking style: {player.voiceTone}");
            sb.AppendLine();

            // Character skills
            if (player.skills != null && player.skills.Length > 0)
            {
                sb.AppendLine("YOUR SKILLS:");
                foreach (var skill in player.skills)
                {
                    sb.AppendLine($"- {skill.skillName}: {skill.description}");
                    sb.AppendLine($"  [Technique: {skill.promptModifier}]");
                }
                sb.AppendLine();
            }

            // Strategy from strategy phase
            var strategy = _manager.Strategy;
            if (!string.IsNullOrWhiteSpace(strategy.StrategyText))
            {
                sb.AppendLine("YOUR STRATEGY:");
                sb.AppendLine(strategy.StrategyText);
                sb.AppendLine();
            }

            // Evidence items
            if (strategy.Items != null && strategy.Items.Count > 0)
            {
                sb.AppendLine("YOUR EVIDENCE:");
                foreach (var item in strategy.Items)
                    sb.AppendLine($"- {item.itemName} (ID: {item.itemId}): {item.description}");
                sb.AppendLine();
            }

            sb.AppendLine("RULES:");
            sb.AppendLine($"- You are debating against {opponent.characterName} to persuade them.");
            sb.AppendLine("- Use your skills and strategy naturally across turns.");
            sb.AppendLine("- When using evidence, include the tag: <evidence_used=ID>");
            sb.AppendLine("- Keep responses to 2-3 sentences of dialogue only. No narration.");
            if (player.loseConditions != null && player.loseConditions.Length > 0)
            {
                sb.Append("- NEVER do any of these (instant loss):");
                foreach (var cond in player.loseConditions)
                    sb.Append($" \"{cond}\",");
                sb.AppendLine();
            }

            return sb.ToString();
        }

        private string BuildOpponentSystemPrompt()
        {
            var player = _manager.SelectedBattler;
            var opponent = _manager.Opponent;
            var sb = new StringBuilder();

            sb.AppendLine($"You are {opponent.characterName}.");
            sb.AppendLine(opponent.systemPromptLore);
            sb.AppendLine($"Your speaking style: {opponent.voiceTone}");
            sb.AppendLine();
            sb.AppendLine("RULES:");
            sb.AppendLine($"- You are being debated by {player.characterName}.");
            sb.AppendLine("- Stay in character. Defend your position.");
            sb.AppendLine("- React naturally to persuasive arguments and evidence presented.");
            sb.AppendLine("- Keep responses to 2-3 sentences of dialogue only. No narration.");
            if (opponent.loseConditions != null && opponent.loseConditions.Length > 0)
            {
                sb.Append("- You lose if you do any of these:");
                foreach (var cond in opponent.loseConditions)
                    sb.Append($" \"{cond}\",");
                sb.AppendLine();
            }

            return sb.ToString();
        }

        private string GetPlayerThinkingEffort()
        {
            var max = ThinkingEffort.Medium;
            var player = _manager.SelectedBattler;
            if (player.skills != null)
            {
                foreach (var skill in player.skills)
                    if (skill.thinkingEffort > max)
                        max = skill.thinkingEffort;
            }
            return max.ToString().ToLower();
        }

        // ─── Auto-Battle Loop ───────────────────────────────────────────────

        private async void RunAutoBattle()
        {
            // Opponent opening
            _opponentHistory.Add(new LLMMessage("user", "The debate begins. State your opening position."));

            var opponentRequest = new LLMRequest
            {
                SystemPrompt = _opponentSystemPrompt,
                ThinkingEffort = "medium",
                History = new List<LLMMessage>(_opponentHistory)
            };

            SetSpeaker(false);
            await MovePanelForwardAsync(false);
            if (!_isBattleActive) return;

            ShowThinking(_manager.Opponent.characterName);
            var opponentResponse = await _manager.LLMService.GenerateResponseAsync(opponentRequest);
            if (!_isBattleActive) return;

            if (opponentResponse.Success)
            {
                string content = CleanDialogue(opponentResponse.Content);
                _opponentHistory.Add(new LLMMessage("model", content));
                _playerHistory.Add(new LLMMessage("user", content));
                ShowDialogue(_manager.Opponent.characterName, content, opponentResponse.ThoughtSummary);
            }

            await MovePanelBackAsync(false);
            if (!_isBattleActive) return;

            if (!_isAutoProgress)
            {
                _waitingForInput = true;
                while (_waitingForInput && _isBattleActive)
                    await Awaitable.NextFrameAsync();
                if (!_isBattleActive) return;
            }

            await RunBattleLoop();
        }

        private async void ContinueAutoBattle()
        {
            await RunBattleLoop();
        }

        private async System.Threading.Tasks.Task RunBattleLoop()
        {
            string playerThinking = GetPlayerThinkingEffort();

            while (_isBattleActive && _manager.CurrentTurn <= _manager.MaxTurns)
            {
                // ── Player's turn ──
                SetSpeaker(true);
                await MovePanelForwardAsync(true);
                if (!_isBattleActive) return;

                ShowThinking(_manager.SelectedBattler.characterName);

                var playerRequest = new LLMRequest
                {
                    SystemPrompt = _playerSystemPrompt,
                    ThinkingEffort = playerThinking,
                    History = new List<LLMMessage>(_playerHistory)
                };

                var playerResponse = await _manager.LLMService.GenerateResponseAsync(playerRequest);
                if (!_isBattleActive) return;

                if (playerResponse.Success)
                {
                    string playerContent = CleanDialogue(playerResponse.Content);
                    _playerHistory.Add(new LLMMessage("model", playerContent));
                    _opponentHistory.Add(new LLMMessage("user", playerContent));
                    ShowDialogue(_manager.SelectedBattler.characterName, playerContent, playerResponse.ThoughtSummary);

                    if (CheckLoseConditions(_manager.SelectedBattler.loseConditions, playerContent))
                    {
                        _isBattleActive = false;
                        ShowDialogue("JUDGE", "Player triggered a lose condition!", null);
                        _manager.OnBattleFinished(false);
                        return;
                    }
                }

                await MovePanelBackAsync(true);
                if (!_isBattleActive) return;

                if (!_isAutoProgress)
                {
                    _waitingForInput = true;
                    while (_waitingForInput && _isBattleActive)
                        await Awaitable.NextFrameAsync();
                    if (!_isBattleActive) return;
                }

                // ── Opponent's turn ──
                SetSpeaker(false);
                await MovePanelForwardAsync(false);
                if (!_isBattleActive) return;

                ShowThinking(_manager.Opponent.characterName);

                var opponentRequest = new LLMRequest
                {
                    SystemPrompt = _opponentSystemPrompt,
                    ThinkingEffort = "medium",
                    History = new List<LLMMessage>(_opponentHistory)
                };

                var opponentResponse = await _manager.LLMService.GenerateResponseAsync(opponentRequest);
                if (!_isBattleActive) return;

                if (opponentResponse.Success)
                {
                    string opponentContent = CleanDialogue(opponentResponse.Content);
                    _opponentHistory.Add(new LLMMessage("model", opponentContent));
                    _playerHistory.Add(new LLMMessage("user", opponentContent));
                    ShowDialogue(_manager.Opponent.characterName, opponentContent, opponentResponse.ThoughtSummary);

                    if (CheckLoseConditions(_manager.Opponent.loseConditions, opponentContent))
                    {
                        _isBattleActive = false;
                        ShowDialogue("JUDGE", "Opponent triggered a lose condition! Player wins!", null);
                        _manager.OnBattleFinished(true);
                        return;
                    }
                }

                await MovePanelBackAsync(false);
                if (!_isBattleActive) return;

                _manager.AdvanceTurn();
                UpdateTurnTracker();

                if (!_isAutoProgress)
                {
                    _waitingForInput = true;
                    while (_waitingForInput && _isBattleActive)
                        await Awaitable.NextFrameAsync();
                    if (!_isBattleActive) return;
                }
            }

            if (_isBattleActive)
            {
                await RunFinalVerdict();
            }
        }

        // ─── Judge System ───────────────────────────────────────────────────

        private bool CheckLoseConditions(string[] conditions, string dialogue)
        {
            if (conditions == null) return false;
            foreach (var cond in conditions)
            {
                if (string.IsNullOrWhiteSpace(cond)) continue;
                if (Regex.IsMatch(dialogue, Regex.Escape(cond), RegexOptions.IgnoreCase))
                    return true;
            }
            return false;
        }

        private async System.Threading.Tasks.Task RunFinalVerdict()
        {
            ShowDialogue("JUDGE", "Maximum turns reached. Delivering final verdict...", null);
            ShowThinking("Judge");

            var logBuilder = new StringBuilder();
            foreach (var entry in _manager.ConversationHistory)
                logBuilder.AppendLine($"[{entry.SpeakerName}]: {entry.SpeechText}");

            var condBuilder = new StringBuilder();
            foreach (var cond in _manager.Opponent.loseConditions)
                condBuilder.AppendLine($"- {cond}");

            string judgePrompt = "You are an impartial judge reviewing a persuasion debate. " +
                "Analyze the full conversation and determine the outcome.";

            string judgeMessage = $"DEBATE HISTORY:\n{logBuilder}\n\n" +
                $"OPPONENT ({_manager.Opponent.characterName}) LOSE CONDITIONS:\n{condBuilder}\n\n" +
                $"Did {_manager.SelectedBattler.characterName} successfully persuade {_manager.Opponent.characterName}?\n" +
                "Consider: Did the opponent show signs of yielding or conceding any lose conditions?\n" +
                "Respond with exactly one word: WIN, LOSE, or DRAW";

            var judgeRequest = new LLMRequest
            {
                SystemPrompt = judgePrompt,
                ThinkingEffort = "high"
            };
            judgeRequest.History.Add(new LLMMessage("user", judgeMessage));

            var judgeResponse = await _manager.LLMService.GenerateResponseAsync(judgeRequest);
            if (!_isBattleActive) return;

            bool playerWon = false;
            if (judgeResponse.Success)
            {
                string verdict = judgeResponse.Content.Trim().ToUpper();
                playerWon = verdict.Contains("WIN");
                ShowDialogue("JUDGE", $"Final Verdict: {verdict}", judgeResponse.ThoughtSummary);
            }
            else
            {
                ShowDialogue("JUDGE", "Could not reach a verdict. Defaulting to DRAW.", null);
            }

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

            _manager.AddConversationEntry(new ConversationEntry
            {
                SpeakerName = speaker,
                SpeechText = text,
                Timestamp = $"Turn {_manager.CurrentTurn}",
                IsPlayer = speaker == _manager.SelectedBattler.characterName,
                EvidenceUsed = ParseTag(text, "evidence_used")
            });
        }

        private string CleanDialogue(string raw)
        {
            var match = Regex.Match(raw, @"\[Thought Process\].*?\n(.+)", RegexOptions.Singleline);
            return match.Success ? match.Groups[1].Value.Trim() : raw.Trim();
        }

        private string ParseTag(string text, string tagName)
        {
            var match = Regex.Match(text, $"<{tagName}=([^>]+)>");
            return match.Success ? match.Groups[1].Value : null;
        }
    }
}
