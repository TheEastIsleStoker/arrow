using System;
using System.Collections.Generic;

[Serializable]
public class LevelData
{
    public int levelId;
    public int boardWidth = 12;
    public int boardHeight = 12;
    public float cellSize = 1f;
    public List<ArrowData> arrows = new List<ArrowData>();
}
