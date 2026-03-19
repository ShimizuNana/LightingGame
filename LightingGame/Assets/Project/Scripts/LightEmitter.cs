using System.Collections.Generic;
using UnityEngine;

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

    [Header("Visual Settings")]
    [SerializeField] private Material lineMaterial;
    [SerializeField] private float lineWidth = 0.12f;
    [SerializeField] private int sortingOrder = 10;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLog = true;

    private readonly List<LineRenderer> segmentRenderers = new List<LineRenderer>();

    private struct BeamSegment
    {
        public Vector3 start;
        public Vector3 end;
        public BeamColor color;

        public BeamSegment(Vector3 start, Vector3 end, BeamColor color)
        {
            this.start = start;
            this.end = end;
            this.color = color;
        }
    }

    private void Update()
    {
        CastBeam();
    }

    private void CastBeam()
    {
        ResetReceivers();

        List<BeamSegment> segments = new List<BeamSegment>();

        Vector2 currentOrigin = beamOrigin != null
            ? (Vector2)beamOrigin.position
            : (Vector2)transform.position;

        Vector2 currentDirection = startDirection.normalized;
        BeamColor currentColor = startColor;

        Collider2D lastHitCollider = null;

        for (int i = 0; i < maxBounceCount; i++)
        {
            RaycastHit2D hit = GetNextValidHit(currentOrigin, currentDirection, maxDistancePerSegment, lastHitCollider);

            if (hit.collider == null)
            {
                Vector2 endPoint = currentOrigin + currentDirection * maxDistancePerSegment;
                segments.Add(new BeamSegment(currentOrigin, endPoint, currentColor));

                DebugLog("没有命中任何物体，本段结束，颜色 = " + currentColor);
                break;
            }

            PlacementConsole console = hit.collider.GetComponentInParent<PlacementConsole>();
            if (console != null)
            {
                Vector2 exitPoint = console.GetBeamExitPoint();

                segments.Add(new BeamSegment(currentOrigin, exitPoint, currentColor));
                DebugLog("命中放置型控制台：" + hit.collider.name + " | 当前段颜色 = " + currentColor);

                lastHitCollider = hit.collider;

                if (!console.HasPlacedItem())
                {
                    DebugLog("控制台为空，光线直接穿过");
                    currentOrigin = exitPoint;
                    continue;
                }

                ItemData item = console.GetPlacedItem();
                float angle = NormalizeAngle(console.GetPlacedItemAngle());

                if (item == null)
                {
                    DebugLog("控制台显示有物品，但 GetPlacedItem() 返回 null，光线停止");
                    break;
                }

                DebugLog("控制台物品 = " + item.itemName + "，角度 = " + angle);

                if (item.isColoredGlass)
                {
                    if (Mathf.Approximately(angle, 0f) || Mathf.Approximately(angle, 180f))
                    {
                        currentColor = ApplyGlassColor(currentColor, item.glassColor);
                        DebugLog("有色玻璃生效，光线颜色变为 = " + currentColor);

                        currentOrigin = exitPoint;
                        continue;
                    }
                    else
                    {
                        DebugLog("有色玻璃角度不正确，光线停止");
                        break;
                    }
                }

                if (item.isMirror)
                {
                    if (Mathf.Approximately(angle, 45f) || Mathf.Approximately(angle, 225f))
                    {
                        currentDirection = RotateDirection(currentDirection, -90f);
                        DebugLog("镜子生效，方向旋转 -90 度");

                        currentOrigin = exitPoint;
                        continue;
                    }

                    if (Mathf.Approximately(angle, 135f) || Mathf.Approximately(angle, 315f))
                    {
                        currentDirection = RotateDirection(currentDirection, 90f);
                        DebugLog("镜子生效，方向旋转 +90 度");

                        currentOrigin = exitPoint;
                        continue;
                    }

                    DebugLog("镜子角度不正确，光线停止");
                    break;
                }

                DebugLog("控制台上的物品不改变光线，默认穿过");
                currentOrigin = exitPoint;
                continue;
            }

            segments.Add(new BeamSegment(currentOrigin, hit.point, currentColor));
            DebugLog("命中物体：" + hit.collider.name + " | 当前段颜色 = " + currentColor);

            lastHitCollider = hit.collider;

            LightReceiver receiver = hit.collider.GetComponentInParent<LightReceiver>();
            if (receiver != null)
            {
                DebugLog("命中接收器，发送颜色 = " + currentColor);
                receiver.ReceiveBeam(currentColor);
                break;
            }

            DebugLog("命中阻挡物，光线停止");
            break;
        }

        DrawSegments(segments);
    }

    private RaycastHit2D GetNextValidHit(Vector2 origin, Vector2 direction, float distance, Collider2D ignoreCollider)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(origin, direction, distance, hitMask);

        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].collider == null) continue;

            if (ignoreCollider != null && hits[i].collider == ignoreCollider)
                continue;

            if (hits[i].distance <= 0.0001f)
                continue;

            return hits[i];
        }

        return new RaycastHit2D();
    }

    private void DrawSegments(List<BeamSegment> segments)
    {
        EnsureSegmentRendererCount(segments.Count);

        for (int i = 0; i < segmentRenderers.Count; i++)
        {
            if (i < segments.Count)
            {
                segmentRenderers[i].gameObject.SetActive(true);
                SetupSegmentRenderer(segmentRenderers[i], segments[i]);
            }
            else
            {
                segmentRenderers[i].gameObject.SetActive(false);
            }
        }
    }

    private void EnsureSegmentRendererCount(int count)
    {
        while (segmentRenderers.Count < count)
        {
            GameObject segmentObj = new GameObject("BeamSegment_" + segmentRenderers.Count);
            segmentObj.transform.SetParent(transform, false);

            LineRenderer lr = segmentObj.AddComponent<LineRenderer>();
            lr.useWorldSpace = true;
            lr.positionCount = 2;
            lr.widthMultiplier = lineWidth;
            lr.sortingOrder = sortingOrder;

            if (lineMaterial != null)
            {
                lr.material = lineMaterial;
            }

            segmentRenderers.Add(lr);
        }
    }

    private void SetupSegmentRenderer(LineRenderer lr, BeamSegment segment)
    {
        lr.positionCount = 2;
        lr.SetPosition(0, segment.start);
        lr.SetPosition(1, segment.end);

        Color color = ConvertBeamColor(segment.color);

        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(color, 0f),
                new GradientColorKey(color, 1f)
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 1f)
            }
        );

        lr.colorGradient = gradient;
        lr.widthMultiplier = lineWidth;

        if (lineMaterial != null && lr.material != lineMaterial)
        {
            lr.material = lineMaterial;
        }
    }

    private BeamColor ApplyGlassColor(BeamColor currentColor, LightColor glassColor)
    {
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