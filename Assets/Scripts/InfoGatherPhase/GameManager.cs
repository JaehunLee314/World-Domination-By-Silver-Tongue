using System;
using System.Collections.Generic;
using UnityEngine;

namespace InfoGatherPhase
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        private List<ItemData> collectedItems = new List<ItemData>();
        private HashSet<string> collectedItemNames = new HashSet<string>();

        public event Action<ItemData> OnItemCollected;
        public IReadOnlyList<ItemData> CollectedItems => collectedItems;

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

        public void CollectItem(ItemData item)
        {
            if (item == null || collectedItemNames.Contains(item.itemName)) return;

            collectedItems.Add(item);
            collectedItemNames.Add(item.itemName);
            Debug.Log($"[GameManager] Collected: {item.itemName}");
            OnItemCollected?.Invoke(item);
        }

        public bool HasItem(string itemName)
        {
            return collectedItemNames.Contains(itemName);
        }

        public void ResetInventory()
        {
            collectedItems.Clear();
            collectedItemNames.Clear();
        }
    }
}
