using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace SilverTongue.StartScene
{
    public class StartView : MonoBehaviour
    {
        [Header("Title Elements")]
        [SerializeField] private RectTransform worldDominationText;
        [SerializeField] private RectTransform byAText;
        [SerializeField] private RectTransform silverTongueText;

        [Header("Button")]
        [SerializeField] private Button startButton;
        [SerializeField] private CanvasGroup buttonCanvasGroup;

        [Header("Timing")]
        [SerializeField] private float initialDelay = 0.4f;
        [SerializeField] private float slideDuration = 0.7f;
        [SerializeField] private float staggerDelay = 0.3f;

        private StartSceneManager _manager;
        private bool _isTransitioning;

        public void Initialize(StartSceneManager manager)
        {
            _manager = manager;
            startButton.onClick.RemoveAllListeners();
            startButton.onClick.AddListener(OnStartButtonClicked);

            SetupInitialState();
            PlayIntroSequence();
        }

        private void SetupInitialState()
        {
            // "World Domination" starts off-screen left
            worldDominationText.anchoredPosition = new Vector2(-2400f, worldDominationText.anchoredPosition.y);

            // "By a" starts above its mask (hidden)
            byAText.anchoredPosition = new Vector2(byAText.anchoredPosition.x, 100f);

            // "Silver Tongue" starts off-screen right
            silverTongueText.anchoredPosition = new Vector2(2000f, silverTongueText.anchoredPosition.y);

            // Button starts invisible
            buttonCanvasGroup.alpha = 0f;
            startButton.interactable = false;
        }

        private void PlayIntroSequence()
        {
            var seq = DOTween.Sequence();

            // "World Domination" flies in from left
            seq.AppendInterval(initialDelay);
            seq.Append(worldDominationText
                .DOAnchorPosX(0f, slideDuration)
                .SetEase(Ease.OutBack, 1.1f));

            // "By a" drops into view from its mask
            seq.AppendInterval(staggerDelay);
            seq.Append(byAText
                .DOAnchorPosY(0f, slideDuration * 0.7f)
                .SetEase(Ease.OutBounce));

            // "Silver Tongue" slides in from right
            seq.AppendInterval(staggerDelay);
            seq.Append(silverTongueText
                .DOAnchorPosX(0f, slideDuration)
                .SetEase(Ease.OutBack, 1.1f));

            // Button fades in
            seq.AppendInterval(0.2f);
            seq.Append(buttonCanvasGroup.DOFade(1f, 0.5f));
            seq.AppendCallback(() => startButton.interactable = true);
        }

        private void OnStartButtonClicked()
        {
            if (_isTransitioning) return;
            _isTransitioning = true;
            startButton.interactable = false;

            PlayButtonFlicker();
        }

        private void PlayButtonFlicker()
        {
            var seq = DOTween.Sequence();

            // Flicker the button 5 times rapidly
            for (int i = 0; i < 5; i++)
            {
                seq.Append(buttonCanvasGroup.DOFade(0.1f, 0.06f));
                seq.Append(buttonCanvasGroup.DOFade(1f, 0.06f));
            }

            // Final fade out
            seq.Append(buttonCanvasGroup.DOFade(0f, 0.15f));
            seq.AppendInterval(0.1f);
            seq.AppendCallback(() => _manager.OnStartClicked());
        }

        private void OnDestroy()
        {
            DOTween.Kill(worldDominationText);
            DOTween.Kill(byAText);
            DOTween.Kill(silverTongueText);
            DOTween.Kill(buttonCanvasGroup);
        }
    }
}
