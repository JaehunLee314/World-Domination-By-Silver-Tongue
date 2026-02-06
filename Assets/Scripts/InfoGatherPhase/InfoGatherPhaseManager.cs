using UnityEngine;

namespace InfoGatherPhase
{
    public class InfoGatherPhaseManager : MonoBehaviour
    {
        [Header("Subsystems")]
        [SerializeField] private InfoGatherInputHandler inputHandler;
        [SerializeField] private NodeMovementController movementController;
        [SerializeField] private PanelClickDetector clickDetector;
        [SerializeField] private DialogueManager dialogueManager;
        [SerializeField] private InventoryUI inventoryUI;

        private void OnEnable()
        {
            clickDetector.Init(dialogueManager);

            inputHandler.OnMoveInput += movementController.HandleMoveInput;
            inputHandler.OnClickInput += HandleClick;

            movementController.OnArrivedAtNode += HandleArrival;

            dialogueManager.OnDialogueStarted += OnDialogueStarted;
            dialogueManager.OnDialogueEnded += OnDialogueEnded;

            if (inventoryUI != null)
            {
                inventoryUI.Init(dialogueManager);
                inventoryUI.OnInventoryOpened += OnInventoryOpened;
                inventoryUI.OnInventoryClosed += OnInventoryClosed;
            }
        }

        private void OnDisable()
        {
            inputHandler.OnMoveInput -= movementController.HandleMoveInput;
            inputHandler.OnClickInput -= HandleClick;

            movementController.OnArrivedAtNode -= HandleArrival;

            dialogueManager.OnDialogueStarted -= OnDialogueStarted;
            dialogueManager.OnDialogueEnded -= OnDialogueEnded;

            if (inventoryUI != null)
            {
                inventoryUI.OnInventoryOpened -= OnInventoryOpened;
                inventoryUI.OnInventoryClosed -= OnInventoryClosed;
            }
        }

        private void HandleClick()
        {
            if (inventoryUI != null && inventoryUI.IsOpen) return;

            if (dialogueManager.IsActive)
            {
                Debug.Log("[PhaseManager] Click -> advancing dialogue");
                dialogueManager.AdvanceLine();
            }
            else
            {
                Debug.Log("[PhaseManager] Click -> raycasting for clickable");
                clickDetector.HandleClick();
            }
        }

        private void HandleArrival(MovementNode node)
        {
            Debug.Log($"[PhaseManager] Arrived at node: {node.name}");
            if (node.ArrivalDialogue != null)
            {
                dialogueManager.StartDialogue(node.ArrivalDialogue);
            }
        }

        private void OnDialogueStarted()
        {
            movementController.IsLocked = true;
        }

        private void OnDialogueEnded()
        {
            movementController.IsLocked = false;
        }

        private void OnInventoryOpened()
        {
            movementController.IsLocked = true;
        }

        private void OnInventoryClosed()
        {
            movementController.IsLocked = false;
        }
    }
}
