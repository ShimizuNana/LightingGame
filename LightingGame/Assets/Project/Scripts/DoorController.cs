using UnityEngine;

public class DoorController : MonoBehaviour
{
    [Header("Door State")]
    [SerializeField] private bool isOpen = false;

    [Header("Visuals")]
    [SerializeField] private SpriteRenderer doorSpriteRenderer;
    [SerializeField] private Sprite closedSprite;
    [SerializeField] private Sprite openSprite;

    [Header("Collision")]
    [SerializeField] private Collider2D doorCollider;

    private void Start()
    {
        RefreshDoorState();
    }

    public void OpenDoor()
    {
        isOpen = true;
        RefreshDoorState();
        Debug.Log("门已打开");
    }

    public void CloseDoor()
    {
        isOpen = false;
        RefreshDoorState();
        Debug.Log("门已关闭");
    }

    public bool IsOpen()
    {
        return isOpen;
    }

    private void RefreshDoorState()
    {
        if (doorSpriteRenderer != null)
        {
            if (isOpen)
            {
                if (openSprite != null)
                    doorSpriteRenderer.sprite = openSprite;
            }
            else
            {
                if (closedSprite != null)
                    doorSpriteRenderer.sprite = closedSprite;
            }
        }

        if (doorCollider != null)
        {
            doorCollider.enabled = !isOpen;
        }
    }
}