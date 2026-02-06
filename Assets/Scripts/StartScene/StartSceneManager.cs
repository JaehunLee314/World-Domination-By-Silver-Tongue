using UnityEngine;

namespace SilverTongue.StartScene
{
    public class StartSceneManager : MonoBehaviour
    {
        [SerializeField] private StartView startView;

        private void Start()
        {
            if (GameManager.Instance == null)
                Debug.LogError("[StartSceneManager] GameManager not found in scene!");

            startView.Initialize(this);
        }

        public void OnStartClicked()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("DummyInfoGatheringScene");
        }
    }
}
