using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InventorySlot
{
    public ItemData item;

    public bool IsEmpty()
    {
        return item == null;
    }

    public void Clear()
    {
        item = null;
    }
}