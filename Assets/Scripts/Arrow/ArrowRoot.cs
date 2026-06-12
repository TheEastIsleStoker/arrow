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

    private ArrowViewConfig config;
    private Coroutine feedbackCoroutine;

    public void Initialize(ArrowData data, ArrowViewConfig viewConfig)
    {
        Data = data;
        config = viewConfig;
        SetState(ArrowState.Idle);
        gameObject.name = $"ArrowRoot_{data.id}";
    }

    public void SetBodyLine(List<Vector3> worldPoints)
    {
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

        if (config.lineMaterial != null)
        {
            BodyLine.material = config.lineMaterial;
        }
    }

    public void SetArrowHead(Vector3 position, Direction dir)
    {
        if (ArrowHead == null)
        {
            Debug.LogError($"{name} missing ArrowHead.");
            return;
        }

        ArrowHead.position = position;
        ArrowHead.rotation = DirectionUtil.ToArrowHeadRotation(dir);

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

    public void PlayFlyOutPlaceholder(Action onComplete)
    {
        SetState(ArrowState.Flying);
        gameObject.SetActive(false);
        onComplete?.Invoke();
    }

    public void PlayBlockedFeedbackPlaceholder()
    {
        if (feedbackCoroutine != null)
        {
            StopCoroutine(feedbackCoroutine);
        }

        feedbackCoroutine = StartCoroutine(BlockedFeedbackRoutine());
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