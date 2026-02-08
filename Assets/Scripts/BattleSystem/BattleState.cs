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

        // Condition tracking for both sides
        private ConditionStatus[] _playerConditions;
        private ConditionStatus[] _opponentConditions;

        public int CurrentTurn => _currentTurn;
        public int MaxTurns => _maxTurns;
        public int OpponentMaxSanity => _opponentMaxSanity;
        public int OpponentCurrentSanity => _opponentCurrentSanity;
        public IReadOnlyList<ConversationEntry> ConversationHistory => _conversationHistory;
        public StrategyEntry Strategy => _strategy;
        public JudgeResult LastJudgeResult => _lastJudgeResult;
        public ConditionStatus[] PlayerConditions => _playerConditions;
        public ConditionStatus[] OpponentConditions => _opponentConditions;

        public void Reset(int maxTurns, int maxSanity, string[] playerLoseConditions, string[] opponentLoseConditions)
        {
            _currentTurn = 1;
            _maxTurns = maxTurns;
            _opponentMaxSanity = maxSanity;
            _opponentCurrentSanity = maxSanity;
            _conversationHistory.Clear();
            _strategy = default;
            _lastJudgeResult = default;
            _playerConditions = InitConditions(playerLoseConditions);
            _opponentConditions = InitConditions(opponentLoseConditions);
        }

        private static ConditionStatus[] InitConditions(string[] conditions)
        {
            if (conditions == null) return System.Array.Empty<ConditionStatus>();
            var result = new ConditionStatus[conditions.Length];
            for (int i = 0; i < conditions.Length; i++)
            {
                result[i] = new ConditionStatus
                {
                    Condition = conditions[i],
                    IsMet = false,
                    Reasoning = "",
                    MetOnTurn = 0
                };
            }
            return result;
        }

        public int ApplyDamage(int damage)
        {
            _opponentCurrentSanity = Mathf.Clamp(_opponentCurrentSanity - damage, 0, _opponentMaxSanity);
            return _opponentCurrentSanity;
        }

        public void UpdateConditions(JudgeEvaluation playerEval, JudgeEvaluation opponentEval)
        {
            MergeConditions(_playerConditions, playerEval.conditions);
            MergeConditions(_opponentConditions, opponentEval.conditions);
        }

        private void MergeConditions(ConditionStatus[] existing, ConditionStatus[] evaluated)
        {
            if (evaluated == null || existing == null) return;
            for (int i = 0; i < existing.Length && i < evaluated.Length; i++)
            {
                if (!existing[i].IsMet && evaluated[i].IsMet)
                {
                    existing[i].IsMet = true;
                    existing[i].Reasoning = evaluated[i].Reasoning;
                    existing[i].MetOnTurn = _currentTurn;
                }
            }
        }

        public bool AllConditionsMet(bool isPlayer)
        {
            var conditions = isPlayer ? _playerConditions : _opponentConditions;
            if (conditions == null || conditions.Length == 0) return false;
            foreach (var c in conditions)
                if (!c.IsMet) return false;
            return true;
        }

        public ConditionStatus[] GetUnmetConditions(bool isPlayer)
        {
            var conditions = isPlayer ? _playerConditions : _opponentConditions;
            if (conditions == null) return System.Array.Empty<ConditionStatus>();
            var unmet = new List<ConditionStatus>();
            foreach (var c in conditions)
                if (!c.IsMet) unmet.Add(c);
            return unmet.ToArray();
        }

        public int MetConditionCount(bool isPlayer)
        {
            var conditions = isPlayer ? _playerConditions : _opponentConditions;
            if (conditions == null) return 0;
            int count = 0;
            foreach (var c in conditions)
                if (c.IsMet) count++;
            return count;
        }

        public int TotalConditionCount(bool isPlayer)
        {
            var conditions = isPlayer ? _playerConditions : _opponentConditions;
            return conditions?.Length ?? 0;
        }

        public void AdvanceTurn() => _currentTurn++;
        public void SetStrategy(StrategyEntry strategy) => _strategy = strategy;
        public void SetLastJudgeResult(JudgeResult result) => _lastJudgeResult = result;
        public void AddConversationEntry(ConversationEntry entry) => _conversationHistory.Add(entry);
    }
}
