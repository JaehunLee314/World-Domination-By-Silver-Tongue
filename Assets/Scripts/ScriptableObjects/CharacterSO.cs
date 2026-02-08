using UnityEngine;

namespace SilverTongue.Data
{
    public enum ThinkingEffort
    {
        Low,
        Medium,
        High
    }

    public enum Emotion
    {
        Neutral,
        Happy,
        Angry,
        Sad,
        Surprised,
        Concerned
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

    [System.Serializable]
    public struct EmotionSprite
    {
        public Emotion emotion;
        public Sprite sprite;
    }

    [CreateAssetMenu(fileName = "NewCharacter", menuName = "SilverTongue/Character")]
    public class CharacterSO : ScriptableObject
    {
        [Header("Identity")]
        public string characterName;
        public Sprite profileImage;

        [Header("Emotion Sprites")]
        [Tooltip("Map emotions to sprites. Falls back to profileImage if not found.")]
        public EmotionSprite[] emotionSprites;

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

        public Sprite GetEmotionSprite(Emotion emotion)
        {
            if (emotionSprites != null)
            {
                foreach (var es in emotionSprites)
                {
                    if (es.emotion == emotion)
                        return es.sprite;
                }
            }
            return profileImage;
        }
    }
}
