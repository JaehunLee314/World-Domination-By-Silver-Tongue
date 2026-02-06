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
        [SerializeField] private Button pauseButton;
        [SerializeField] private TextMeshProUGUI pauseButtonText;
        [SerializeField] private Button logButton;

        [Header("Character Display")]
        [SerializeField] private Image playerCharacterImage;
        [SerializeField] private Image opponentCharacterImage;
        [SerializeField] private TextMeshProUGUI playerNameLabel;
        [SerializeField] private TextMeshProUGUI opponentNameLabel;

        [Header("Dialogue Area")]
        [SerializeField] private TextMeshProUGUI speakerNameText;
        [SerializeField] private TextMeshProUGUI dialogueText;
        [SerializeField] private ScrollRect logScrollRect;
        [SerializeField] private TextMeshProUGUI logText;

        [Header("Visual Settings")]
        [SerializeField] private Color activeSpeakerColor = Color.white;
        [SerializeField] private Color inactiveSpeakerColor = new Color(0.3f, 0.3f, 0.3f, 1f);

        private BattleSceneManager _manager;
        private bool _isPaused;
        private bool _isBattleActive;
        private List<string> _battleLog = new List<string>();

        // Cumulative LLM conversation histories (mirrored perspectives)
        private List<LLMMessage> _playerHistory = new List<LLMMessage>();
        private List<LLMMessage> _opponentHistory = new List<LLMMessage>();
        private string _playerSystemPrompt;
        private string _opponentSystemPrompt;

        public void Initialize(BattleSceneManager manager)
        {
            _manager = manager;
            _isPaused = false;
            _isBattleActive = true;
            _battleLog.Clear();
            _playerHistory.Clear();
            _opponentHistory.Clear();

            SetupDisplay();
            SetupButtons();
            UpdateTurnTracker();

            _playerSystemPrompt = BuildPlayerSystemPrompt();
            _opponentSystemPrompt = BuildOpponentSystemPrompt();

            SetSpeaker(false);
            RunAutoBattle();
        }

        public void Resume()
        {
            _isPaused = false;
            _isBattleActive = true;
            UpdateTurnTracker();

            // Rebuild player prompt in case agenda changed during pause
            _playerSystemPrompt = BuildPlayerSystemPrompt();
            ContinueAutoBattle();
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
            logText.text = "";
        }

        private void SetupButtons()
        {
            pauseButton.onClick.RemoveAllListeners();
            pauseButton.onClick.AddListener(OnPause);

            if (logButton != null)
            {
                logButton.onClick.RemoveAllListeners();
                logButton.onClick.AddListener(() => _manager.ShowConversationHistory());
            }
        }

        private void UpdateTurnTracker()
        {
            turnTrackerText.text = $"Turn {_manager.CurrentTurn}/{_manager.MaxTurns}";
        }

        public void SetSpeaker(bool isPlayer)
        {
            playerCharacterImage.color = isPlayer ? activeSpeakerColor : inactiveSpeakerColor;
            opponentCharacterImage.color = isPlayer ? inactiveSpeakerColor : activeSpeakerColor;
            speakerNameText.text = isPlayer
                ? _manager.SelectedBattler.characterName
                : _manager.Opponent.characterName;
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

            // Agenda from strategy phase
            var agenda = _manager.Agenda;
            if (agenda != null && agenda.Length > 0)
            {
                sb.AppendLine("YOUR DEBATE AGENDA:");
                for (int i = 0; i < agenda.Length; i++)
                {
                    var entry = agenda[i];
                    if (string.IsNullOrWhiteSpace(entry.PointText) && entry.Skill == null && entry.Evidence == null)
                        continue;

                    sb.AppendLine($"{i + 1}. {(string.IsNullOrWhiteSpace(entry.PointText) ? "(no talking point)" : entry.PointText)}");
                    if (entry.Skill != null)
                        sb.AppendLine($"   [Skill: {entry.Skill.skillName} - {entry.Skill.promptModifier}]");
                    if (entry.Evidence != null)
                        sb.AppendLine($"   [Evidence: {entry.Evidence.itemName} (ID: {entry.Evidence.itemId}) - {entry.Evidence.description}]");
                }
                sb.AppendLine();
            }

            sb.AppendLine("RULES:");
            sb.AppendLine($"- You are debating against {opponent.characterName} to persuade them.");
            sb.AppendLine("- Weave your agenda points, evidence, and skills naturally across turns.");
            sb.AppendLine("- When using evidence, include the tag: <evidence_used=ID>");
            sb.AppendLine("- When using a skill technique, include the tag: <skill_used=ID>");
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
            var agenda = _manager.Agenda;
            if (agenda != null)
            {
                foreach (var entry in agenda)
                    if (entry.Skill != null && entry.Skill.thinkingEffort > max)
                        max = entry.Skill.thinkingEffort;
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
            ShowThinking(_manager.Opponent.characterName);
            var opponentResponse = await _manager.LLMService.GenerateResponseAsync(opponentRequest);
            if (_isPaused || !_isBattleActive) return;

            if (opponentResponse.Success)
            {
                string content = CleanDialogue(opponentResponse.Content);
                _opponentHistory.Add(new LLMMessage("model", content));
                _playerHistory.Add(new LLMMessage("user", content));
                ShowDialogue(_manager.Opponent.characterName, content);
            }

            // Start the battle loop
            await RunBattleLoop();
        }

        private async void ContinueAutoBattle()
        {
            await RunBattleLoop();
        }

        private async System.Threading.Tasks.Task RunBattleLoop()
        {
            string playerThinking = GetPlayerThinkingEffort();

            while (_isBattleActive && !_isPaused && _manager.CurrentTurn <= _manager.MaxTurns)
            {
                // ── Player's turn ──
                SetSpeaker(true);
                ShowThinking(_manager.SelectedBattler.characterName);

                var playerRequest = new LLMRequest
                {
                    SystemPrompt = _playerSystemPrompt,
                    ThinkingEffort = playerThinking,
                    History = new List<LLMMessage>(_playerHistory)
                };

                var playerResponse = await _manager.LLMService.GenerateResponseAsync(playerRequest);
                if (_isPaused || !_isBattleActive) return;

                if (playerResponse.Success)
                {
                    string playerContent = CleanDialogue(playerResponse.Content);
                    _playerHistory.Add(new LLMMessage("model", playerContent));
                    _opponentHistory.Add(new LLMMessage("user", playerContent));
                    ShowDialogue(_manager.SelectedBattler.characterName, playerContent);

                    // Fast Check: did player trigger their own lose conditions?
                    if (CheckLoseConditions(_manager.SelectedBattler.loseConditions, playerContent))
                    {
                        _isBattleActive = false;
                        AddToLog("[JUDGE] Player triggered a lose condition!");
                        _manager.OnBattleFinished(false);
                        return;
                    }
                }

                if (_isPaused || !_isBattleActive) return;

                // ── Opponent's turn ──
                SetSpeaker(false);
                ShowThinking(_manager.Opponent.characterName);

                var opponentRequest = new LLMRequest
                {
                    SystemPrompt = _opponentSystemPrompt,
                    ThinkingEffort = "medium",
                    History = new List<LLMMessage>(_opponentHistory)
                };

                var opponentResponse = await _manager.LLMService.GenerateResponseAsync(opponentRequest);
                if (_isPaused || !_isBattleActive) return;

                if (opponentResponse.Success)
                {
                    string opponentContent = CleanDialogue(opponentResponse.Content);
                    _opponentHistory.Add(new LLMMessage("model", opponentContent));
                    _playerHistory.Add(new LLMMessage("user", opponentContent));
                    ShowDialogue(_manager.Opponent.characterName, opponentContent);

                    // Fast Check: did opponent trigger their lose conditions?
                    if (CheckLoseConditions(_manager.Opponent.loseConditions, opponentContent))
                    {
                        _isBattleActive = false;
                        AddToLog("[JUDGE] Opponent triggered a lose condition! Player wins!");
                        _manager.OnBattleFinished(true);
                        return;
                    }
                }

                _manager.AdvanceTurn();
                UpdateTurnTracker();
            }

            // Max turns reached → Final Verdict
            if (_isBattleActive && !_isPaused)
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
            AddToLog("[JUDGE] Maximum turns reached. Delivering final verdict...");
            ShowThinking("Judge");

            // Build the full debate log for the judge
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
                AddToLog($"[JUDGE] Final Verdict: {verdict}");
            }
            else
            {
                AddToLog("[JUDGE] Could not reach a verdict. Defaulting to DRAW.");
            }

            _isBattleActive = false;
            _manager.OnBattleFinished(playerWon);
        }

        // ─── Dialogue Display ───────────────────────────────────────────────

        private void ShowThinking(string speaker)
        {
            speakerNameText.text = speaker;
            dialogueText.text = "<i>Thinking...</i>";
        }

        public void ShowDialogue(string speaker, string text)
        {
            speakerNameText.text = speaker;
            dialogueText.text = text;
            AddToLog($"[{speaker}]: {text}");

            _manager.AddConversationEntry(new ConversationEntry
            {
                SpeakerName = speaker,
                SpeechText = text,
                Timestamp = $"Turn {_manager.CurrentTurn}",
                IsPlayer = speaker == _manager.SelectedBattler.characterName,
                EvidenceUsed = ParseTag(text, "evidence_used"),
                SkillUsed = ParseTag(text, "skill_used")
            });
        }

        private string CleanDialogue(string raw)
        {
            // Strip [Thought Process] prefix if present
            var match = Regex.Match(raw, @"\[Thought Process\].*?\n(.+)", RegexOptions.Singleline);
            return match.Success ? match.Groups[1].Value.Trim() : raw.Trim();
        }

        private string ParseTag(string text, string tagName)
        {
            var match = Regex.Match(text, $"<{tagName}=([^>]+)>");
            return match.Success ? match.Groups[1].Value : null;
        }

        private void AddToLog(string entry)
        {
            _battleLog.Add(entry);
            logText.text = string.Join("\n", _battleLog);

            Canvas.ForceUpdateCanvases();
            logScrollRect.verticalNormalizedPosition = 0f;
        }

        private void OnPause()
        {
            _isPaused = true;
            _manager.PauseToStrategy();
        }
    }
}
