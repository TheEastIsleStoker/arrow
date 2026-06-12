using System.Collections.Generic;
using UnityEngine;

public class ArrowFactory : MonoBehaviour
{
    [Header("Prefabs")]
    public ArrowRoot arrowRootPrefab;
    public GameObject segmentHitPrefab;

    [Header("Scene")]
    public Transform boardRoot;

    [Header("View")]
    public ArrowViewConfig viewConfig = new ArrowViewConfig();

    public ArrowRoot CreateArrow(ArrowData data, LevelData levelData)
    {
        if (arrowRootPrefab == null)
        {
            Debug.LogError("ArrowFactory missing arrowRootPrefab.");
            return null;
        }

        if (data == null || data.path == null || data.path.Count < 2)
        {
            Debug.LogError("Invalid arrow data. Path must contain at least 2 points.");
            return null;
        }

        Transform parent = boardRoot != null ? boardRoot : transform;
        ArrowRoot arrowRoot = Instantiate(arrowRootPrefab, parent);
        arrowRoot.Initialize(data, viewConfig);

        EnsureArrowReferences(arrowRoot);

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

    private void EnsureArrowReferences(ArrowRoot arrowRoot)
    {
        if (arrowRoot.BodyLine == null)
        {
            arrowRoot.BodyLine = arrowRoot.GetComponentInChildren<LineRenderer>();
        }

        if (arrowRoot.HitArea == null)
        {
            Transform hitArea = arrowRoot.transform.Find("HitArea");

            if (hitArea == null)
            {
                GameObject hitAreaObject = new GameObject("HitArea");
                hitAreaObject.transform.SetParent(arrowRoot.transform);
                hitAreaObject.transform.localPosition = Vector3.zero;
                hitAreaObject.transform.localRotation = Quaternion.identity;
                hitAreaObject.transform.localScale = Vector3.one;
                hitArea = hitAreaObject.transform;
            }

            arrowRoot.HitArea = hitArea;
        }
    }

    private void CreateSegmentHits(ArrowRoot arrowRoot, ArrowData data, LevelData levelData)
    {
        if (segmentHitPrefab == null)
        {
            Debug.LogWarning("ArrowFactory missing segmentHitPrefab. Click detection will not work.");
            return;
        }

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

            GameObject segmentHit = Instantiate(segmentHitPrefab, arrowRoot.HitArea);
            segmentHit.name = $"SegmentHit_{i}";
            segmentHit.transform.position = center;

            if (Mathf.Abs(delta.y) > Mathf.Abs(delta.x))
            {
                segmentHit.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
            }
            else
            {
                segmentHit.transform.rotation = Quaternion.identity;
            }

            BoxCollider2D boxCollider = segmentHit.GetComponent<BoxCollider2D>();

            if (boxCollider != null)
            {
                boxCollider.size = viewConfig.segmentHitSize;
                boxCollider.isTrigger = true;
            }
        }
    }
}