using UnityEngine;

namespace InfoGatherPhase
{
    public class ClickableItem : ClickableObject
    {
        [SerializeField] private ItemData itemData;
        [SerializeField] private bool destroyAfterPickup = true;

        private DialogueManager cachedDM;
        private bool collected;

        public ItemData ItemData => itemData;

        public override void OnClicked(DialogueManager dialogueManager)
        {
            if (itemData == null) return;

            if (collected || GameManager.Instance.HasItem(itemData.itemName))
            {
                if (!destroyAfterPickup && itemData.pickupDialogue != null)
                {
                    dialogueManager.StartDialogue(itemData.pickupDialogue);
                }
                return;
            }

            if (itemData.pickupDialogue != null)
            {
                cachedDM = dialogueManager;
                dialogueManager.OnDialogueEnded += OnDialogueFinished;
                dialogueManager.StartDialogue(itemData.pickupDialogue);
            }
            else
            {
                GameManager.Instance.CollectItem(itemData);
                collected = true;
                if (destroyAfterPickup)
                    Destroy(gameObject);
            }
        }

        private void OnDialogueFinished()
        {
            if (cachedDM != null)
                cachedDM.OnDialogueEnded -= OnDialogueFinished;

            GameManager.Instance.CollectItem(itemData);
            collected = true;
            if (destroyAfterPickup)
                Destroy(gameObject);
        }
    }
}
