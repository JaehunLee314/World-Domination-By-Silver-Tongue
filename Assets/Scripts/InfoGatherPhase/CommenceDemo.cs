using UnityEngine;

namespace InfoGatherPhase
{
    public class CommenceDemo : MonoBehaviour
    {
        [SerializeField] private DialogueManager dialogueManager;
        [SerializeField] private DialogueEvent demoDialogue;

        private void Start()
        {
            if (demoDialogue != null && dialogueManager != null)
            {
                dialogueManager.StartDialogue(demoDialogue);
            }
        }
    }
}
