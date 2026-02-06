using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SilverTongue.Data;

namespace SilverTongue.BattleScene
{
    public class AgendaSlotUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI slotNumberText;
        [SerializeField] private TMP_InputField pointTextField;
        [SerializeField] private Image skillSlotImage;
        [SerializeField] private Image itemSlotImage;
        [SerializeField] private TextMeshProUGUI skillSlotLabel;
        [SerializeField] private TextMeshProUGUI itemSlotLabel;

        [Header("Data")]
        public ItemSO assignedSkill;
        public ItemSO assignedItem;
        public string pointText;

        public void Setup(int slotNumber)
        {
            slotNumberText.text = $"#{slotNumber}";
            pointTextField.characterLimit = 150; // ~30 words
            pointTextField.onValueChanged.RemoveAllListeners();
            pointTextField.onValueChanged.AddListener(text => pointText = text);
            ClearSlot();
        }

        public void AssignSkill(ItemSO skill)
        {
            assignedSkill = skill;
            skillSlotLabel.text = skill.skillName;
            if (skill.itemImage != null)
                skillSlotImage.sprite = skill.itemImage;
            skillSlotImage.color = Color.white;
        }

        public void AssignItem(ItemSO item)
        {
            assignedItem = item;
            itemSlotLabel.text = item.itemName;
            if (item.itemImage != null)
                itemSlotImage.sprite = item.itemImage;
            itemSlotImage.color = Color.white;
        }

        public void ClearSlot()
        {
            assignedSkill = null;
            assignedItem = null;
            pointText = "";
            pointTextField.text = "";
            skillSlotLabel.text = "Skill";
            itemSlotLabel.text = "Item";
            skillSlotImage.color = new Color(0.8f, 0.8f, 0.8f, 0.5f);
            itemSlotImage.color = new Color(0.8f, 0.8f, 0.8f, 0.5f);
        }
    }
}
