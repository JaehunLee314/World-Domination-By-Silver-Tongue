using UnityEngine;

namespace SilverTongue.Data
{
    [CreateAssetMenu(fileName = "NewItem", menuName = "SilverTongue/Item")]
    public class ItemSO : ScriptableObject
    {
        [Header("Item")]
        public string itemId;
        public string itemName;
        public Sprite itemImage;
        [TextArea(2, 5)]
        public string description;

        [Header("Battle Injection")]
        [Tooltip("Text injected into LLM context when equipped. e.g. [ITEM: Name] FACT: ...")]
        [TextArea(2, 5)]
        public string factInjection;
    }
}
