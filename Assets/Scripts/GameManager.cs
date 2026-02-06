using System.Collections.Generic;
using UnityEngine;
using SilverTongue.Data;

namespace SilverTongue
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Player Data")]
        public CharacterSO[] availableCharacters;
        public List<ItemSO> playerItems = new List<ItemSO>();

        [Header("Battle State")]
        public CharacterSO selectedBattler;
        public CharacterSO currentOpponent;
        public int maxTurns = 7;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public List<ItemSO> GetItemsByType(ItemType type)
        {
            return playerItems.FindAll(item => item.itemType == type);
        }

        public List<ItemSO> GetEvidence()
        {
            return GetItemsByType(ItemType.Evidence);
        }

        public List<ItemSO> GetSkillBooks()
        {
            return GetItemsByType(ItemType.SkillBook);
        }
    }
}
