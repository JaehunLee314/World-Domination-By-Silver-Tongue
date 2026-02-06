using System;
using UnityEngine;

namespace InfoGatherPhase
{
    [Serializable]
    public struct DialogueLine
    {
        public string speakerName;
        [TextArea(2, 5)]
        public string text;
        public Texture2D speakerPortrait;
    }
}
