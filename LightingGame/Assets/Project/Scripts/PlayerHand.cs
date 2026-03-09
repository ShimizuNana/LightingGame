using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHand : MonoBehaviour
{
    [SerializeField] private Image handIcon;
    [SerializeField] private TMP_Text handNameText;

    private ItemData currentHandItem;

    public bool HasItem()
    {
        return currentHandItem != null;
    }

    public ItemData GetHandItem()
    {
        return currentHandItem;
    }

    public void SetHandItem(ItemData item)
    {
        currentHandItem = item;
        RefreshUI();
    }

    public void ClearHandItem()
    {
        currentHandItem = null;
        RefreshUI();
    }

    private void Start()
    {
        RefreshUI();
    }

    private void RefreshUI()
    {
        if (currentHandItem == null)
        {
            if (handIcon != null)
            {
                handIcon.enabled = false;
                handIcon.sprite = null;
            }

            if (handNameText != null)
            {
                handNameText.text = "手上无物品";
            }
        }
        else
        {
            if (handIcon != null)
            {
                handIcon.enabled = true;
                handIcon.sprite = currentHandItem.icon;
            }

            if (handNameText != null)
            {
                handNameText.text = currentHandItem.itemName;
            }
        }
    }
}