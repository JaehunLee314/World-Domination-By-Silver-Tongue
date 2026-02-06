using UnityEngine;

namespace InfoGatherPhase
{
    public enum ItemType
    {
        Document,
        Photo,
        Object,
        Clue
    }

    [CreateAssetMenu(fileName = "NewItemData", menuName = "InfoGather/Item Data")]
    public class ItemData : ScriptableObject
    {
        public string itemName;
        public ItemType itemType;
        public Sprite icon;
        [TextArea(2, 5)]
        public string description;
        public DialogueEvent pickupDialogue;
    }
}
