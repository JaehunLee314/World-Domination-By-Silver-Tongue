using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SilverTongue.BattleScene
{
    public class BattleResultView : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI resultText;
        [SerializeField] private TextMeshProUGUI resultDescriptionText;
        [SerializeField] private Button closeButton;

        private BattleSceneManager _manager;

        public void Initialize(BattleSceneManager manager, bool playerWon)
        {
            _manager = manager;

            resultText.text = playerWon ? "WIN" : "LOSE";
            resultText.color = playerWon ? Color.yellow : Color.red;

            resultDescriptionText.text = playerWon
                ? "Your words reached their heart! A new nakama has been gained!"
                : "Your persuasion was not enough this time. Better luck next round.";

            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(OnCloseClicked);
        }

        private void OnCloseClicked()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("DummyInfoGatheringScene");
        }
    }
}
