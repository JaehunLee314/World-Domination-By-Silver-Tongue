using UnityEngine;

namespace InfoGatherPhase
{
    public class ClickableCharacter : ClickableObject
    {
        [SerializeField] private DialogueEvent dialogue;

        public override void OnClicked(DialogueManager dialogueManager)
        {
            if (dialogue == null) return;
            dialogueManager.StartDialogue(dialogue);
        }
    }
}
