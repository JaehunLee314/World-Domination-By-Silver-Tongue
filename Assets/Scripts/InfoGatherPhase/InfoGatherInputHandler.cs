using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace InfoGatherPhase
{
    public class InfoGatherInputHandler : MonoBehaviour
    {
        [SerializeField] private InputActionReference moveAction;
        [SerializeField] private InputActionReference clickAction;

        public event Action<Vector2> OnMoveInput;
        public event Action OnClickInput;

        private bool moveConsumed = true;

        private void OnEnable()
        {
            moveAction.action.Enable();
            clickAction.action.Enable();

            moveAction.action.performed += HandleMove;
            moveAction.action.canceled += HandleMoveCanceled;
            clickAction.action.performed += HandleClick;
        }

        private void OnDisable()
        {
            moveAction.action.performed -= HandleMove;
            moveAction.action.canceled -= HandleMoveCanceled;
            clickAction.action.performed -= HandleClick;

            moveAction.action.Disable();
            clickAction.action.Disable();
        }

        private void HandleMove(InputAction.CallbackContext ctx)
        {
            if (moveConsumed)
            {
                Vector2 raw = ctx.ReadValue<Vector2>();
                Vector2 snapped = SnapToCardinal(raw);
                if (snapped != Vector2.zero)
                {
                    moveConsumed = false;
                    Debug.Log($"[Input] Move: raw={raw}, snapped={snapped}");
                    OnMoveInput?.Invoke(snapped);
                }
            }
        }

        private void HandleMoveCanceled(InputAction.CallbackContext ctx)
        {
            moveConsumed = true;
        }

        private void HandleClick(InputAction.CallbackContext ctx)
        {
            Debug.Log("[Input] Click performed");
            OnClickInput?.Invoke();
        }

        private Vector2 SnapToCardinal(Vector2 input)
        {
            if (input.sqrMagnitude < 0.25f) return Vector2.zero;

            if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
                return new Vector2(Mathf.Sign(input.x), 0);
            else
                return new Vector2(0, Mathf.Sign(input.y));
        }
    }
}
