using UnityEngine;

namespace SilverTongue.Data
{
    [CreateAssetMenu(fileName = "NewCharacter", menuName = "SilverTongue/Character")]
    public class CharacterSO : ScriptableObject
    {
        [Header("Identity")]
        public string characterName;
        public Sprite profileImage;

        [Header("Stats")]
        [TextArea(1, 3)]
        public string personality;
        [Range(1, 10)]
        public int intelligence = 5;

        [Header("Lore")]
        [TextArea(3, 10)]
        public string systemPromptLore;
        public string voiceTone;

        [Header("Lose Conditions")]
        [Tooltip("Phrases or behaviors that cause immediate failure")]
        public string[] loseConditions = new string[3];
    }
}
