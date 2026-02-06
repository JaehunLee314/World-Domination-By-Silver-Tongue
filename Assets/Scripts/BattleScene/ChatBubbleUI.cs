using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SilverTongue.BattleScene
{
    public class ChatBubbleUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI speakerNameText;
        [SerializeField] private TextMeshProUGUI speechText;
        [SerializeField] private TextMeshProUGUI timestampText;
        [SerializeField] private GameObject indicatorContainer;
        [SerializeField] private TextMeshProUGUI indicatorText;
        [SerializeField] private HorizontalLayoutGroup rootLayout;
        [SerializeField] private Image bubbleBackground;

        public void Setup(ConversationEntry entry)
        {
            speakerNameText.text = entry.SpeakerName;
            speechText.text = entry.SpeechText;
            timestampText.text = entry.Timestamp;

            rootLayout.childAlignment = entry.IsPlayer
                ? TextAnchor.UpperLeft
                : TextAnchor.UpperRight;

            bubbleBackground.color = entry.IsPlayer
                ? new Color(0.2f, 0.4f, 0.6f, 0.9f)
                : new Color(0.5f, 0.2f, 0.2f, 0.9f);

            bool hasIndicator = !string.IsNullOrEmpty(entry.EvidenceUsed)
                             || !string.IsNullOrEmpty(entry.SkillUsed);
            indicatorContainer.SetActive(hasIndicator);
            if (hasIndicator)
            {
                string indicators = "";
                if (!string.IsNullOrEmpty(entry.EvidenceUsed))
                    indicators += $"[Evidence: {entry.EvidenceUsed}] ";
                if (!string.IsNullOrEmpty(entry.SkillUsed))
                    indicators += $"[Skill: {entry.SkillUsed}]";
                indicatorText.text = indicators.Trim();
            }
        }
    }
}
