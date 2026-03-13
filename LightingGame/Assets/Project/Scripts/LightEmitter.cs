using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LightEmitter : MonoBehaviour
{
    [Header("Beam Settings")]
    [SerializeField] private float maxDistancePerSegment = 20f;
    [SerializeField] private int maxBounceCount = 10;
    [SerializeField] private LayerMask hitMask;

    [Header("Start Settings")]
    [SerializeField] private BeamColor startColor = BeamColor.White;
    [SerializeField] private Vector2 startDirection = Vector2.right;

    [Header("Beam Origin")]
    [SerializeField] private Transform beamOrigin;
    [SerializeField] private float hitPointOffset = 0.01f;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLog = false;

    private LineRenderer lineRenderer;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    private void Update()
    {
        CastBeam();
    }

    private void CastBeam()
    {
        ResetReceivers();

        List<Vector3> points = new List<Vector3>();

        // 如果有单独的 BeamOrigin，就从 BeamOrigin 发射
        Vector2 currentOrigin = beamOrigin != null
            ? (Vector2)beamOrigin.position
            : (Vector2)transform.position;

        Vector2 currentDirection = startDirection.normalized;
        BeamColor currentColor = startColor;

        points.Add(currentOrigin);

        for (int i = 0; i < maxBounceCount; i++)
        {
            RaycastHit2D hit = Physics2D.Raycast(currentOrigin, currentDirection, maxDistancePerSegment, hitMask);

            if (hit.collider == null)
            {
                Vector2 endPoint = currentOrigin + currentDirection * maxDistancePerSegment;
                points.Add(endPoint);

                DebugLog("没有命中任何物体，光线结束");
                break;
            }

            points.Add(hit.point);
            DebugLog("命中物体：" + hit.collider.name);

            // 1. 打到放置型控制台
            PlacementConsole console = hit.collider.GetComponent<PlacementConsole>();
            if (console != null && console.HasPlacedItem())
            {
                ItemData item = console.GetPlacedItem();
                float angle = NormalizeAngle(console.GetPlacedItemAngle());

                if (item != null)
                {
                    DebugLog("命中放置型控制台，物品 = " + item.itemName + "，角度 = " + angle);

                    // 有色玻璃：0 / 180 度变色
                    if (item.isColoredGlass && (Mathf.Approximately(angle, 0f) || Mathf.Approximately(angle, 180f)))
                    {
                        currentColor = ApplyGlassColor(currentColor, item.glassColor);

                        DebugLog("有色玻璃生效，当前光线颜色 = " + currentColor);

                        currentOrigin = hit.point + currentDirection * hitPointOffset;
                        continue;
                    }

                    // 镜子：45 / 225 => -90 度，135 / 315 => +90 度
                    if (item.isMirror)
                    {
                        if (Mathf.Approximately(angle, 45f) || Mathf.Approximately(angle, 225f))
                        {
                            currentDirection = RotateDirection(currentDirection, -90f);
                            DebugLog("镜子生效，方向旋转 -90 度");

                            currentOrigin = hit.point + currentDirection * hitPointOffset;
                            continue;
                        }

                        if (Mathf.Approximately(angle, 135f) || Mathf.Approximately(angle, 315f))
                        {
                            currentDirection = RotateDirection(currentDirection, 90f);
                            DebugLog("镜子生效，方向旋转 +90 度");

                            currentOrigin = hit.point + currentDirection * hitPointOffset;
                            continue;
                        }
                    }
                }
            }

            // 2. 打到接收器
            LightReceiver receiver = hit.collider.GetComponent<LightReceiver>();
            if (receiver != null)
            {
                DebugLog("命中接收器，发送颜色 = " + currentColor);
                receiver.ReceiveBeam(currentColor);
                break;
            }

            // 3. 打到其他阻挡物，停止
            DebugLog("命中阻挡物，光线停止");
            break;
        }

        DrawBeam(points, currentColor);
    }

    private void DrawBeam(List<Vector3> points, BeamColor color)
    {
        if (lineRenderer == null) return;

        lineRenderer.positionCount = points.Count;
        lineRenderer.SetPositions(points.ToArray());

        Color unityColor = ConvertBeamColor(color);

        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[]
            {
            new GradientColorKey(unityColor, 0f),
            new GradientColorKey(unityColor, 1f)
            },
            new GradientAlphaKey[]
            {
            new GradientAlphaKey(1f, 0f),
            new GradientAlphaKey(1f, 1f)
            }
        );

        lineRenderer.colorGradient = gradient;
    }

    private BeamColor ApplyGlassColor(BeamColor currentColor, LightColor glassColor)
    {
        // 第一版先简化：玻璃颜色直接覆盖当前颜色
        switch (glassColor)
        {
            case LightColor.Red:
                return BeamColor.Red;
            case LightColor.Blue:
                return BeamColor.Blue;
            case LightColor.Green:
                return BeamColor.Green;
            case LightColor.Yellow:
                return BeamColor.Yellow;
            default:
                return currentColor;
        }
    }

    private Vector2 RotateDirection(Vector2 direction, float angle)
    {
        Quaternion rotation = Quaternion.Euler(0f, 0f, angle);
        return rotation * direction;
    }

    private float NormalizeAngle(float angle)
    {
        angle %= 360f;
        if (angle < 0f) angle += 360f;
        return angle;
    }

    private Color ConvertBeamColor(BeamColor beamColor)
    {
        switch (beamColor)
        {
            case BeamColor.Red:
                return new Color(1f, 0.2f, 0.2f, 1f);
            case BeamColor.Blue:
                return new Color(0.2f, 0.7f, 1f, 1f);
            case BeamColor.Green:
                return new Color(0.2f, 1f, 0.2f, 1f);
            case BeamColor.Yellow:
                return new Color(1f, 0.9f, 0.2f, 1f);
            default:
                return Color.white;
        }
    }

    private void ResetReceivers()
    {
        LightReceiver[] receivers = FindObjectsByType<LightReceiver>(FindObjectsSortMode.None);
        foreach (LightReceiver receiver in receivers)
        {
            receiver.ResetThisFrame();
        }
    }

    private void DebugLog(string message)
    {
        if (enableDebugLog)
        {
            Debug.Log("[LightEmitter] " + gameObject.name + " : " + message, this);
        }
    }
}