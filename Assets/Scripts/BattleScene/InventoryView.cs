using System.Collections.Generic;
using UnityEngine;
using SilverTongue.Data;

namespace SilverTongue.BattleScene
{
    public class InventoryView : MonoBehaviour
    {
        [SerializeField] private Transform inventoryGrid;
        [SerializeField] private GameObject inventoryItemPrefab;

        public void Populate(IReadOnlyList<ItemSO> items)
        {
            foreach (Transform child in inventoryGrid)
                Destroy(child.gameObject);

            if (items == null) return;

            foreach (var item in items)
            {
                var itemObj = Instantiate(inventoryItemPrefab, inventoryGrid);
                var itemUI = itemObj.GetComponent<InventoryItemUI>();
                if (itemUI != null)
                    itemUI.Setup(item);
            }
        }

        public void PopulateFromGameManager()
        {
            if (GameManager.Instance == null) return;
            Populate(GameManager.Instance.playerItems);
        }
    }
}
