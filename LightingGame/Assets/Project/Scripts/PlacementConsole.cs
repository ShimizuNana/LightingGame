using UnityEngine;

public class PlacementConsole : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlacedObjectDisplay placedObjectDisplay;

    [Header("Beam Exit Points")]
    [SerializeField] private Transform beamExitUp;
    [SerializeField] private Transform beamExitDown;
    [SerializeField] private Transform beamExitLeft;
    [SerializeField] private Transform beamExitRight;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLog = false;

    private bool playerInRange = false;
    private PlayerHand playerHand;

    private void Start()
    {
        playerHand = FindFirstObjectByType<PlayerHand>();

        if (playerHand == null)
        {
            DebugLog("没有找到 PlayerHand");
        }

        if (placedObjectDisplay == null)
        {
            DebugLog("placedObjectDisplay 没有绑定");
        }
    }

    private void Update()
    {
        if (!playerInRange) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            TryPlaceItem();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            TryRotateItem();
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            TryTakeBackItem();
        }
    }

    public void SetPlayerInRange(bool inRange)
    {
        playerInRange = inRange;
    }

    public bool HasPlacedItem()
    {
        return placedObjectDisplay != null && placedObjectDisplay.HasItem();
    }

    public ItemData GetPlacedItem()
    {
        if (placedObjectDisplay == null) return null;
        return placedObjectDisplay.GetItem();
    }

    public float GetPlacedItemAngle()
    {
        if (placedObjectDisplay == null) return 0f;
        return placedObjectDisplay.GetRotationAngle();
    }

    // 根据“当前光线传播方向”返回控制台对应的出射点
    public Vector2 GetBeamExitPoint(Vector2 beamDirection)
    {
        Vector2 dir = beamDirection.normalized;

        // 先判断主方向：水平 or 竖直
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            // 向右传播
            if (dir.x > 0f)
            {
                if (beamExitRight != null) return beamExitRight.position;
            }
            // 向左传播
            else
            {
                if (beamExitLeft != null) return beamExitLeft.position;
            }
        }
        else
        {
            // 向上传播
            if (dir.y > 0f)
            {
                if (beamExitUp != null) return beamExitUp.position;
            }
            // 向下传播
            else
            {
                if (beamExitDown != null) return beamExitDown.position;
            }
        }

        return transform.position;
    }

    private void TryPlaceItem()
    {
        if (placedObjectDisplay == null) return;
        if (placedObjectDisplay.HasItem()) return;
        if (playerHand == null) return;
        if (!playerHand.HasItem()) return;

        ItemData handItem = playerHand.GetHandItem();
        if (handItem == null) return;
        if (!handItem.canPlaceOnPlacementConsole) return;

        placedObjectDisplay.SetItem(handItem);
        playerHand.ClearHandItem();

        DebugLog("成功放置物品：" + handItem.itemName);
    }

    private void TryRotateItem()
    {
        if (placedObjectDisplay == null) return;
        if (!placedObjectDisplay.HasItem()) return;

        placedObjectDisplay.Rotate45Degrees();

        DebugLog("旋转后角度：" + placedObjectDisplay.GetRotationAngle());
    }

    private void TryTakeBackItem()
    {
        if (placedObjectDisplay == null) return;
        if (!placedObjectDisplay.HasItem()) return;

        ItemData item = placedObjectDisplay.GetItem();
        if (item == null) return;
        if (InventorySystem.Instance == null) return;

        bool success = InventorySystem.Instance.AddItem(item);

        if (success)
        {
            placedObjectDisplay.ClearItem();
            DebugLog("物品已放回背包：" + item.itemName);
        }
        else
        {
            DebugLog("背包已满，无法取回");
        }
    }

    private void DebugLog(string message)
    {
        if (enableDebugLog)
        {
            Debug.Log("[PlacementConsole] " + gameObject.name + " : " + message, this);
        }
    }
}