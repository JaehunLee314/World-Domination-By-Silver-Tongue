using System;
using UnityEngine;

namespace InfoGatherPhase
{
    public class ClickableVillain : ClickableObject
    {
        [SerializeField] private DialogueEvent dialogue;

        public event Action OnDialogueFinished;

        private DialogueManager cachedDM;

        public override void OnClicked(DialogueManager dialogueManager)
        {
            if (dialogue == null) return;

            cachedDM = dialogueManager;
            dialogueManager.OnDialogueEnded += HandleDialogueEnded;
            dialogueManager.StartDialogue(dialogue);
        }

        private void HandleDialogueEnded()
        {
            if (cachedDM != null)
                cachedDM.OnDialogueEnded -= HandleDialogueEnded;

            OnDialogueFinished?.Invoke();
        }
    }
}
