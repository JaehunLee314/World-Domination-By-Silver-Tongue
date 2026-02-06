using UnityEngine;
using SilverTongue.Data;

namespace SilverTongue.DummyInfoGatheringScene
{
    public class DummyInfoGatheringSceneManager : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private ItemSO[] itemsToProvide;
        [SerializeField] private CharacterSO opponent;

        [Header("View")]
        [SerializeField] private DummyInfoGatheringView view;

        private void Start()
        {
            ProvideItemsToPlayer();
            SetOpponent();
            view.Initialize(this, itemsToProvide);
        }

        private void ProvideItemsToPlayer()
        {
            if (GameManager.Instance == null) return;
            GameManager.Instance.playerItems.Clear();
            foreach (var item in itemsToProvide)
            {
                if (item != null)
                    GameManager.Instance.playerItems.Add(item);
            }
        }

        private void SetOpponent()
        {
            if (GameManager.Instance == null) return;
            GameManager.Instance.currentOpponent = opponent;
        }

        public void OnProceedToBattle()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("BattleScene");
        }
    }
}
