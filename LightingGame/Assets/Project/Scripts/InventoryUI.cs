using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private InventorySlotUI[] slotUIs;
    [SerializeField] private PlayerHand playerHand;

    private bool isOpen = false;

    private void Start()
    {
        if (InventorySystem.Instance != null)
        {
            InventorySystem.Instance.OnInventoryChanged += RefreshAll;
        }

        InitSlots();
        RefreshAll();
        Close();
    }

    private void OnDestroy()
    {
        if (InventorySystem.Instance != null)
        {
            InventorySystem.Instance.OnInventoryChanged -= RefreshAll;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (isOpen) Close();
            else Open();
        }
    }

    private void InitSlots()
    {
        for (int i = 0; i < slotUIs.Length; i++)
        {
            slotUIs[i].Setup(i, this);
        }
    }

    public void RefreshAll()
    {
        for (int i = 0; i < slotUIs.Length; i++)
        {
            ItemData item = InventorySystem.Instance.GetItemAt(i);
            slotUIs[i].Refresh(item);
        }
    }

    public void Open()
    {
        isOpen = true;
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    public void Close()
    {
        isOpen = false;
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    public void TryTakeItemToHand(int slotIndex)
    {
        if (playerHand.HasItem()) return;

        ItemData item = InventorySystem.Instance.GetItemAt(slotIndex);
        if (item == null) return;

        playerHand.SetHandItem(item);
        InventorySystem.Instance.RemoveItemAt(slotIndex);
    }
}