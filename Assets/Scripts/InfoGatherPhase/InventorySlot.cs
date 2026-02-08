using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace InfoGatherPhase
{
    public class InventorySlot : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI nameLabel;
        [SerializeField] private Button slotButton;

        private ItemData itemData;
        private Action<ItemData> onClick;

        public void Setup(ItemData data, Action<ItemData> clickCallback)
        {
            itemData = data;
            onClick = clickCallback;

            iconImage.sprite = TextureToSprite(data.icon);
            nameLabel.text = data.itemName;

            slotButton.onClick.RemoveAllListeners();
            slotButton.onClick.AddListener(() => onClick?.Invoke(itemData));
        }

        private static Sprite TextureToSprite(Texture2D tex)
        {
            if (tex == null) return null;
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        }
    }
}
