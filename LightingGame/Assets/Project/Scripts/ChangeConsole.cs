using UnityEngine;
using UnityEngine.UI;

public class ChangeConsole : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlacedObjectDisplay placedObjectDisplay;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Camera changeConsoleCamera;
    [SerializeField] private Image redViewOverlay;
    [SerializeField] private PlayerFourDirectionMove playerMovement;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLog = true;

    private PlayerHand playerHand;
    private bool playerInRange = false;
    private bool isInSpecialView = false;

    private void Start()
    {
        playerHand = FindFirstObjectByType<PlayerHand>();

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (changeConsoleCamera != null)
        {
            changeConsoleCamera.enabled = false;
        }

        if (redViewOverlay != null)
        {
            redViewOverlay.gameObject.SetActive(false);
        }

        DebugLog("Start 完成");
    }

    private void Update()
    {
        if (!playerInRange) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            DebugLog("按下 E | playerInRange = " + playerInRange + " | isInSpecialView = " + isInSpecialView);

            if (!isInSpecialView)
            {
                TryPlaceAndEnterView();
            }
            else
            {
                ExitSpecialView();
            }
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            DebugLog("按下 F | playerInRange = " + playerInRange + " | isInSpecialView = " + isInSpecialView);

            if (!isInSpecialView)
            {
                TryTakeBackGlass();
            }
            else
            {
                DebugLog("当前仍在特殊视角中，不能回收玻璃");
            }
        }
    }

    public void SetPlayerInRange(bool inRange)
    {
        playerInRange = inRange;
        DebugLog("SetPlayerInRange = " + playerInRange);
    }

    private void TryPlaceAndEnterView()
    {
        if (placedObjectDisplay == null)
        {
            DebugLogError("placedObjectDisplay 没有绑定");
            return;
        }

        if (placedObjectDisplay.HasItem())
        {
            DebugLog("台子上已经有物品，不能再次放置");
            return;
        }

        if (playerHand == null)
        {
            DebugLogError("没有找到 PlayerHand");
            return;
        }

        if (!playerHand.HasItem())
        {
            DebugLog("玩家手上没有物品");
            return;
        }

        ItemData handItem = playerHand.GetHandItem();

        if (handItem == null)
        {
            DebugLogError("手持物品为空");
            return;
        }

        DebugLog("当前手持物品 = " + handItem.itemName);

        if (handItem.itemType != ItemType.Glass)
        {
            DebugLog("该物品不是玻璃，不能放在改变型控制台");
            return;
        }

        if (!handItem.canPlaceOnChangeConsole)
        {
            DebugLog("该玻璃不能放在改变型控制台");
            return;
        }

        if (!handItem.isColoredGlass || handItem.glassColor != LightColor.Red)
        {
            DebugLog("目前改变型控制台只支持红色玻璃");
            return;
        }

        placedObjectDisplay.SetItem(handItem);
        playerHand.ClearHandItem();

        DebugLog("已放置红色玻璃，准备进入特殊视角");

        EnterSpecialView();
    }

    private void EnterSpecialView()
    {
        isInSpecialView = true;

        if (mainCamera != null)
        {
            mainCamera.enabled = false;
        }

        if (changeConsoleCamera != null)
        {
            changeConsoleCamera.enabled = true;
        }

        if (redViewOverlay != null)
        {
            redViewOverlay.gameObject.SetActive(true);
        }

        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        DebugLog("已进入红色特殊视角。后续可在这个视角里显示隐藏机关");
    }

    private void ExitSpecialView()
    {
        isInSpecialView = false;

        if (changeConsoleCamera != null)
        {
            changeConsoleCamera.enabled = false;
        }

        if (mainCamera != null)
        {
            mainCamera.enabled = true;
        }

        if (redViewOverlay != null)
        {
            redViewOverlay.gameObject.SetActive(false);
        }

        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }

        DebugLog("已退出红色特殊视角");
    }

    private void TryTakeBackGlass()
    {
        if (placedObjectDisplay == null)
        {
            DebugLogError("placedObjectDisplay 没有绑定");
            return;
        }

        if (!placedObjectDisplay.HasItem())
        {
            DebugLog("台子上没有物品可回收");
            return;
        }

        ItemData item = placedObjectDisplay.GetItem();

        if (item == null)
        {
            DebugLogError("placedObjectDisplay 显示有物品，但 GetItem() 返回 null");
            return;
        }

        if (InventorySystem.Instance == null)
        {
            DebugLogError("InventorySystem.Instance 为空");
            return;
        }

        bool success = InventorySystem.Instance.AddItem(item);

        if (success)
        {
            placedObjectDisplay.ClearItem();
            DebugLog("红色玻璃已回收到背包");
        }
        else
        {
            DebugLog("背包已满，无法回收玻璃");
        }
    }

    private void DebugLog(string message)
    {
        if (enableDebugLog)
        {
            Debug.Log("[ChangeConsole] " + gameObject.name + " : " + message, this);
        }
    }

    private void DebugLogError(string message)
    {
        Debug.LogError("[ChangeConsole] " + gameObject.name + " : " + message, this);
    }
}