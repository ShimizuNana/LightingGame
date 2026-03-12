using UnityEngine;

public class NpcQuestReceiver : MonoBehaviour
{
    [Header("Quest Requirement")]
    [SerializeField] private string requiredItemID = "Newspaper";

    [Header("Dialogue")]
    [TextArea][SerializeField] private string[] dialogueLines;

    [Header("References")]
    [SerializeField] private DoorController targetDoor;
    [SerializeField] private DialogueUI dialogueUI;

    private bool playerInRange = false;
    private bool hasCompleted = false;
    private PlayerHand playerHand;

    private void Start()
    {
        playerHand = FindFirstObjectByType<PlayerHand>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }

    private void Update()
    {
        if (!playerInRange) return;
        if (hasCompleted) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            TryGiveItemToNpc();
        }
    }

    private void TryGiveItemToNpc()
    {
        if (playerHand == null) return;
        if (!playerHand.HasItem())
        {
            Debug.Log("玩家手上没有物品，无法交给NPC");
            return;
        }

        ItemData handItem = playerHand.GetHandItem();

        if (handItem == null)
        {
            Debug.Log("手上的物品为空");
            return;
        }

        if (handItem.itemID != requiredItemID)
        {
            Debug.Log("这不是NPC需要的物品");
            return;
        }

        // 销毁物品：直接从手上清掉，不回背包，不回地图
        playerHand.ClearHandItem();

        Debug.Log("已将物品交给NPC：" + handItem.itemName);

        hasCompleted = true;

        if (dialogueUI != null)
        {
            dialogueUI.PlayDialogue(dialogueLines, OnDialogueFinished);
        }
        else
        {
            Debug.LogWarning("DialogueUI 没有绑定，直接开门");
            OnDialogueFinished();
        }
    }

    private void OnDialogueFinished()
    {
        Debug.Log("NPC 对话结束");

        if (targetDoor != null)
        {
            targetDoor.OpenDoor();
        }
        else
        {
            Debug.LogWarning("目标门没有绑定");
        }
    }
}