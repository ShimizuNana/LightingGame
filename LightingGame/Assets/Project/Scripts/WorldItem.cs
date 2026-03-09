using UnityEngine;

public class WorldItem : MonoBehaviour
{
    public ItemData itemData;

    public void Pickup()
    {
        bool success = InventorySystem.Instance.AddItem(itemData);

        if (success)
        {
            Destroy(gameObject);
        }
    }
}
