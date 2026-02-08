using System;
using UnityEngine;

namespace InfoGatherPhase
{
    public class DialogueManager : MonoBehaviour
    {
        [SerializeField] private DialogueUI dialogueUI;

        public bool IsActive { get; private set; }

        public event Action OnDialogueStarted;
        public event Action OnDialogueEnded;

        private DialogueEvent currentEvent;
        private int currentLineIndex;
        private CharacterPanel currentPanel;

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

            if (dialogueUI.IsTyping)
            {
                dialogueUI.CompleteTyping();
                return;
            }

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

            CharacterPanel nextPanel = CharacterPanel.Get(line.speakerName);

            if (nextPanel != currentPanel)
            {
                if (currentPanel != null)
                    currentPanel.Hide();
                currentPanel = nextPanel;
            }

            if (currentPanel != null)
                currentPanel.Show(line.emotion);
        }

        private void EndDialogue()
        {
            IsActive = false;
            currentEvent = null;
            currentLineIndex = 0;

            dialogueUI.Hide();

            if (currentPanel != null)
            {
                currentPanel.Hide();
                currentPanel = null;
            }

            OnDialogueEnded?.Invoke();
        }
    }
}
