using System.Collections;
using UnityEngine;

public class EventCutsceneController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Camera eventCamera;
    [SerializeField] private PlayerFourDirectionMove playerMovement;
    [SerializeField] private MechanismController mechanismController;
    [SerializeField] private DoorController targetDoor;
    [SerializeField] private FogOverlayController fogOverlayController;

    [Header("Camera Points")]
    [SerializeField] private Transform mechanismViewPoint;
    [SerializeField] private Transform doorViewPoint;

    [Header("Timing")]
    [SerializeField] private float mechanismViewStayTime = 0.8f;
    [SerializeField] private float cameraMoveDuration = 1.2f;
    [SerializeField] private float afterDoorOpenStayTime = 1.5f;

    private bool hasPlayed = false;
    private bool isPlaying = false;

    private void Start()
    {
        if (eventCamera != null)
        {
            eventCamera.enabled = false;
        }
    }

    public void TriggerEvent()
    {
        if (hasPlayed || isPlaying) return;
        StartCoroutine(PlayEventSequence());
    }

    private IEnumerator PlayEventSequence()
    {
        isPlaying = true;

        // 1. 禁用玩家移动
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        // 2. 切到事件相机
        if (mainCamera != null)
        {
            mainCamera.enabled = false;
        }

        if (eventCamera != null)
        {
            eventCamera.enabled = true;
        }

        // 3. 先把事件相机放到机关视角点
        if (eventCamera != null && mechanismViewPoint != null)
        {
            eventCamera.transform.position = mechanismViewPoint.position;
            eventCamera.transform.rotation = mechanismViewPoint.rotation;
        }

        Debug.Log("事件相机切到机关视角");

        
        // 先来一点相机震动
        yield return StartCoroutine(ShakeEventCamera(0.25f, 0.08f));

        // Fog 淡出
        if (fogOverlayController != null)
        {
            yield return StartCoroutine(fogOverlayController.FadeOutFog());
        }
        yield return new WaitForSeconds(0.3f); // 等待一点时间，让 Fog 淡出完成

        // 4. 播放机关变化
        if (mechanismController != null)
        {
            yield return StartCoroutine(mechanismController.PlayActivationSequence());
        }

        // 5. 稍微停留，让玩家看清机关变化完成
        yield return new WaitForSeconds(mechanismViewStayTime);

        // 6. 平滑移动到大门视角点
        if (eventCamera != null && doorViewPoint != null)
        {
            yield return StartCoroutine(MoveCameraToPoint(doorViewPoint));
        }

        Debug.Log("事件相机移动到大门视角");

        // 7. 打开门
        if (targetDoor != null)
        {
            targetDoor.OpenDoor();
        }

        // 8. 停留，让玩家看清门打开
        yield return new WaitForSeconds(afterDoorOpenStayTime);

        // 9. 切回主相机
        if (eventCamera != null)
        {
            eventCamera.enabled = false;
        }

        if (mainCamera != null)
        {
            mainCamera.enabled = true;
        }

        // 10. 恢复玩家移动
        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }

        Debug.Log("事件展示结束，恢复玩家控制");

        hasPlayed = true;
        isPlaying = false;
    }

    private IEnumerator MoveCameraToPoint(Transform targetPoint)
    {
        Vector3 startPos = eventCamera.transform.position;
        Quaternion startRot = eventCamera.transform.rotation;

        Vector3 endPos = targetPoint.position;
        Quaternion endRot = targetPoint.rotation;

        float elapsed = 0f;

        while (elapsed < cameraMoveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / cameraMoveDuration);

            eventCamera.transform.position = Vector3.Lerp(startPos, endPos, t);
            eventCamera.transform.rotation = Quaternion.Lerp(startRot, endRot, t);

            yield return null;
        }

        eventCamera.transform.position = endPos;
        eventCamera.transform.rotation = endRot;
    }

    private IEnumerator ShakeEventCamera(float duration, float strength)
    {
        if (eventCamera == null) yield break;

        Vector3 originalPos = eventCamera.transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float offsetX = Random.Range(-strength, strength);
            float offsetY = Random.Range(-strength, strength);

            eventCamera.transform.position = new Vector3(
                originalPos.x + offsetX,
                originalPos.y + offsetY,
                originalPos.z
            );

            yield return null;
        }

        eventCamera.transform.position = originalPos;
    }
}