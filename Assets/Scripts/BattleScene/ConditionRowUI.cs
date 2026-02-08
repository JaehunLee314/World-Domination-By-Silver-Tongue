using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SilverTongue.BattleScene
{
    public class ConditionRowUI : MonoBehaviour
    {
        [SerializeField] private Image statusIcon;
        [SerializeField] private TextMeshProUGUI conditionText;
        [SerializeField] private Color metColor = new Color(0.2f, 0.8f, 0.3f);
        [SerializeField] private Color unmetColor = new Color(0.5f, 0.5f, 0.5f);

        public void Setup(string label)
        {
            conditionText.text = label;
            SetMet(false);
        }

        public void SetMet(bool isMet)
        {
            statusIcon.color = isMet ? metColor : unmetColor;
        }
    }
}
