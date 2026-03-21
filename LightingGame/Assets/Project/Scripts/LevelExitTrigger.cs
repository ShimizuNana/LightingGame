using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelExitTrigger : MonoBehaviour
{
    [SerializeField] private DoorController targetDoor;
    [SerializeField] private string clearSceneName = "ClearScene";

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (targetDoor == null)
        {
            Debug.LogWarning("LevelExitTrigger：没有绑定 targetDoor");
            return;
        }

        if (!targetDoor.IsOpen())
        {
            Debug.Log("门还没打开，不能通关");
            return;
        }

        Debug.Log("玩家进入出口，跳转到场景：" + clearSceneName);
        SceneManager.LoadScene(clearSceneName);
    }
}