using System.Collections.Generic;
using SilverTongue.Data;

namespace SilverTongue.BattleSystem
{
    public enum BattlePhase
    {
        BattlerSelecting,
        StrategySelecting,
        Battle,
        BattleResult
    }

    public struct StrategyEntry
    {
        public string StrategyText;
        public List<ItemSO> Items;
    }

    [System.Serializable]
    public struct ConversationEntry
    {
        public string SpeakerName;
        public string SpeechText;
        public string Timestamp;
        public bool IsPlayer;
        public string EvidenceUsed;

        // Judge evaluation data (only set on JUDGE entries)
        public string DamageType;    // "Ineffective", "Normal Hit", "Critical Hit", "Trap Trigger"
        public int DamageDealt;
    }

    [System.Serializable]
    public struct JudgeResult
    {
        public string reasoning;
        public string damage_type;    // "Ineffective", "Normal Hit", "Critical Hit", "Trap Trigger"
        public int damage_dealt;
        public int prophet_current_sanity;
        public string status;         // "ONGOING", "KENTA_WINS", "PROPHET_WINS"
    }
}
