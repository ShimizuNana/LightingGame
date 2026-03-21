using UnityEngine;

public class PlacedObjectDisplay : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Vector3 placedItemScale = new Vector3(1.5f, 1.5f, 1f);

    private ItemData currentItem;
    private int currentRotationStep = 0; // 0~7，每步45度

    public void SetItem(ItemData item)
    {
        currentItem = item;
        currentRotationStep = 0;

        transform.localScale = placedItemScale;
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

        transform.localScale = Vector3.one;
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
        RefreshVisual();
    }

    private void RefreshVisual()
    {
        if (spriteRenderer == null)
            return;

        if (currentItem == null)
        {
            spriteRenderer.enabled = false;
            spriteRenderer.sprite = null;
            return;
        }

        spriteRenderer.enabled = true;

        // 如果有角度素材，就优先用角度素材
        if (currentItem.angleSprites != null &&
            currentItem.angleSprites.Length > currentRotationStep &&
            currentItem.angleSprites[currentRotationStep] != null)
        {
            spriteRenderer.sprite = currentItem.angleSprites[currentRotationStep];
        }
        else
        {
            // 如果没设置角度素材，退回到 icon
            spriteRenderer.sprite = currentItem.icon;
        }
    }
}