using UnityEngine;

public class WorldItem : MonoBehaviour
{
    public ItemData itemData;

    public void Pickup()
    {
        bool success = InventorySystem.Instance.AddItem(itemData);

        if (success)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(AudioManager.Instance.playerPickupClip);
            }

            Destroy(gameObject);
        }
    }
}
