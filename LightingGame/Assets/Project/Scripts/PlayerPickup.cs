using UnityEngine;

public class PlayerPickup : MonoBehaviour
{
    private WorldItem currentItem;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out WorldItem item))
        {
            currentItem = item;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent(out WorldItem item) && item == currentItem)
        {
            currentItem = null;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (currentItem != null)
            {
                currentItem.Pickup();
            }
        }
    }
}