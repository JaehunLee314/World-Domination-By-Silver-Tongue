using UnityEngine;
using TMPro;

namespace InfoGatherPhase
{
    public class DialogueUI : MonoBehaviour
    {
        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI lineText;

        public void Show(string speaker, string text)
        {
            nameText.text = speaker;
            lineText.text = text;
            dialoguePanel.SetActive(true);
        }

        public void Hide()
        {
            dialoguePanel.SetActive(false);
        }
    }
}
