using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SilverTongue.Data;

namespace SilverTongue.BattleScene
{
    public class CharacterCardUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Image profileImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI loseConditionsText;
        [SerializeField] private TextMeshProUGUI skillsText;
        [SerializeField] private Button selectButton;

        private CharacterSO _character;
        private Action<CharacterSO> _onSelect;

        public void Setup(CharacterSO character, Action<CharacterSO> onSelect)
        {
            _character = character;
            _onSelect = onSelect;

            nameText.text = character.characterName;

            string conditions = "";
            for (int i = 0; i < character.loseConditions.Length; i++)
            {
                conditions += $"- {character.loseConditions[i]}\n";
            }
            loseConditionsText.text = conditions.TrimEnd();

            if (skillsText != null && character.skills != null)
            {
                string skills = "";
                foreach (var skill in character.skills)
                    skills += $"- {skill.skillName}: {skill.description}\n";
                skillsText.text = skills.TrimEnd();
            }

            if (character.profileImage != null)
                profileImage.sprite = character.profileImage;

            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(() => _onSelect?.Invoke(_character));
        }
    }
}
