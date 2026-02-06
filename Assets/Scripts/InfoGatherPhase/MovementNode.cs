using System;
using UnityEngine;

namespace InfoGatherPhase
{
    [Serializable]
    public struct CameraSettings
    {
        public Vector3 positionOffset;
        public Vector3 rotation;
        public float fov;

        public static CameraSettings Default => new CameraSettings
        {
            positionOffset = Vector3.zero,
            rotation = Vector3.zero,
            fov = 60f
        };
    }

    public class MovementNode : MonoBehaviour
    {
        [Header("Neighbors")]
        [SerializeField] private MovementNode north; // W / +Z
        [SerializeField] private MovementNode south; // S / -Z
        [SerializeField] private MovementNode west;  // A / -X
        [SerializeField] private MovementNode east;  // D / +X

        [Header("Camera")]
        [SerializeField] private CameraSettings cameraSettings = CameraSettings.Default;

        [Header("Arrival")]
        [SerializeField] private DialogueEvent arrivalDialogue;

        public CameraSettings CamSettings => cameraSettings;
        public DialogueEvent ArrivalDialogue => arrivalDialogue;

        public MovementNode GetNeighbor(Vector2 direction)
        {
            if (direction.y > 0.5f) return north;
            if (direction.y < -0.5f) return south;
            if (direction.x < -0.5f) return west;
            if (direction.x > 0.5f) return east;
            return null;
        }
    }
}
