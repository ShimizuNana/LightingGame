using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventorySlotUI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image icon;

    private int slotIndex;
    private InventoryUI inventoryUI;

    public void Setup(int index, InventoryUI ui)
    {
        slotIndex = index;
        inventoryUI = ui;
    }

    public void Refresh(ItemData item)
    {
        if (item == null)
        {
            icon.enabled = false;
            icon.sprite = null;
        }
        else
        {
            icon.enabled = true;
            icon.sprite = item.icon;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.clickCount >= 2)
        {
            inventoryUI.TryTakeItemToHand(slotIndex);
        }
    }
}
