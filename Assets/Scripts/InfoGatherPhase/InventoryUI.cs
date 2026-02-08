using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace InfoGatherPhase
{
    public class InventoryUI : MonoBehaviour
    {
        [Header("Inventory Button")]
        [SerializeField] private Button inventoryButton;

        [Header("Inventory Panel")]
        [SerializeField] private GameObject inventoryPanel;
        [SerializeField] private Button inventoryCloseButton;
        [SerializeField] private Transform gridContainer;
        [SerializeField] private InventorySlot slotPrefab;

        [Header("Detail Popup")]
        [SerializeField] private GameObject detailPopup;
        [SerializeField] private Button detailCloseButton;
        [SerializeField] private Image detailIcon;
        [SerializeField] private TextMeshProUGUI detailName;
        [SerializeField] private TextMeshProUGUI detailType;
        [SerializeField] private TextMeshProUGUI detailDescription;

        public bool IsOpen => inventoryPanel.activeSelf;

        public event Action OnInventoryOpened;
        public event Action OnInventoryClosed;

        private List<InventorySlot> activeSlots = new List<InventorySlot>();
        private DialogueManager dialogueManager;

        public void Init(DialogueManager dm)
        {
            dialogueManager = dm;
            dialogueManager.OnDialogueStarted += OnDialogueStarted;
            dialogueManager.OnDialogueEnded += OnDialogueEnded;
        }

        private void OnEnable()
        {
            inventoryButton.onClick.AddListener(OpenInventory);
            inventoryCloseButton.onClick.AddListener(CloseInventory);
            detailCloseButton.onClick.AddListener(CloseDetail);
        }

        private void OnDisable()
        {
            inventoryButton.onClick.RemoveListener(OpenInventory);
            inventoryCloseButton.onClick.RemoveListener(CloseInventory);
            detailCloseButton.onClick.RemoveListener(CloseDetail);

            if (dialogueManager != null)
            {
                dialogueManager.OnDialogueStarted -= OnDialogueStarted;
                dialogueManager.OnDialogueEnded -= OnDialogueEnded;
            }
        }

        private void Start()
        {
            inventoryPanel.SetActive(false);
            detailPopup.SetActive(false);

            if (GameManager.Instance != null)
                GameManager.Instance.OnItemCollected += OnItemCollected;
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnItemCollected -= OnItemCollected;
        }

        private void OpenInventory()
        {
            RefreshGrid();
            inventoryPanel.SetActive(true);
            OnInventoryOpened?.Invoke();
        }

        public void CloseInventory()
        {
            detailPopup.SetActive(false);
            inventoryPanel.SetActive(false);
            OnInventoryClosed?.Invoke();
        }

        private void RefreshGrid()
        {
            foreach (var slot in activeSlots)
                Destroy(slot.gameObject);
            activeSlots.Clear();

            var items = GameManager.Instance.CollectedItems;
            for (int i = 0; i < items.Count; i++)
            {
                InventorySlot slot = Instantiate(slotPrefab, gridContainer);
                slot.gameObject.SetActive(true);
                slot.Setup(items[i], ShowDetail);
                activeSlots.Add(slot);
            }
        }

        private void ShowDetail(ItemData item)
        {
            detailIcon.sprite = TextureToSprite(item.icon);
            detailIcon.enabled = item.icon != null;
            detailName.text = item.itemName;
            detailType.text = item.itemType.ToString();
            detailDescription.text = item.description;
            detailPopup.SetActive(true);
        }

        private void CloseDetail()
        {
            detailPopup.SetActive(false);
        }

        private void OnItemCollected(ItemData item)
        {
            if (inventoryPanel.activeSelf)
                RefreshGrid();
        }

        private void OnDialogueStarted()
        {
            inventoryButton.interactable = false;
        }

        private void OnDialogueEnded()
        {
            inventoryButton.interactable = true;
        }

        private static Sprite TextureToSprite(Texture2D tex)
        {
            if (tex == null) return null;
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        }
    }
}
