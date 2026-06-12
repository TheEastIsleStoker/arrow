using System.Collections.Generic;
using UnityEngine;

public class ArrowFactory : MonoBehaviour
{
    [Header("Scene")]
    public Transform boardRoot;

    [Header("View")]
    public ArrowViewConfig viewConfig = new ArrowViewConfig();

    [Header("Runtime Assets")]
    public Sprite arrowHeadSprite;

    public ArrowRoot CreateArrow(ArrowData data, LevelData levelData)
    {
        if (data == null || data.path == null || data.path.Count < 2)
        {
            Debug.LogError("Invalid arrow data. Path must contain at least 2 points.");
            return null;
        }

        ArrowRoot arrowRoot = CreateArrowRootObject(data);
        arrowRoot.Initialize(data, viewConfig);

        List<Vector3> worldPoints = new List<Vector3>();

        foreach (GridPos point in data.path)
        {
            worldPoints.Add(GridToWorld(point, levelData));
        }

        arrowRoot.SetBodyLine(worldPoints);

        GridPos headGridPos = data.path[data.path.Count - 1];
        Vector3 headWorldPos = GridToWorld(headGridPos, levelData);
        Direction headDirection = DirectionUtil.GetHeadDirection(data);

        arrowRoot.SetArrowHead(headWorldPos, headDirection);

        CreateSegmentHits(arrowRoot, data, levelData);

        return arrowRoot;
    }

    public Vector3 GridToWorld(GridPos pos, LevelData levelData)
    {
        float cellSize = levelData.cellSize;
        float originOffsetX = (levelData.boardWidth - 1) * 0.5f;
        float originOffsetY = (levelData.boardHeight - 1) * 0.5f;

        float worldX = (pos.x - originOffsetX) * cellSize;
        float worldY = (pos.y - originOffsetY) * cellSize;

        return new Vector3(worldX, worldY, 0f);
    }

    private ArrowRoot CreateArrowRootObject(ArrowData data)
    {
        Transform parent = boardRoot != null ? boardRoot : transform;

        GameObject rootObject = new GameObject($"ArrowRoot_{data.id}");
        rootObject.transform.SetParent(parent);
        rootObject.transform.localPosition = Vector3.zero;
        rootObject.transform.localRotation = Quaternion.identity;
        rootObject.transform.localScale = Vector3.one;

        ArrowRoot arrowRoot = rootObject.AddComponent<ArrowRoot>();

        GameObject bodyLineObject = new GameObject("BodyLine");
        bodyLineObject.transform.SetParent(rootObject.transform);
        bodyLineObject.transform.localPosition = Vector3.zero;
        bodyLineObject.transform.localRotation = Quaternion.identity;
        bodyLineObject.transform.localScale = Vector3.one;

        LineRenderer lineRenderer = bodyLineObject.AddComponent<LineRenderer>();
        ConfigureLineRenderer(lineRenderer);

        GameObject arrowHeadObject = new GameObject("ArrowHead");
        arrowHeadObject.transform.SetParent(rootObject.transform);
        arrowHeadObject.transform.localPosition = Vector3.zero;
        arrowHeadObject.transform.localRotation = Quaternion.identity;
        arrowHeadObject.transform.localScale = new Vector3(0.5f, 0.6f, 1f);

        SpriteRenderer spriteRenderer = arrowHeadObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = arrowHeadSprite;
        spriteRenderer.color = viewConfig.arrowColor;

        if (viewConfig.lineMaterial != null)
        {
            spriteRenderer.material = viewConfig.lineMaterial;
        }

        GameObject hitAreaObject = new GameObject("HitArea");
        hitAreaObject.transform.SetParent(rootObject.transform);
        hitAreaObject.transform.localPosition = Vector3.zero;
        hitAreaObject.transform.localRotation = Quaternion.identity;
        hitAreaObject.transform.localScale = Vector3.one;

        arrowRoot.BodyLine = lineRenderer;
        arrowRoot.ArrowHead = arrowHeadObject.transform;
        arrowRoot.HitArea = hitAreaObject.transform;

        return arrowRoot;
    }

    private void ConfigureLineRenderer(LineRenderer lineRenderer)
    {
        lineRenderer.useWorldSpace = true;
        lineRenderer.startWidth = viewConfig.bodyLineWidth;
        lineRenderer.endWidth = viewConfig.bodyLineWidth;
        lineRenderer.startColor = viewConfig.arrowColor;
        lineRenderer.endColor = viewConfig.arrowColor;
        lineRenderer.numCapVertices = 6;
        lineRenderer.numCornerVertices = 6;
        lineRenderer.alignment = LineAlignment.View;

        if (viewConfig.lineMaterial != null)
        {
            lineRenderer.material = viewConfig.lineMaterial;
        }
    }

    private void CreateSegmentHits(ArrowRoot arrowRoot, ArrowData data, LevelData levelData)
    {
        if (arrowRoot.HitArea == null)
        {
            Debug.LogWarning($"{arrowRoot.name} missing HitArea.");
            return;
        }

        for (int i = 0; i < data.path.Count - 1; i++)
        {
            Vector3 start = GridToWorld(data.path[i], levelData);
            Vector3 end = GridToWorld(data.path[i + 1], levelData);
            Vector3 center = (start + end) * 0.5f;
            Vector3 delta = end - start;

            GameObject segmentHit = new GameObject($"SegmentHit_{i}");
            segmentHit.transform.SetParent(arrowRoot.HitArea);
            segmentHit.transform.position = center;
            segmentHit.transform.localScale = Vector3.one;

            if (Mathf.Abs(delta.y) > Mathf.Abs(delta.x))
            {
                segmentHit.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
            }
            else
            {
                segmentHit.transform.rotation = Quaternion.identity;
            }

            BoxCollider2D boxCollider = segmentHit.AddComponent<BoxCollider2D>();
            boxCollider.size = viewConfig.segmentHitSize;
            boxCollider.isTrigger = true;
        }
    }
}