using UnityEngine;

namespace InfoGatherPhase
{
    public class ClickableItem : MonoBehaviour
    {
        [SerializeField] private ItemData itemData;

        public ItemData ItemData => itemData;

        public void OnClicked(DialogueManager dialogueManager)
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
                dialogueManager.StartDialogue(itemData.pickupDialogue);
            }
        }
    }
}
