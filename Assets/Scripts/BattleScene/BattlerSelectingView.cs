using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SilverTongue.Data;
using System.Collections;

namespace SilverTongue.BattleScene
{
    public class BattlerSelectingView : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Transform characterCardContainer;
        [SerializeField] private GameObject characterCardPrefab;
        [SerializeField] private ScrollRect scrollRect;

        [Header("Confirmation Popup")]
        [SerializeField] private GameObject confirmationPopup;
        [SerializeField] private Image confirmPopupImage;
        [SerializeField] private TextMeshProUGUI confirmPopupName;
        [SerializeField] private Button confirmYesButton;
        [SerializeField] private Button confirmNoButton;

        private BattleSceneManager _manager;
        private CharacterSO _pendingSelection;

        public void Initialize(BattleSceneManager manager)
        {
            _manager = manager;
            confirmationPopup.SetActive(false);

            confirmYesButton.onClick.RemoveAllListeners();
            confirmYesButton.onClick.AddListener(OnConfirmYes);
            confirmNoButton.onClick.RemoveAllListeners();
            confirmNoButton.onClick.AddListener(OnConfirmNo);

            PopulateCharacters();
        }

        private void PopulateCharacters()
        {
            // Clear existing cards
            foreach (Transform child in characterCardContainer)
            {
                Destroy(child.gameObject);
            }

            var characters = GameManager.Instance != null
                ? GameManager.Instance.availableCharacters
                : null;

            if (characters == null || characters.Length == 0)
            {
                Debug.LogWarning("[BattlerSelectingView] No characters available");
                return;
            }

            foreach (var character in characters)
            {
                var cardObj = Instantiate(characterCardPrefab, characterCardContainer);
                var card = cardObj.GetComponent<CharacterCardUI>();
                if (card != null)
                {
                    card.Setup(character, OnCharacterClicked);
                }
            }

            // Force layout rebuild after one frame
            StartCoroutine(ForceLayoutRebuild());
        }

        private IEnumerator ForceLayoutRebuild()
        {
            yield return null;
            if (characterCardContainer is RectTransform contentRt)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(contentRt);
            }
            if (scrollRect != null)
            {
                Canvas.ForceUpdateCanvases();
                scrollRect.horizontalNormalizedPosition = 0f;
            }
        }

        private void OnCharacterClicked(CharacterSO character)
        {
            _pendingSelection = character;
            confirmPopupName.text = $"Select {character.characterName}?";
            if (character.profileImage != null)
                confirmPopupImage.sprite = character.profileImage;
            confirmationPopup.SetActive(true);
        }

        public void OnConfirmYes()
        {
            confirmationPopup.SetActive(false);
            _manager.OnBattlerSelected(_pendingSelection);
        }

        public void OnConfirmNo()
        {
            confirmationPopup.SetActive(false);
            _pendingSelection = null;
        }
    }
}
