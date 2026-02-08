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
        [SerializeField] private BattleDecisionUI battleDecisionUI;

        private void OnEnable()
        {
            clickDetector.Init(dialogueManager);

            inputHandler.OnMoveInput += movementController.HandleMoveInput;
            inputHandler.OnClickInput += HandleClick;

            movementController.OnArrivedAtNode += HandleArrival;
            clickDetector.OnClickableHit += HandleClickableHit;

            dialogueManager.OnDialogueStarted += OnDialogueStarted;
            dialogueManager.OnDialogueEnded += OnDialogueEnded;

            if (inventoryUI != null)
            {
                inventoryUI.Init(dialogueManager);
                inventoryUI.OnInventoryOpened += OnInventoryOpened;
                inventoryUI.OnInventoryClosed += OnInventoryClosed;
            }

            if (battleDecisionUI != null)
            {
                battleDecisionUI.OnConfront += HandleConfront;
                battleDecisionUI.OnDecline += HandleDecline;
            }
        }

        private void OnDisable()
        {
            inputHandler.OnMoveInput -= movementController.HandleMoveInput;
            inputHandler.OnClickInput -= HandleClick;

            movementController.OnArrivedAtNode -= HandleArrival;
            clickDetector.OnClickableHit -= HandleClickableHit;

            dialogueManager.OnDialogueStarted -= OnDialogueStarted;
            dialogueManager.OnDialogueEnded -= OnDialogueEnded;

            if (inventoryUI != null)
            {
                inventoryUI.OnInventoryOpened -= OnInventoryOpened;
                inventoryUI.OnInventoryClosed -= OnInventoryClosed;
            }

            if (battleDecisionUI != null)
            {
                battleDecisionUI.OnConfront -= HandleConfront;
                battleDecisionUI.OnDecline -= HandleDecline;
            }
        }

        private void HandleClick()
        {
            if (battleDecisionUI != null && battleDecisionUI.IsOpen) return;
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
            if (node.ArrivalDialogue != null && !node.HasPlayedArrival)
            {
                node.MarkArrivalPlayed();
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

        private ClickableVillain activeVillain;

        private void HandleClickableHit(ClickableObject clickable)
        {
            if (clickable is ClickableVillain villain)
            {
                activeVillain = villain;
                villain.OnDialogueFinished += ShowBattleDecision;
            }
        }

        private void ShowBattleDecision()
        {
            if (activeVillain != null)
            {
                activeVillain.OnDialogueFinished -= ShowBattleDecision;
                activeVillain = null;
            }

            if (battleDecisionUI == null) return;
            movementController.IsLocked = true;
            battleDecisionUI.Show();
        }

        private void HandleConfront()
        {
            Debug.Log("[PhaseManager] Player chose to confront villain - loading battle scene");
            // TODO: Load battle scene
        }

        private void HandleDecline()
        {
            Debug.Log("[PhaseManager] Player chose 'not yet'");
            movementController.IsLocked = false;
        }
    }
}
