using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SilverTongue.BattleSystem;

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

            bool hasIndicator = !string.IsNullOrEmpty(entry.EvidenceUsed);
            indicatorContainer.SetActive(hasIndicator);
            if (hasIndicator)
            {
                indicatorText.text = $"[Evidence: {entry.EvidenceUsed}]";
            }
        }
    }
}
