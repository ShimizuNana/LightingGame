using UnityEngine;

public class PlacementConsole : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlacedObjectDisplay placedObjectDisplay;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLog = true;

    private bool playerInRange = false;
    private PlayerHand playerHand;

    private void Awake()
    {
        DebugLog("Awake() 运行了");
    }

    private void OnEnable()
    {
        DebugLog("OnEnable() 运行了");
    }

    private void Start()
    {
        DebugLog("Start() 运行了");

        playerHand = FindFirstObjectByType<PlayerHand>();

        if (playerHand == null)
        {
            DebugLogError("Start(): 没有找到 PlayerHand，请确认 Player 身上挂了 PlayerHand");
        }
        else
        {
            DebugLog("Start(): 成功找到 PlayerHand，物体名 = " + playerHand.gameObject.name);
        }

        if (placedObjectDisplay == null)
        {
            DebugLogError("Start(): placedObjectDisplay 没有绑定，请在 Inspector 里拖入");
        }
        else
        {
            DebugLog("Start(): placedObjectDisplay 已绑定，物体名 = " + placedObjectDisplay.gameObject.name);
        }

        DebugLog("Start(): gameObject.activeInHierarchy = " + gameObject.activeInHierarchy);
        DebugLog("Start(): this.enabled = " + enabled);
    }

    private void Update()
    {
        DebugLog("Update() 正在运行");

        if (Input.GetKeyDown(KeyCode.E))
        {
            DebugLog("检测到按下 E，当前 playerInRange = " + playerInRange);

            if (playerInRange)
            {
                TryPlaceItem();
            }
            else
            {
                DebugLog("按下 E 但玩家不在台子交互范围内");
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            DebugLog("检测到按下 R，当前 playerInRange = " + playerInRange);

            if (playerInRange)
            {
                TryRotateItem();
            }
            else
            {
                DebugLog("按下 R 但玩家不在台子交互范围内");
            }
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            DebugLog("检测到按下 F，当前 playerInRange = " + playerInRange);

            if (playerInRange)
            {
                TryTakeBackItem();
            }
            else
            {
                DebugLog("按下 F 但玩家不在台子交互范围内");
            }
        }
    }

    private void OnDisable()
    {
        DebugLogWarning("OnDisable()：PlacementConsole 被禁用了");
    }

    private void OnDestroy()
    {
        DebugLogWarning("OnDestroy()：PlacementConsole 被销毁了");
    }

    public void SetPlayerInRange(bool inRange)
    {
        playerInRange = inRange;
        DebugLog("SetPlayerInRange() 被调用，playerInRange = " + playerInRange);
    }

    private void TryPlaceItem()
    {
        DebugLog("TryPlaceItem()：开始尝试放置");

        if (placedObjectDisplay == null)
        {
            DebugLogError("TryPlaceItem() 失败：placedObjectDisplay 没有绑定");
            return;
        }

        if (placedObjectDisplay.HasItem())
        {
            DebugLogWarning("TryPlaceItem() 失败：台子上已经有物品");
            return;
        }

        if (playerHand == null)
        {
            DebugLogError("TryPlaceItem() 失败：playerHand 为空");
            return;
        }

        if (!playerHand.HasItem())
        {
            DebugLogWarning("TryPlaceItem() 失败：玩家手上没有物品");
            return;
        }

        ItemData handItem = playerHand.GetHandItem();

        if (handItem == null)
        {
            DebugLogError("TryPlaceItem() 失败：GetHandItem() 返回了 null");
            return;
        }

        DebugLog("TryPlaceItem()：当前手持物品 = " + handItem.itemName);
        DebugLog("TryPlaceItem()：itemID = " + handItem.itemID);
        DebugLog("TryPlaceItem()：itemType = " + handItem.itemType);
        DebugLog("TryPlaceItem()：canPlaceOnPlacementConsole = " + handItem.canPlaceOnPlacementConsole);
        DebugLog("TryPlaceItem()：isColoredGlass = " + handItem.isColoredGlass);
        DebugLog("TryPlaceItem()：isMirror = " + handItem.isMirror);

        if (!handItem.canPlaceOnPlacementConsole)
        {
            DebugLogWarning("TryPlaceItem() 失败：该物品不能放在放置型控制台上");
            return;
        }

        placedObjectDisplay.SetItem(handItem);
        playerHand.ClearHandItem();

        DebugLog("TryPlaceItem() 成功：已放置物品 = " + handItem.itemName);

        EvaluateLightInteraction();
    }

    private void TryRotateItem()
    {
        DebugLog("TryRotateItem()：开始尝试旋转");

        if (placedObjectDisplay == null)
        {
            DebugLogError("TryRotateItem() 失败：placedObjectDisplay 没有绑定");
            return;
        }

        if (!placedObjectDisplay.HasItem())
        {
            DebugLogWarning("TryRotateItem() 失败：台子上没有物品可旋转");
            return;
        }

        placedObjectDisplay.Rotate45Degrees();

        DebugLog("TryRotateItem() 成功：当前角度 = " + placedObjectDisplay.GetRotationAngle());

        EvaluateLightInteraction();
    }

    private void TryTakeBackItem()
    {
        DebugLog("TryTakeBackItem()：开始尝试取回物品");

        if (placedObjectDisplay == null)
        {
            DebugLogError("TryTakeBackItem() 失败：placedObjectDisplay 没有绑定");
            return;
        }

        if (!placedObjectDisplay.HasItem())
        {
            DebugLogWarning("TryTakeBackItem() 失败：台子上没有物品");
            return;
        }

        ItemData item = placedObjectDisplay.GetItem();

        if (item == null)
        {
            DebugLogError("TryTakeBackItem() 失败：台子显示有物品，但 GetItem() 返回 null");
            return;
        }

        DebugLog("TryTakeBackItem()：准备取回物品 = " + item.itemName);

        if (InventorySystem.Instance == null)
        {
            DebugLogError("TryTakeBackItem() 失败：InventorySystem.Instance 为空");
            return;
        }

        bool success = InventorySystem.Instance.AddItem(item);

        if (success)
        {
            placedObjectDisplay.ClearItem();
            DebugLog("TryTakeBackItem() 成功：物品已回到背包 = " + item.itemName);

            EvaluateLightInteraction();
        }
        else
        {
            DebugLogWarning("TryTakeBackItem() 失败：背包已满，无法取回物品");
        }
    }

    private void EvaluateLightInteraction()
    {
        DebugLog("EvaluateLightInteraction()：开始评估光线交互");

        if (placedObjectDisplay == null)
        {
            DebugLogError("EvaluateLightInteraction() 失败：placedObjectDisplay 为空");
            return;
        }

        if (!placedObjectDisplay.HasItem())
        {
            DebugLog("EvaluateLightInteraction()：台子为空，不影响光线");
            return;
        }

        ItemData item = placedObjectDisplay.GetItem();

        if (item == null)
        {
            DebugLogError("EvaluateLightInteraction() 失败：HasItem 为 true，但 GetItem() 为 null");
            return;
        }

        float angle = placedObjectDisplay.GetRotationAngle();

        DebugLog("EvaluateLightInteraction()：当前物品 = " + item.itemName);
        DebugLog("EvaluateLightInteraction()：当前角度 = " + angle);

        // 有色玻璃逻辑
        if (item.isColoredGlass)
        {
            if (Mathf.Approximately(angle, 0f) || Mathf.Approximately(angle, 180f))
            {
                DebugLog("有色玻璃生效：光线颜色变为 " + item.glassColor);
            }
            else
            {
                DebugLog("有色玻璃当前角度不生效");
            }
        }

        // 镜子逻辑
        if (item.isMirror)
        {
            if (Mathf.Approximately(angle, 45f))
            {
                DebugLog("镜子生效：光线发生90度折射");
            }
            else
            {
                DebugLog("镜子当前角度不生效");
            }
        }
    }

    private void DebugLog(string message)
    {
        if (enableDebugLog)
        {
            Debug.Log("[PlacementConsole] " + gameObject.name + " : " + message, this);
        }
    }

    private void DebugLogWarning(string message)
    {
        if (enableDebugLog)
        {
            Debug.LogWarning("[PlacementConsole] " + gameObject.name + " : " + message, this);
        }
    }

    private void DebugLogError(string message)
    {
        Debug.LogError("[PlacementConsole] " + gameObject.name + " : " + message, this);
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
}