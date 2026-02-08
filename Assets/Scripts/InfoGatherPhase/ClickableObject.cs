using UnityEngine;

namespace InfoGatherPhase
{
    public abstract class ClickableObject : MonoBehaviour
    {
        public abstract void OnClicked(DialogueManager dialogueManager);
    }
}
