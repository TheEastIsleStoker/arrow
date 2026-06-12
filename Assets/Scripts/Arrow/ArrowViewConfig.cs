using System;
using UnityEngine;

[Serializable]
public class ArrowViewConfig
{
    public float bodyLineWidth = 0.2f;
    public float cellSize = 1f;
    public Color arrowColor = Color.black;
    public Material lineMaterial;
    public Vector2 segmentHitSize = new Vector2(1.2f, 0.3f);
}
