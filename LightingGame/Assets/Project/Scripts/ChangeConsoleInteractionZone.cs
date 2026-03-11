using UnityEngine;

public class ChangeConsoleInteractionZone : MonoBehaviour
{
    [SerializeField] private ChangeConsole changeConsole;
    private int playerColliderCount = 0;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("改变型控制台交互区：进入物体 = " + other.name + " | Tag = " + other.tag);

        if (!other.CompareTag("Player")) return;

        playerColliderCount++;
        if (changeConsole != null)
        {
            changeConsole.SetPlayerInRange(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Debug.Log("改变型控制台交互区：离开物体 = " + other.name + " | Tag = " + other.tag);

        if (!other.CompareTag("Player")) return;

        playerColliderCount--;
        if (playerColliderCount < 0) playerColliderCount = 0;

        if (changeConsole != null)
        {
            changeConsole.SetPlayerInRange(playerColliderCount > 0);
        }
    }
}