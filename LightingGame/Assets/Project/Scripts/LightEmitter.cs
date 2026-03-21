using System.Collections.Generic;
using UnityEngine;

public class LightEmitter : MonoBehaviour
{
    [Header("Beam Settings")]
    [SerializeField] private float maxDistancePerSegment = 20f;
    [SerializeField] private int maxBounceCount = 20;
    [SerializeField] private LayerMask hitMask;

    [Header("Start Settings")]
    [SerializeField] private BeamColor startColor = BeamColor.White;
    [SerializeField] private Vector2 startDirection = Vector2.right;

    [Header("Beam Origins")]
    [SerializeField] private Transform beamOriginUp;
    [SerializeField] private Transform beamOriginDown;
    [SerializeField] private Transform beamOriginLeft;
    [SerializeField] private Transform beamOriginRight;

    [Header("Raycast Restart")]
    [SerializeField] private float rayRestartOffset = 0.01f;

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

        Vector2 currentDirection = SnapToCardinal(startDirection.normalized);
        Vector2 currentOrigin = GetStartOriginByDirection(currentDirection);

        BeamColor currentColor = startColor;
        GameObject lastIgnoredConsoleRoot = null;

        Log("====== 新一轮光线计算开始 ======");
        Log("初始方向 = " + currentDirection + " | 初始起点 = " + currentOrigin + " | 初始颜色 = " + currentColor);

        for (int i = 0; i < maxBounceCount; i++)
        {
            Debug.DrawRay(currentOrigin, currentDirection * maxDistancePerSegment, Color.magenta, 0f, false);

            RaycastHit2D hit = GetNextValidHit(
                currentOrigin,
                currentDirection,
                maxDistancePerSegment,
                lastIgnoredConsoleRoot
            );

            if (hit.collider == null)
            {
                Vector2 endPoint = currentOrigin + currentDirection * maxDistancePerSegment;
                segments.Add(new BeamSegment(currentOrigin, endPoint, currentColor));
                Log("没有命中任何物体，本段结束");
                break;
            }

            Log("命中物体 = " + hit.collider.name +
                " | Layer = " + LayerMask.LayerToName(hit.collider.gameObject.layer) +
                " | Point = " + hit.point);

            PlacementConsole console = hit.collider.GetComponentInParent<PlacementConsole>();
            if (console != null)
            {
                ItemData item = console.GetPlacedItem();
                Vector2 passExitPoint = console.GetBeamExitPoint(currentDirection);

                Log("命中控制台 = " + console.name + " | 当前方向对应出射点 = " + passExitPoint);

                // 空控制台：直接穿过
                if (!console.HasPlacedItem() || item == null)
                {
                    segments.Add(new BeamSegment(currentOrigin, passExitPoint, currentColor));
                    Log("控制台为空 -> 直接穿过");

                    lastIgnoredConsoleRoot = console.gameObject;
                    currentOrigin = passExitPoint + currentDirection * rayRestartOffset;
                    continue;
                }

                float itemAngle = NormalizeAngle(console.GetPlacedItemAngle());
                float referenceOffset = GetReferenceOffsetByDirection(currentDirection);
                float relativeAngle = NormalizeAngle(itemAngle - referenceOffset);

                Log(
                    "控制台物品 = " + item.itemName +
                    " | isColoredGlass = " + item.isColoredGlass +
                    " | isMirror = " + item.isMirror +
                    " | 世界角度 = " + itemAngle +
                    " | 当前方向 = " + currentDirection +
                    " | 参考偏移 = " + referenceOffset +
                    " | 相对角度 = " + relativeAngle +
                    " | 当前颜色 = " + currentColor
                );

                // 有色玻璃：相对角度 0 / 180 生效
                if (item.isColoredGlass)
                {
                    segments.Add(new BeamSegment(currentOrigin, passExitPoint, currentColor));

                    bool glassMatch = Mathf.Approximately(relativeAngle, 0f) || Mathf.Approximately(relativeAngle, 180f);
                    Log("有色玻璃判定 = " + glassMatch);

                    if (glassMatch)
                    {
                        currentColor = ApplyGlassColor(currentColor, item.glassColor);
                        Log("有色玻璃生效 -> 新颜色 = " + currentColor);

                        lastIgnoredConsoleRoot = console.gameObject;
                        currentOrigin = passExitPoint + currentDirection * rayRestartOffset;
                        continue;
                    }
                    else
                    {
                        Log("有色玻璃角度不匹配，光线停止");
                        break;
                    }
                }

                // 镜子：相对角度 45 / 225 => 右拐(-90)，135 / 315 => 左拐(+90)
                if (item.isMirror)
                {
                    Vector2 newDirection;
                    bool mirrorMatched = false;

                    if (Mathf.Approximately(relativeAngle, 45f) || Mathf.Approximately(relativeAngle, 225f))
                    {
                        newDirection = SnapToCardinal(RotateDirection(currentDirection, -90f));
                        mirrorMatched = true;
                    }
                    else if (Mathf.Approximately(relativeAngle, 135f) || Mathf.Approximately(relativeAngle, 315f))
                    {
                        newDirection = SnapToCardinal(RotateDirection(currentDirection, 90f));
                        mirrorMatched = true;
                    }
                    else
                    {
                        newDirection = currentDirection;
                    }

                    Log("镜子判定 = " + mirrorMatched);

                    if (!mirrorMatched)
                    {
                        Log("镜子角度不匹配，光线停止");
                        break;
                    }

                    Vector2 mirrorExitPoint = console.GetBeamExitPoint(newDirection);
                    segments.Add(new BeamSegment(currentOrigin, mirrorExitPoint, currentColor));

                    Log("镜子生效 -> 新方向 = " + newDirection +
                        " | 新出射点 = " + mirrorExitPoint +
                        " | 颜色保持 = " + currentColor);

                    currentDirection = newDirection;
                    lastIgnoredConsoleRoot = console.gameObject;
                    currentOrigin = mirrorExitPoint + currentDirection * rayRestartOffset;
                    continue;
                }

                // 其他物品默认穿过
                segments.Add(new BeamSegment(currentOrigin, passExitPoint, currentColor));
                Log("控制台上的物品不影响光线 -> 默认穿过");

                lastIgnoredConsoleRoot = console.gameObject;
                currentOrigin = passExitPoint + currentDirection * rayRestartOffset;
                continue;
            }

            // 非控制台
            segments.Add(new BeamSegment(currentOrigin, hit.point, currentColor));
            lastIgnoredConsoleRoot = null;

            LightReceiver receiver = hit.collider.GetComponentInParent<LightReceiver>();
            if (receiver != null)
            {
                Log("命中接收器 -> 发送颜色 = " + currentColor);
                receiver.ReceiveBeam(currentColor);
                break;
            }

            Log("命中阻挡物 -> 光线停止");
            break;
        }

