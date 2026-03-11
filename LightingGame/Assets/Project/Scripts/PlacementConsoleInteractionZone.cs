using UnityEngine;

public class PlacementConsoleInteractionZone : MonoBehaviour
{
    [SerializeField] private PlacementConsole placementConsole;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("有物体进入交互区：" + other.name);

        if (other.CompareTag("Player"))
        {
            Debug.Log("玩家进入放置型控制台范围");
            placementConsole.SetPlayerInRange(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Debug.Log("有物体离开交互区：" + other.name);

        if (other.CompareTag("Player"))
        {
            Debug.Log("玩家离开放置型控制台范围");
            placementConsole.SetPlayerInRange(false);
        }
    }
}