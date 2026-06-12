using System.Collections.Generic;
using UnityEngine;

public class BoardModel
{
    private int boardWidth;
    private int boardHeight;
    private int[,] occupancy;
    private readonly Dictionary<int, ArrowData> arrowDataMap = new Dictionary<int, ArrowData>();

    public void Initialize(LevelData levelData)
    {
        Clear();

        boardWidth = levelData.boardWidth;
        boardHeight = levelData.boardHeight;
        occupancy = new int[boardWidth, boardHeight];

        for (int x = 0; x < boardWidth; x++)
        {
            for (int y = 0; y < boardHeight; y++)
            {
                occupancy[x, y] = -1;
            }
        }
    }

    public void RegisterAllArrows(List<ArrowData> arrows)
    {
        foreach (ArrowData arrowData in arrows)
        {
            RegisterArrow(arrowData);
        }
    }

    public void RegisterArrow(ArrowData arrowData)
    {
        if (arrowData == null || arrowData.path == null)
        {
            return;
        }

        arrowDataMap[arrowData.id] = arrowData;

        foreach (GridPos point in arrowData.path)
        {
            if (!IsInsideBoard(point))
            {
                Debug.LogWarning($"Arrow {arrowData.id} has out-of-board point ({point.x}, {point.y}).");
                continue;
            }

            if (occupancy[point.x, point.y] != -1)
            {
                Debug.LogWarning($"Grid ({point.x}, {point.y}) is already occupied by arrow {occupancy[point.x, point.y]}.");
            }

            occupancy[point.x, point.y] = arrowData.id;
        }
    }

    public bool CanFlyOut(ArrowData arrowData)
    {
        if (arrowData == null || arrowData.path == null || arrowData.path.Count < 2)
        {
            return false;
        }

        GridPos head = arrowData.path[arrowData.path.Count - 1];
        Direction headDirection = DirectionUtil.GetHeadDirection(arrowData);
        GridPos offset = DirectionUtil.ToGridOffset(headDirection);

        int x = head.x + offset.x;
        int y = head.y + offset.y;

        while (IsInsideBoard(x, y))
        {
            int occupiedId = occupancy[x, y];

            if (occupiedId != -1 && occupiedId != arrowData.id)
            {
                return false;
            }

            x += offset.x;
            y += offset.y;
        }

        return true;
    }

    public void RemoveArrow(ArrowData arrowData)
    {
        if (arrowData == null || arrowData.path == null)
        {
            return;
        }

        foreach (GridPos point in arrowData.path)
        {
            if (!IsInsideBoard(point))
            {
                continue;
            }

            if (occupancy[point.x, point.y] == arrowData.id)
            {
                occupancy[point.x, point.y] = -1;
            }
        }

        arrowDataMap.Remove(arrowData.id);
    }

    public bool IsInsideBoard(GridPos pos)
    {
        return IsInsideBoard(pos.x, pos.y);
    }

    public bool IsInsideBoard(int x, int y)
    {
        return x >= 0 && x < boardWidth && y >= 0 && y < boardHeight;
    }

    public void Clear()
    {
        boardWidth = 0;
        boardHeight = 0;
        occupancy = null;
        arrowDataMap.Clear();
    }
}