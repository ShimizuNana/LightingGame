using UnityEngine;

public class NpcQuestReceiver : MonoBehaviour
{
    [Header("Quest Requirement")]
    [SerializeField] private string requiredItemID = "Newspaper";

    [Header("Dialogue - Wrong / Missing Item")]
    [TextArea][SerializeField] private string[] noItemDialogueLines;
    [TextArea][SerializeField] private string[] wrongItemDialogueLines;

    [Header("Dialogue - Correct Item")]
    [TextArea][SerializeField] private string[] successDialogueLines;

    [Header("References")]
    [SerializeField] private DoorController targetDoor;
    [SerializeField] private DialogueUI dialogueUI;

    private bool playerInRange = false;
    private bool hasCompleted = false;
    private bool isTalking = false;

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
        if (isTalking) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            InteractWithNpc();
        }
    }

    private void InteractWithNpc()
    {
        // 如果任务已经完成，仍然可以播放完成后的普通对话
        if (hasCompleted)
        {
            PlayDialogue(successDialogueLines, null);
            return;
        }

        if (playerHand == null)
        {
            Debug.LogWarning("NpcQuestReceiver: 没有找到 PlayerHand");
            return;
        }

        // 情况1：手上没有任何物品
        if (!playerHand.HasItem())
        {
            PlayDialogue(noItemDialogueLines, null);
            return;
        }

        ItemData handItem = playerHand.GetHandItem();

        // 情况2：手上物品为空
        if (handItem == null)
        {
            PlayDialogue(noItemDialogueLines, null);
            return;
        }

        // 情况3：手上拿的不是正确物品
        if (handItem.itemID != requiredItemID)
        {
            PlayDialogue(wrongItemDialogueLines, null);
            return;
        }

        // 情况4：手上拿的是正确物品
        playerHand.ClearHandItem(); // 消耗掉，不回背包，不回地图
        hasCompleted = true;

        Debug.Log("已将正确物品交给 NPC：" + handItem.itemName);

        PlayDialogue(successDialogueLines, OnDialogueFinished);
    }

    private void OnDialogueFinished()
    {
        Debug.Log("NPC 正确物品对话结束");

        if (targetDoor != null)
        {
            targetDoor.OpenDoor();
        }
        else
        {
            Debug.LogWarning("目标门没有绑定");
        }
    }

    private void PlayDialogue(string[] lines, System.Action onComplete)
    {
        if (dialogueUI == null)
        {
            Debug.LogWarning("DialogueUI 没有绑定");
            onComplete?.Invoke();
            return;
        }

        if (lines == null || lines.Length == 0)
        {
            Debug.LogWarning("对话内容为空");
            onComplete?.Invoke();
            return;
        }

        isTalking = true;
        dialogueUI.PlayDialogue(lines, () =>
        {
            isTalking = false;
            onComplete?.Invoke();
        });
    }
}