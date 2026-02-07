using System.Collections.Generic;
using UnityEngine;

namespace SilverTongue.BattleSystem
{
    public class BattleState
    {
        private int _currentTurn;
        private int _maxTurns;
        private int _opponentMaxSanity;
        private int _opponentCurrentSanity;
        private readonly List<ConversationEntry> _conversationHistory = new List<ConversationEntry>();
        private StrategyEntry _strategy;
        private JudgeResult _lastJudgeResult;

        public int CurrentTurn => _currentTurn;
        public int MaxTurns => _maxTurns;
        public int OpponentMaxSanity => _opponentMaxSanity;
        public int OpponentCurrentSanity => _opponentCurrentSanity;
        public IReadOnlyList<ConversationEntry> ConversationHistory => _conversationHistory;
        public StrategyEntry Strategy => _strategy;
        public JudgeResult LastJudgeResult => _lastJudgeResult;

        public void Reset(int maxTurns, int maxSanity)
        {
            _currentTurn = 1;
            _maxTurns = maxTurns;
            _opponentMaxSanity = maxSanity;
            _opponentCurrentSanity = maxSanity;
            _conversationHistory.Clear();
            _strategy = default;
            _lastJudgeResult = default;
        }

        public int ApplyDamage(int damage)
        {
            _opponentCurrentSanity = Mathf.Clamp(_opponentCurrentSanity - damage, 0, _opponentMaxSanity);
            return _opponentCurrentSanity;
        }

        public void AdvanceTurn()
        {
            _currentTurn++;
        }

        public void SetStrategy(StrategyEntry strategy)
        {
            _strategy = strategy;
        }

        public void SetLastJudgeResult(JudgeResult result)
        {
            _lastJudgeResult = result;
        }

        public void AddConversationEntry(ConversationEntry entry)
        {
            _conversationHistory.Add(entry);
        }
    }
}
