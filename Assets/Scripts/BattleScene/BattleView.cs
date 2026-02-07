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

        // ─── Public API (called by BattleSceneManager) ───────────────────

        public void Initialize(BattleSceneManager manager)
        {
            _manager = manager;
            _isAutoProgress = false;
            _isBattleActive = true;
            _waitingForInput = false;

            CacheOriginalPanelPositions();
            SetupDisplay();
            SetupButtons();
            UpdateTurnTracker();
            UpdateAutoProgressUI();
            UpdateSanityBar();

            SetSpeaker(false);
        }

        public void Resume()
        {
            _isAutoProgress = false;
            _isBattleActive = true;
            _waitingForInput = false;
            UpdateTurnTracker();
            UpdateAutoProgressUI();
            UpdateSanityBar();
        }

        public void SetBattleActive(bool active)
        {
            _isBattleActive = active;
            if (!active)
                _waitingForInput = false;
        }

        // ─── Display Methods (called by manager during battle loop) ──────

        public void ShowThinking(string speaker)
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
        }

        public void ShowJudgeDialogue(string text, JudgeResult result)
        {
            speakerNameText.text = "JUDGE";
            dialogueText.text = text;
            thoughtText.text = !string.IsNullOrEmpty(result.reasoning) ? result.reasoning : "";
        }

        public void UpdateTurnTracker(string phaseLabel = null)
        {
            if (string.IsNullOrEmpty(phaseLabel))
                turnTrackerText.text = $"Turn {_manager.CurrentTurn}/{_manager.MaxTurns}";
            else
                turnTrackerText.text = $"Turn {_manager.CurrentTurn}/{_manager.MaxTurns} ({phaseLabel})";
        }

        public void UpdateSanityBar()
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

        public void SetSpeaker(bool isPlayer)
        {
            playerCharacterImage.color = isPlayer ? activeSpeakerColor : inactiveSpeakerColor;
            opponentCharacterImage.color = isPlayer ? inactiveSpeakerColor : activeSpeakerColor;
            speakerNameText.text = isPlayer
                ? _manager.SelectedBattler.characterName
                : _manager.Opponent.characterName;
        }

        public void SetJudgeSpeaker()
        {
            playerCharacterImage.color = inactiveSpeakerColor;
            opponentCharacterImage.color = inactiveSpeakerColor;
            speakerNameText.text = "JUDGE";
        }

        // ─── Async Helpers (called by manager) ──────────────────────────

        public async System.Threading.Tasks.Task WaitForInputAsync()
        {
            if (_isAutoProgress) return;
            _waitingForInput = true;
            while (_waitingForInput && _isBattleActive)
                await Awaitable.NextFrameAsync();
        }

        public async System.Threading.Tasks.Task MovePanelForwardAsync(bool isPlayer)
        {
            if (isPlayer && playerPanel != null)
                await SmoothMovePanelAsync(playerPanel, _playerForwardAnchorMin, _playerForwardAnchorMax, panelMoveDuration);
            else if (!isPlayer && opponentPanel != null)
                await SmoothMovePanelAsync(opponentPanel, _opponentForwardAnchorMin, _opponentForwardAnchorMax, panelMoveDuration);
        }

        public async System.Threading.Tasks.Task MovePanelBackAsync(bool isPlayer)
        {
            if (isPlayer && playerPanel != null)
                await SmoothMovePanelAsync(playerPanel, _playerPanelOrigAnchorMin, _playerPanelOrigAnchorMax, panelMoveDuration);
            else if (!isPlayer && opponentPanel != null)
                await SmoothMovePanelAsync(opponentPanel, _opponentPanelOrigAnchorMin, _opponentPanelOrigAnchorMax, panelMoveDuration);
        }

        // ─── Internal Setup ──────────────────────────────────────────────

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

        // ─── Button Handlers ─────────────────────────────────────────────

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
            _manager.PauseToStrategy();
        }

        private void OnDialogueAreaClicked()
        {
            if (!_isAutoProgress && _waitingForInput)
                _waitingForInput = false;
        }

        // ─── Smooth Panel Movement ───────────────────────────────────────

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
    }
}
