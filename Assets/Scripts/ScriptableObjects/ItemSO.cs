using UnityEngine;

namespace SilverTongue.Data
{
    public enum ItemType
    {
        Evidence,
        SkillBook
    }

    public enum ThinkingEffort
    {
        Low,
        Medium,
        High
    }

    [CreateAssetMenu(fileName = "NewItem", menuName = "SilverTongue/Item")]
    public class ItemSO : ScriptableObject
    {
        [Header("Common")]
        public string itemId;
        public string itemName;
        public Sprite itemImage;
        public ItemType itemType;
        [TextArea(2, 5)]
        public string description;

        [Header("Skill Book Only")]
        public string skillName;
        [TextArea(2, 5)]
        public string promptModifier;
        public ThinkingEffort thinkingEffort = ThinkingEffort.Medium;
    }
}
