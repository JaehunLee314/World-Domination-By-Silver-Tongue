using UnityEngine;

namespace SilverTongue.Data
{
    public enum ThinkingEffort
    {
        Low,
        Medium,
        High
    }

    [System.Serializable]
    public struct SkillData
    {
        public string skillName;
        [TextArea(1, 3)]
        public string description;
        [TextArea(2, 5)]
        public string promptModifier;
        public ThinkingEffort thinkingEffort;
    }

    [CreateAssetMenu(fileName = "NewCharacter", menuName = "SilverTongue/Character")]
    public class CharacterSO : ScriptableObject
    {
        [Header("Identity")]
        public string characterName;
        public Sprite profileImage;

        [Header("Stats")]
        [Tooltip("Controls LLM token limit: Low (Mob), Medium (Elite), High (Boss)")]
        public ThinkingEffort thinkingEffort = ThinkingEffort.Medium;

        [Header("Skills")]
        public SkillData[] skills;

        [Header("Lore")]
        [TextArea(3, 10)]
        public string lore;

        [Header("Lose Conditions")]
        [Tooltip("Phrases or behaviors that cause immediate failure")]
        public string[] loseConditions = new string[3];
    }
}
