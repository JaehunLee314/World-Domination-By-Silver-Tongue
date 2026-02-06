using System;
using UnityEngine;

namespace InfoGatherPhase
{
    public class DialogueManager : MonoBehaviour
    {
        [SerializeField] private DialogueUI dialogueUI;
        [SerializeField] private SpeakerPanel speakerPanel;

        public bool IsActive { get; private set; }

        public event Action OnDialogueStarted;
        public event Action OnDialogueEnded;

        private DialogueEvent currentEvent;
        private int currentLineIndex;

        public void StartDialogue(DialogueEvent dialogueEvent)
        {
            if (dialogueEvent == null || dialogueEvent.lines.Length == 0) return;

            currentEvent = dialogueEvent;
            currentLineIndex = 0;
            IsActive = true;

            OnDialogueStarted?.Invoke();
            ShowCurrentLine();
        }

        public void AdvanceLine()
        {
            if (!IsActive) return;

            currentLineIndex++;

            if (currentLineIndex >= currentEvent.lines.Length)
            {
                EndDialogue();
                return;
            }

            ShowCurrentLine();
        }

        private void ShowCurrentLine()
        {
            DialogueLine line = currentEvent.lines[currentLineIndex];
            dialogueUI.Show(line.speakerName, line.text);
            speakerPanel.Show(line.speakerPortrait);
        }

        private void EndDialogue()
        {
            IsActive = false;
            currentEvent = null;
            currentLineIndex = 0;

            dialogueUI.Hide();
            speakerPanel.Hide();

            OnDialogueEnded?.Invoke();
        }
    }
}