        DrawSegments(segments);
    }

    private Vector2 GetStartOriginByDirection(Vector2 direction)
    {
        Vector2 dir = SnapToCardinal(direction);

        if (dir == Vector2.up && beamOriginUp != null)
            return beamOriginUp.position;

        if (dir == Vector2.down && beamOriginDown != null)
            return beamOriginDown.position;

        if (dir == Vector2.left && beamOriginLeft != null)
            return beamOriginLeft.position;

        if (dir == Vector2.right && beamOriginRight != null)
            return beamOriginRight.position;

        return transform.position;
    }

    private RaycastHit2D GetNextValidHit(
        Vector2 origin,
        Vector2 direction,
        float distance,
        GameObject ignoreConsoleRoot)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(origin, direction, distance, hitMask);

        if (enableDebugLog)
        {
            if (hits.Length == 0)
            {
                Log("RaycastAll 命中数量 = 0");
            }
            else
            {
                string info = "";
                for (int i = 0; i < hits.Length; i++)
                {
                    if (hits[i].collider == null) continue;
                    info += $"[{i}] {hits[i].collider.name} | Layer={LayerMask.LayerToName(hits[i].collider.gameObject.layer)} | Distance={hits[i].distance:F3}\n";
                }
                Log("RaycastAll 命中列表：\n" + info);
            }
        }

        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].collider == null)
                continue;

            PlacementConsole hitConsole = hits[i].collider.GetComponentInParent<PlacementConsole>();
            if (ignoreConsoleRoot != null && hitConsole != null && hitConsole.gameObject == ignoreConsoleRoot)
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

    private float GetReferenceOffsetByDirection(Vector2 direction)
    {
        Vector2 dir = SnapToCardinal(direction);

        if (dir == Vector2.up || dir == Vector2.down)
            return 0f;

        return 90f;
    }

    private BeamColor ApplyGlassColor(BeamColor currentColor, LightColor glassColor)
    {
        switch (glassColor)
        {
            case LightColor.Red: return BeamColor.Red;
            case LightColor.Blue: return BeamColor.Blue;
            case LightColor.Green: return BeamColor.Green;
            case LightColor.Yellow: return BeamColor.Yellow;
            default: return currentColor;
        }
    }

    private Vector2 RotateDirection(Vector2 direction, float angle)
    {
        Quaternion rotation = Quaternion.Euler(0f, 0f, angle);
        return rotation * direction;
    }

    private Vector2 SnapToCardinal(Vector2 dir)
    {
        dir = dir.normalized;

        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            return dir.x >= 0f ? Vector2.right : Vector2.left;
        }
        else
        {
            return dir.y >= 0f ? Vector2.up : Vector2.down;
        }
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
            case BeamColor.Red: return new Color(1f, 0.2f, 0.2f, 1f);
            case BeamColor.Blue: return new Color(0.2f, 0.7f, 1f, 1f);
            case BeamColor.Green: return new Color(0.2f, 1f, 0.2f, 1f);
            case BeamColor.Yellow: return new Color(1f, 0.9f, 0.2f, 1f);
            default: return Color.white;
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

    private void Log(string message)
    {
        if (enableDebugLog)
        {
            Debug.Log("[LightEmitter] " + gameObject.name + " : " + message, this);
        }
    }
}