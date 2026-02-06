using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SilverTongue.StartScene
{
    public class StartView : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private Button startButton;

        private StartSceneManager _manager;

        public void Initialize(StartSceneManager manager)
        {
            _manager = manager;
            startButton.onClick.RemoveAllListeners();
            startButton.onClick.AddListener(OnStartButtonClicked);
        }

        private void OnStartButtonClicked()
        {
            _manager.OnStartClicked();
        }
    }
}
