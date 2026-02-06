namespace SilverTongue.BattleScene
{
    [System.Serializable]
    public struct ConversationEntry
    {
        public string SpeakerName;
        public string SpeechText;
        public string Timestamp;
        public bool IsPlayer;
        public string EvidenceUsed;
        public string SkillUsed;
    }
}
