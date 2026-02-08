using UnityEngine;

namespace InfoGatherPhase
{
    public class ClickableCharacter : ClickableObject
    {
        [SerializeField] private DialogueEvent dialogue;
        [SerializeField] private ItemData rewardItem;

        private DialogueManager cachedDM;

        public override void OnClicked(DialogueManager dialogueManager)
        {
            if (dialogue == null) return;

            if (rewardItem != null && !GameManager.Instance.HasItem(rewardItem.itemName))
            {
                cachedDM = dialogueManager;
                dialogueManager.OnDialogueEnded += CollectReward;
            }

            dialogueManager.StartDialogue(dialogue);
        }

        private void CollectReward()
        {
            if (cachedDM != null)
                cachedDM.OnDialogueEnded -= CollectReward;

            if (rewardItem != null)
                GameManager.Instance.CollectItem(rewardItem);
        }
    }
}
