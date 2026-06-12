using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowRoot : MonoBehaviour
{
    public ArrowData Data { get; private set; }
    public ArrowState State { get; private set; } = ArrowState.Idle;

    [Header("References")]
    public LineRenderer BodyLine;
    public Transform ArrowHead;
    public Transform HitArea;

    [Header("Fly Animation")]
    public float flyDuration = 0.8f;
    public float exitPadding = 2f;
    public AnimationCurve flyCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private ArrowViewConfig config;
    private Coroutine feedbackCoroutine;
    private Coroutine flyCoroutine;
    private readonly List<Vector3> originalWorldPoints = new List<Vector3>();

    public void Initialize(ArrowData data, ArrowViewConfig viewConfig)
    {
        Data = data;
        config = viewConfig;
        SetState(ArrowState.Idle);
        gameObject.name = $"ArrowRoot_{data.id}";
    }

    public void SetBodyLine(List<Vector3> worldPoints)
    {
        originalWorldPoints.Clear();
        originalWorldPoints.AddRange(worldPoints);

        if (BodyLine == null)
        {
            BodyLine = GetComponentInChildren<LineRenderer>();
        }

        if (BodyLine == null)
        {
            Debug.LogError($"{name} missing LineRenderer.");
            return;
        }

        BodyLine.useWorldSpace = true;
        BodyLine.positionCount = worldPoints.Count;
        BodyLine.SetPositions(worldPoints.ToArray());
        BodyLine.startWidth = config.bodyLineWidth;
        BodyLine.endWidth = config.bodyLineWidth;
        BodyLine.startColor = config.arrowColor;
        BodyLine.endColor = config.arrowColor;
        BodyLine.numCapVertices = 6;
        BodyLine.numCornerVertices = 6;

        if (config.lineMaterial != null)
        {
            BodyLine.material = config.lineMaterial;
        }
    }

    public void SetArrowHead(Vector3 position, Direction direction)
    {
        if (ArrowHead == null)
        {
            Debug.LogError($"{name} missing ArrowHead.");
            return;
        }

        ArrowHead.position = position;
        ArrowHead.rotation = DirectionUtil.ToArrowHeadRotation(direction);

        SpriteRenderer spriteRenderer = ArrowHead.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = config.arrowColor;
        }
    }

    public void SetState(ArrowState state)
    {
        State = state;
    }

    public void DisableHitArea()
    {
        if (HitArea == null)
        {
            return;
        }

        Collider2D[] colliders = HitArea.GetComponentsInChildren<Collider2D>(true);

        foreach (Collider2D hitCollider in colliders)
        {
            hitCollider.enabled = false;
        }

        HitArea.gameObject.SetActive(false);
    }

    public void PlayFlyOut(Camera targetCamera, Action onComplete)
    {
        if (flyCoroutine != null)
        {
            StopCoroutine(flyCoroutine);
        }

        flyCoroutine = StartCoroutine(FlyOutRoutine(targetCamera, onComplete));
    }

    public void PlayBlockedFeedbackPlaceholder()
    {
        if (feedbackCoroutine != null)
        {
            StopCoroutine(feedbackCoroutine);
        }

        feedbackCoroutine = StartCoroutine(BlockedFeedbackRoutine());
    }

    private IEnumerator FlyOutRoutine(Camera targetCamera, Action onComplete)
    {
        SetState(ArrowState.Flying);

        if (originalWorldPoints.Count < 2)
        {
            gameObject.SetActive(false);
            onComplete?.Invoke();
            yield break;
        }

        List<Vector3> routePoints = BuildFlyRoute(targetCamera);
        float arrowLength = CalculatePolylineLength(originalWorldPoints);
        float routeLength = CalculatePolylineLength(routePoints);

        float startHeadDistance = arrowLength;
        float endHeadDistance = routeLength;

        float elapsed = 0f;

        while (elapsed < flyDuration)
        {
            elapsed += Time.deltaTime;

            float normalized = Mathf.Clamp01(elapsed / flyDuration);
            float curved = flyCurve != null ? flyCurve.Evaluate(normalized) : normalized;

            float headDistance = Mathf.Lerp(startHeadDistance, endHeadDistance, curved);
            float tailDistance = Mathf.Max(0f, headDistance - arrowLength);

            UpdateVisibleLine(routePoints, tailDistance, headDistance);
            UpdateArrowHead(routePoints, headDistance);

            yield return null;
        }

        UpdateVisibleLine(routePoints, endHeadDistance, endHeadDistance);

        if (BodyLine != null)
        {
            BodyLine.positionCount = 0;
        }

        gameObject.SetActive(false);
        onComplete?.Invoke();
    }

    private List<Vector3> BuildFlyRoute(Camera targetCamera)
    {
        List<Vector3> routePoints = new List<Vector3>();
        routePoints.AddRange(originalWorldPoints);

        Direction headDirection = DirectionUtil.GetHeadDirection(Data);
        Vector3 worldDirection = DirectionUtil.ToWorldDirection(headDirection);

        Vector3 headPosition = originalWorldPoints[originalWorldPoints.Count - 1];
        Vector3 exitPoint = CalculateExitPoint(headPosition, worldDirection, targetCamera);

        routePoints.Add(exitPoint);

        return routePoints;
    }

    private Vector3 CalculateExitPoint(Vector3 headPosition, Vector3 worldDirection, Camera targetCamera)
    {
        if (targetCamera == null)
        {
            return headPosition + worldDirection * 20f;
        }

        float cameraHeight = targetCamera.orthographicSize * 2f;
        float cameraWidth = cameraHeight * targetCamera.aspect;

        Vector3 cameraCenter = targetCamera.transform.position;

        float left = cameraCenter.x - cameraWidth * 0.5f;
        float right = cameraCenter.x + cameraWidth * 0.5f;
        float bottom = cameraCenter.y - cameraHeight * 0.5f;
        float top = cameraCenter.y + cameraHeight * 0.5f;

        float extraDistance = CalculatePolylineLength(originalWorldPoints) + exitPadding;

        if (worldDirection == Vector3.right)
        {
            return new Vector3(right + extraDistance, headPosition.y, headPosition.z);
        }

        if (worldDirection == Vector3.left)
        {
            return new Vector3(left - extraDistance, headPosition.y, headPosition.z);
        }

        if (worldDirection == Vector3.up)
        {
            return new Vector3(headPosition.x, top + extraDistance, headPosition.z);
        }

        if (worldDirection == Vector3.down)
        {
            return new Vector3(headPosition.x, bottom - extraDistance, headPosition.z);
        }

        return headPosition + worldDirection * 20f;
    }

    private void UpdateVisibleLine(List<Vector3> routePoints, float startDistance, float endDistance)
    {
        List<Vector3> visiblePoints = GetPolylineSection(routePoints, startDistance, endDistance);

        if (BodyLine == null)
        {
            return;
        }

        BodyLine.positionCount = visiblePoints.Count;

        if (visiblePoints.Count > 0)
        {
            BodyLine.SetPositions(visiblePoints.ToArray());
        }
    }

    private void UpdateArrowHead(List<Vector3> routePoints, float headDistance)
    {
        if (ArrowHead == null)
        {
            return;
        }

        Vector3 headPosition = GetPointAtDistance(routePoints, headDistance);
        Vector3 direction = GetDirectionAtDistance(routePoints, headDistance);

        ArrowHead.position = headPosition;
        ArrowHead.rotation = GetRotationFromWorldDirection(direction);
    }

    private float CalculatePolylineLength(List<Vector3> points)
    {
        float length = 0f;

        for (int i = 0; i < points.Count - 1; i++)
        {
            length += Vector3.Distance(points[i], points[i + 1]);
        }

        return length;
    }

    private List<Vector3> GetPolylineSection(List<Vector3> points, float startDistance, float endDistance)
    {
        List<Vector3> result = new List<Vector3>();

        if (points == null || points.Count < 2 || endDistance <= startDistance)
        {
            return result;
        }

        float totalLength = CalculatePolylineLength(points);
        startDistance = Mathf.Clamp(startDistance, 0f, totalLength);
        endDistance = Mathf.Clamp(endDistance, 0f, totalLength);

        Vector3 startPoint = GetPointAtDistance(points, startDistance);
        Vector3 endPoint = GetPointAtDistance(points, endDistance);

        result.Add(startPoint);

        float accumulated = 0f;

        for (int i = 0; i < points.Count - 1; i++)
        {
            Vector3 segmentStart = points[i];
            Vector3 segmentEnd = points[i + 1];

            float segmentLength = Vector3.Distance(segmentStart, segmentEnd);
            float segmentStartDistance = accumulated;
            float segmentEndDistance = accumulated + segmentLength;

            if (segmentEndDistance > startDistance && segmentEndDistance < endDistance)
            {
                result.Add(segmentEnd);
            }

            accumulated += segmentLength;
        }

        if (result.Count == 0 || Vector3.Distance(result[result.Count - 1], endPoint) > 0.001f)
        {
            result.Add(endPoint);
        }

        return result;
    }

    private Vector3 GetPointAtDistance(List<Vector3> points, float distance)
    {
        if (points == null || points.Count == 0)
        {
            return Vector3.zero;
        }

        if (points.Count == 1)
        {
            return points[0];
        }

        float accumulated = 0f;

        for (int i = 0; i < points.Count - 1; i++)
        {
            Vector3 start = points[i];
            Vector3 end = points[i + 1];
            float segmentLength = Vector3.Distance(start, end);

            if (accumulated + segmentLength >= distance)
            {
                float t = segmentLength <= 0f ? 0f : (distance - accumulated) / segmentLength;
                return Vector3.Lerp(start, end, t);
            }

            accumulated += segmentLength;
        }

        return points[points.Count - 1];
    }

    private Vector3 GetDirectionAtDistance(List<Vector3> points, float distance)
    {
        if (points == null || points.Count < 2)
        {
            return Vector3.up;
        }

        float accumulated = 0f;

        for (int i = 0; i < points.Count - 1; i++)
        {
            Vector3 start = points[i];
            Vector3 end = points[i + 1];
            float segmentLength = Vector3.Distance(start, end);

            if (accumulated + segmentLength >= distance)
            {
                Vector3 direction = (end - start).normalized;
                return direction.sqrMagnitude > 0f ? direction : Vector3.up;
            }

            accumulated += segmentLength;
        }

        Vector3 finalDirection = (points[points.Count - 1] - points[points.Count - 2]).normalized;
        return finalDirection.sqrMagnitude > 0f ? finalDirection : Vector3.up;
    }

    private Quaternion GetRotationFromWorldDirection(Vector3 direction)
    {
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            if (direction.x > 0f)
            {
                return DirectionUtil.ToArrowHeadRotation(Direction.Right);
            }

            return DirectionUtil.ToArrowHeadRotation(Direction.Left);
        }

        if (direction.y > 0f)
        {
            return DirectionUtil.ToArrowHeadRotation(Direction.Up);
        }

        return DirectionUtil.ToArrowHeadRotation(Direction.Down);
    }

    private IEnumerator BlockedFeedbackRoutine()
    {
        SetState(ArrowState.BlockedFeedback);

        Color originalColor = config.arrowColor;
        Color blockedColor = Color.gray;

        SetVisualColor(blockedColor);
        yield return new WaitForSeconds(0.12f);
        SetVisualColor(originalColor);

        SetState(ArrowState.Idle);
        feedbackCoroutine = null;
    }

    private void SetVisualColor(Color color)
    {
        if (BodyLine != null)
        {
            BodyLine.startColor = color;
            BodyLine.endColor = color;
        }

        if (ArrowHead != null)
        {
            SpriteRenderer spriteRenderer = ArrowHead.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.color = color;
            }
        }
    }
}