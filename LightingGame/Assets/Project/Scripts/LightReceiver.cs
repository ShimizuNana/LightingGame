using UnityEngine;

public class LightReceiver : MonoBehaviour
{
    [SerializeField] private bool isActivated = false;
    [SerializeField] private EventCutsceneController eventCutsceneController;

    public void ResetThisFrame()
    {
        isActivated = false;
    }

    public void ReceiveBeam(BeamColor color)
    {
        if (color == BeamColor.Blue)
        {
            if (!isActivated)
            {
                isActivated = true;
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySFX(AudioManager.Instance.beamSuccessClip);
                }
                Debug.Log("接收器收到蓝色光线，触发事件");

                if (eventCutsceneController != null)
                {
                    eventCutsceneController.TriggerEvent();
                }
            }
        }
    }
}