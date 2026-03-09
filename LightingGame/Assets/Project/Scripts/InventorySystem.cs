using System;
using System.Collections.Generic;
using UnityEngine;

public class InventorySystem : MonoBehaviour
{
    public static InventorySystem Instance;

    [SerializeField] private int slotCount = 10;
    public List<InventorySlot> slots = new List<InventorySlot>();

    public Action OnInventoryChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        for (int i = 0; i < slotCount; i++)
        {
            slots.Add(new InventorySlot());
        }
    }

    public bool AddItem(ItemData item)
    {
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].IsEmpty())
            {
                slots[i].item = item;
                OnInventoryChanged?.Invoke();
                return true;
            }
        }

        Debug.Log("Backpack Full");
        return false;
    }

    public bool RemoveItemAt(int index)
    {
        if (index < 0 || index >= slots.Count) return false;
        if (slots[index].IsEmpty()) return false;

        slots[index].Clear();
        OnInventoryChanged?.Invoke();
        return true;
    }

    public ItemData GetItemAt(int index)
    {
        if (index < 0 || index >= slots.Count) return null;
        return slots[index].item;
    }
}
