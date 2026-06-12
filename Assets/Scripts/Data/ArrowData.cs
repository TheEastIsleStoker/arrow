using System;
using System.Collections.Generic;

[Serializable]
public class ArrowData
{
    public int id;
    public List<GridPos> path = new List<GridPos>();
}