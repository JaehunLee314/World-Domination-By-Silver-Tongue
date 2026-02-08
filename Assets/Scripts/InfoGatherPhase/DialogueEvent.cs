using UnityEngine;

namespace InfoGatherPhase
{
    [CreateAssetMenu(fileName = "NewDialogueEvent", menuName = "InfoGather/Dialogue Event")]
    public class DialogueEvent : ScriptableObject
    {
        public DialogueLine[] lines;
    }
}
