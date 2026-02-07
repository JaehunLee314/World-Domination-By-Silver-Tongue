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
    }
}
