using UnityEngine;
using TMPro;
using SilverTongue.BattleSystem;

namespace SilverTongue.BattleScene
{
    public class ConditionPanelView : MonoBehaviour
    {
        [SerializeField] private ConditionRowUI[] playerConditionRows;
        [SerializeField] private ConditionRowUI[] opponentConditionRows;
        [SerializeField] private TextMeshProUGUI playerHeaderText;
        [SerializeField] private TextMeshProUGUI opponentHeaderText;

        public void Initialize(ConditionStatus[] playerConditions, ConditionStatus[] opponentConditions)
        {
            SetupSide(playerConditionRows, playerConditions);
            SetupSide(opponentConditionRows, opponentConditions);
        }

        public void Refresh(ConditionStatus[] playerConditions, ConditionStatus[] opponentConditions)
        {
            UpdateSide(playerConditionRows, playerConditions);
            UpdateSide(opponentConditionRows, opponentConditions);
        }

        private void SetupSide(ConditionRowUI[] rows, ConditionStatus[] conditions)
        {
            for (int i = 0; i < rows.Length; i++)
            {
                if (i < conditions.Length)
                {
                    rows[i].gameObject.SetActive(true);
                    rows[i].Setup(conditions[i].Condition);
                }
                else
                {
                    rows[i].gameObject.SetActive(false);
                }
            }
        }

        private void UpdateSide(ConditionRowUI[] rows, ConditionStatus[] conditions)
        {
            for (int i = 0; i < rows.Length && i < conditions.Length; i++)
                rows[i].SetMet(conditions[i].IsMet);
        }
    }
}
