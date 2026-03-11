using UnityEngine;

public class PlacedObjectDisplay : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;

    private ItemData currentItem;
    private int currentRotationStep = 0;

    public void SetItem(ItemData item)
    {
        currentItem = item;
        currentRotationStep = 0;
        RefreshVisual();
    }

    public void ClearItem()
    {
        currentItem = null;
        currentRotationStep = 0;

        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = null;
            spriteRenderer.enabled = false;
        }

        transform.rotation = Quaternion.identity;
    }

    public bool HasItem()
    {
        return currentItem != null;
    }

    public ItemData GetItem()
    {
        return currentItem;
    }

    public int GetRotationStep()
    {
        return currentRotationStep;
    }

    public float GetRotationAngle()
    {
        return currentRotationStep * 45f;
    }

    public void Rotate45Degrees()
    {
        if (currentItem == null) return;

        currentRotationStep = (currentRotationStep + 1) % 8;
        RefreshRotation();
    }

    private void RefreshVisual()
    {
        if (spriteRenderer != null)
        {
            if (currentItem != null && currentItem.icon != null)
            {
                spriteRenderer.enabled = true;
                spriteRenderer.sprite = currentItem.icon;
            }
            else
            {
                spriteRenderer.enabled = false;
                spriteRenderer.sprite = null;
            }
        }

        RefreshRotation();
    }

    private void RefreshRotation()
    {
        float angle = currentRotationStep * 45f;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }
}