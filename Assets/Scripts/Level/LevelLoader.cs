using System.Collections.Generic;
using UnityEngine;

public class LevelLoader : MonoBehaviour
{
    [System.Serializable]
    public class LevelAssetEntry
    {
        public int levelId;
        public TextAsset levelJson;
    }

    [Header("Level Assets")]
    public List<LevelAssetEntry> levelAssets = new List<LevelAssetEntry>();

    public LevelData LoadLevel(int levelId)
    {
        TextAsset levelJson = FindLevelJson(levelId);

        if (levelJson == null)
        {
            Debug.LogError($"Level json not found. levelId = {levelId}");
            return null;
        }

        LevelData levelData = JsonUtility.FromJson<LevelData>(levelJson.text);

        if (levelData == null)
        {
            Debug.LogError($"Failed to parse level json. levelId = {levelId}");
            return null;
        }

        return levelData;
    }

    private TextAsset FindLevelJson(int levelId)
    {
        if (levelAssets == null)
        {
            return null;
        }

        foreach (LevelAssetEntry entry in levelAssets)
        {
            if (entry == null)
            {
                continue;
            }

            if (entry.levelId == levelId)
            {
                return entry.levelJson;
            }
        }

        return null;
    }

    public bool ValidateLevel(LevelData levelData)
    {
        if (levelData == null)
        {
            Debug.LogError("LevelData is null.");
            return false;
        }

        if (levelData.boardWidth <= 0 || levelData.boardHeight <= 0)
        {
            Debug.LogError("Board size is invalid.");
            return false;
        }

        if (levelData.cellSize <= 0f)
        {
            Debug.LogError("Cell size is invalid.");
            return false;
        }

        if (levelData.arrows == null || levelData.arrows.Count == 0)
        {
            Debug.LogError("LevelData has no arrows.");
            return false;
        }

        HashSet<Vector2Int> occupiedCells = new HashSet<Vector2Int>();
        HashSet<int> arrowIds = new HashSet<int>();

        foreach (ArrowData arrow in levelData.arrows)
        {
            if (arrow == null)
            {
                Debug.LogError("Level contains a null arrow.");
                return false;
            }

            if (arrowIds.Contains(arrow.id))
            {
                Debug.LogError($"Duplicate arrow id: {arrow.id}");
                return false;
            }

            arrowIds.Add(arrow.id);

            if (arrow.path == null || arrow.path.Count < 2)
            {
                Debug.LogError($"Arrow {arrow.id} path must contain at least 2 points.");
                return false;
            }

            if (!ValidateArrowPath(levelData, arrow, occupiedCells))
            {
                return false;
            }
        }

        return true;
    }

    private bool ValidateArrowPath(LevelData levelData, ArrowData arrow, HashSet<Vector2Int> occupiedCells)
    {
        HashSet<Vector2Int> currentArrowCells = new HashSet<Vector2Int>();

        for (int i = 0; i < arrow.path.Count; i++)
        {
            GridPos point = arrow.path[i];
            Vector2Int pointKey = point.ToVector2Int();

            if (!IsInsideBoard(levelData, point))
            {
                Debug.LogError($"Arrow {arrow.id} has out-of-board point ({point.x}, {point.y}).");
                return false;
            }

            if (currentArrowCells.Contains(pointKey))
            {
                Debug.LogError($"Arrow {arrow.id} has duplicate point ({point.x}, {point.y}).");
                return false;
            }

            currentArrowCells.Add(pointKey);

            if (occupiedCells.Contains(pointKey))
            {
                Debug.LogError($"Grid ({point.x}, {point.y}) is occupied by multiple arrows.");
                return false;
            }

            occupiedCells.Add(pointKey);

            if (i < arrow.path.Count - 1)
            {
                GridPos nextPoint = arrow.path[i + 1];

                if (!AreAdjacent(point, nextPoint))
                {
                    Debug.LogError(
                        $"Arrow {arrow.id} has invalid segment from " +
                        $"({point.x}, {point.y}) to ({nextPoint.x}, {nextPoint.y})."
                    );

                    return false;
                }
            }
        }

        if (!HasValidHeadDirection(arrow))
        {
            Debug.LogError($"Arrow {arrow.id} has invalid head direction.");
            return false;
        }

        return true;
    }

    private bool IsInsideBoard(LevelData levelData, GridPos point)
    {
        return point.x >= 0 &&
               point.x < levelData.boardWidth &&
               point.y >= 0 &&
               point.y < levelData.boardHeight;
    }

    private bool AreAdjacent(GridPos a, GridPos b)
    {
        int distance = Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
        return distance == 1;
    }

    private bool HasValidHeadDirection(ArrowData arrow)
    {
        if (arrow.path == null || arrow.path.Count < 2)
        {
            return false;
        }

        GridPos previous = arrow.path[arrow.path.Count - 2];
        GridPos head = arrow.path[arrow.path.Count - 1];

        int dx = head.x - previous.x;
        int dy = head.y - previous.y;

        return Mathf.Abs(dx) + Mathf.Abs(dy) == 1;
    }
}