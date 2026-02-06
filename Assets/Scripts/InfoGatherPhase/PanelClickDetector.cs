using UnityEngine;
using UnityEngine.InputSystem;

namespace InfoGatherPhase
{
    public class PanelClickDetector : MonoBehaviour
    {
        [SerializeField] private Camera cam;
        [SerializeField] private LayerMask clickableLayer;
        [SerializeField] private float maxRayDistance = 100f;

        private DialogueManager dialogueManager;

        public void Init(DialogueManager dm)
        {
            dialogueManager = dm;
        }

        public void HandleClick()
        {
            if (dialogueManager != null && dialogueManager.IsActive) return;

            if (Mouse.current == null)
            {
                Debug.LogWarning("[ClickDetector] Mouse.current is null");
                return;
            }

            Vector2 mousePos = Mouse.current.position.ReadValue();
            Ray ray = cam.ScreenPointToRay(mousePos);

            if (Physics.Raycast(ray, out RaycastHit hit, maxRayDistance, clickableLayer))
            {
                Debug.Log($"[ClickDetector] Hit: {hit.collider.name} (layer {hit.collider.gameObject.layer}) at {hit.point}");
                ClickableItem item = hit.collider.GetComponent<ClickableItem>();
                if (item != null)
                {
                    item.OnClicked(dialogueManager);
                }
                else
                {
                    Debug.Log($"[ClickDetector] Hit object has no ClickableItem component");
                }
            }
            else
            {
                // Debug raycast without layer filter to see if we're hitting anything at all
                if (Physics.Raycast(ray, out RaycastHit anyHit, maxRayDistance))
                {
                    Debug.Log($"[ClickDetector] Missed layer {clickableLayer.value}, but ray hit: {anyHit.collider.name} (layer {anyHit.collider.gameObject.layer}) at {anyHit.point}");
                }
                else
                {
                    Debug.Log($"[ClickDetector] Ray from mousePos={mousePos} hit nothing (origin={ray.origin}, dir={ray.direction})");
                }
            }
        }
    }
}
