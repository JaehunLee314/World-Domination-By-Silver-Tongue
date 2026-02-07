using UnityEngine;

namespace InfoGatherPhase
{
    public class ClickableItem : ClickableObject
    {
        [SerializeField] private ItemData itemData;

        private DialogueManager cachedDM;

        public ItemData ItemData => itemData;

        public override void OnClicked(DialogueManager dialogueManager)
        {
            if (itemData == null) return;

            if (GameManager.Instance.HasItem(itemData.itemName))
            {
                Debug.Log($"[ClickableItem] Already collected: {itemData.itemName}");
                return;
            }

            GameManager.Instance.CollectItem(itemData);

            if (itemData.pickupDialogue != null)
            {
                cachedDM = dialogueManager;
                dialogueManager.OnDialogueEnded += DestroyAfterDialogue;
                dialogueManager.StartDialogue(itemData.pickupDialogue);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void DestroyAfterDialogue()
        {
            if (cachedDM != null)
                cachedDM.OnDialogueEnded -= DestroyAfterDialogue;

            Destroy(gameObject);
        }
    }
}
