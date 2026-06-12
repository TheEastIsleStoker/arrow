using UnityEngine;

public static class DirectionUtil
{
    public static GridPos ToGridOffset(Direction direction)
    {
        switch (direction)
        {
            case Direction.Up:
                return new GridPos(0, 1);
            case Direction.Right:
                return new GridPos(1, 0);
            case Direction.Down:
                return new GridPos(0, -1);
            case Direction.Left:
                return new GridPos(-1, 0);
            default:
                return new GridPos(0, 0);
        }
    }

    public static Vector2Int ToVector2Int(Direction direction)
    {
        switch (direction)
        {
            case Direction.Up:
                return Vector2Int.up;
            case Direction.Right:
                return Vector2Int.right;
            case Direction.Down:
                return Vector2Int.down;
            case Direction.Left:
                return Vector2Int.left;
            default:
                return Vector2Int.zero;
        }
    }

    public static Vector3 ToWorldDirection(Direction direction)
    {
        switch (direction)
        {
            case Direction.Up:
                return Vector3.up;
            case Direction.Right:
                return Vector3.right;
            case Direction.Down:
                return Vector3.down;
            case Direction.Left:
                return Vector3.left;
            default:
                return Vector3.zero;
        }
    }

    public static Direction FromGridDelta(GridPos from, GridPos to)
    {
        int dx = to.x - from.x;
        int dy = to.y - from.y;

        if (dx == 0 && dy == 1)
        {
            return Direction.Up;
        }

        if (dx == 1 && dy == 0)
        {
            return Direction.Right;
        }

        if (dx == 0 && dy == -1)
        {
            return Direction.Down;
        }

        if (dx == -1 && dy == 0)
        {
            return Direction.Left;
        }

        Debug.LogError($"Invalid direction delta from ({from.x}, {from.y}) to ({to.x}, {to.y}).");
        return Direction.Up;
    }

    public static Direction GetHeadDirection(ArrowData arrowData)
    {
        if (arrowData == null || arrowData.path == null || arrowData.path.Count < 2)
        {
            Debug.LogError("Cannot calculate arrow head direction. Path must contain at least 2 points.");
            return Direction.Up;
        }

        GridPos previous = arrowData.path[arrowData.path.Count - 2];
        GridPos head = arrowData.path[arrowData.path.Count - 1];

        return FromGridDelta(previous, head);
    }

    public static float ToArrowHeadZRotation(Direction direction)
    {
        switch (direction)
        {
            case Direction.Up:
                return 0f;
            case Direction.Left:
                return 90f;
            case Direction.Down:
                return 180f;
            case Direction.Right:
                return -90f;
            default:
                return 0f;
        }
    }

    public static Quaternion ToArrowHeadRotation(Direction direction)
    {
        return Quaternion.Euler(0f, 0f, ToArrowHeadZRotation(direction));
    }
}