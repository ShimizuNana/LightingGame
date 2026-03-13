using UnityEngine;

public class LightReceiver : MonoBehaviour
{
    [SerializeField] private bool isActivated = false;

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
                Debug.Log("接收器收到蓝色光线，机关触发！");
                ActivateMechanism();
            }
        }
    }

    private void ActivateMechanism()
    {
        Debug.Log("这里后续可以接具体机关逻辑");
    }
}