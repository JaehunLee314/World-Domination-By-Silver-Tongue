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
        public string SpeechText;      // Clean display text (tags stripped)
        public string RawText;         // Original text with tags (for judge context)
        public string Timestamp;
        public bool IsPlayer;
        public string EvidenceUsed;
        public Emotion Emotion;
    }

    public struct ParsedTags
    {
        public bool AcceptDefeat;
        public string EvidenceUsed;
        public string InnerThought;
        public Emotion Emotion;
    }

    [System.Serializable]
    public struct ConditionStatus
    {
        public string Condition;    // Text from CharacterSO.loseConditions
        public bool IsMet;
        public string Reasoning;    // Judge's explanation for why it is met
        public int MetOnTurn;       // Turn number when condition was met (0 = not met)
    }

    [System.Serializable]
    public struct JudgeEvaluation
    {
        public ConditionStatus[] conditions;
    }

    [System.Serializable]
    public struct JudgeResult
    {
        public JudgeEvaluation playerEval;
        public JudgeEvaluation opponentEval;
        public int damage;          // Damage to opponent sanity
        public string reasoning;    // Overall reasoning
    }

    public static class BattleStatus
    {
        public const string Ongoing      = "ONGOING";
        public const string PlayerWins   = "PLAYER_WINS";
        public const string OpponentWins = "OPPONENT_WINS";
    }
}
