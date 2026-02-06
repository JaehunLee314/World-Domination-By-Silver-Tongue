using System.Collections.Generic;
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

        [Header("Player Action")]
        [SerializeField] private TMP_InputField actionInputField;
        [SerializeField] private Button sendActionButton;

        [Header("Visual Settings")]
        [SerializeField] private Color activeSpeakerColor = Color.white;
        [SerializeField] private Color inactiveSpeakerColor = new Color(0.3f, 0.3f, 0.3f, 1f);

        private BattleSceneManager _manager;
        private bool _isPaused;
        private bool _isBattleActive;
        private List<string> _battleLog = new List<string>();

        public void Initialize(BattleSceneManager manager)
        {
            _manager = manager;
            _isPaused = false;
            _isBattleActive = true;
            _battleLog.Clear();

            SetupDisplay();
            SetupButtons();
            UpdateTurnTracker();

            SetSpeaker(false);
            StartBattle();
        }

        public void Resume()
        {
            _isPaused = false;
            _isBattleActive = true;
            UpdateTurnTracker();
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

            sendActionButton.onClick.RemoveAllListeners();
            sendActionButton.onClick.AddListener(OnSendAction);

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

        private async void StartBattle()
        {
            var request = new LLMRequest
            {
                SystemPrompt = _manager.Opponent.systemPromptLore,
                ThinkingEffort = "medium"
            };
            request.History.Add(new LLMMessage("user", "The debate begins. State your opening position."));

            SetSpeaker(false);
            var response = await _manager.LLMService.GenerateResponseAsync(request);
            if (response.Success && !_isPaused)
            {
                ShowDialogue(_manager.Opponent.characterName, response.Content);
            }
        }

        private async void OnSendAction()
        {
            if (_isPaused || !_isBattleActive) return;

            string playerAction = actionInputField.text;
            if (string.IsNullOrWhiteSpace(playerAction)) return;

            actionInputField.text = "";

            SetSpeaker(true);
            ShowDialogue(_manager.SelectedBattler.characterName, playerAction);

            SetSpeaker(false);
            var request = new LLMRequest
            {
                SystemPrompt = _manager.Opponent.systemPromptLore,
                ThinkingEffort = "medium"
            };
            request.History.Add(new LLMMessage("user", playerAction));

            var response = await _manager.LLMService.GenerateResponseAsync(request);
            if (response.Success && !_isPaused)
            {
                ShowDialogue(_manager.Opponent.characterName, response.Content);
                _manager.AdvanceTurn();
                UpdateTurnTracker();

                if (_manager.CurrentTurn > _manager.MaxTurns)
                {
                    _isBattleActive = false;
                    _manager.OnBattleFinished(false);
                }
            }
        }
    }
}
