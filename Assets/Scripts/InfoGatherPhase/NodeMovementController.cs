using System;
using UnityEngine;

namespace InfoGatherPhase
{
    public class NodeMovementController : MonoBehaviour
    {
        [SerializeField] private MovementNode startNode;
        [SerializeField] private CameraRig cameraRig;

        public MovementNode CurrentNode { get; private set; }
        public bool IsLocked { get; set; }

        public event Action<MovementNode> OnArrivedAtNode;

        private void Start()
        {
            if (startNode != null)
            {
                CurrentNode = startNode;
                cameraRig.SnapTo(CurrentNode.transform.position, CurrentNode.CamSettings);
            }
        }

        public void HandleMoveInput(Vector2 direction)
        {
            if (IsLocked)
            {
                Debug.Log("[Movement] Blocked: movement is locked (dialogue active)");
                return;
            }
            if (cameraRig.IsTweening)
            {
                Debug.Log("[Movement] Blocked: camera is tweening");
                return;
            }
            if (CurrentNode == null)
            {
                Debug.Log("[Movement] Blocked: no current node");
                return;
            }

            MovementNode target = CurrentNode.GetNeighbor(direction);
            if (target == null)
            {
                Debug.Log($"[Movement] No neighbor {direction} from {CurrentNode.name}");
                return;
            }

            Debug.Log($"[Movement] Moving from {CurrentNode.name} to {target.name}");
            CurrentNode = target;
            cameraRig.TweenTo(target.transform.position, target.CamSettings);
            cameraRig.OnTweenComplete += HandleArrival;
        }

        private void HandleArrival()
        {
            cameraRig.OnTweenComplete -= HandleArrival;
            OnArrivedAtNode?.Invoke(CurrentNode);
        }
    }
}
