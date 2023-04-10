using Godot;
using System;
using System.Collections.Generic;

public class OutlineBG : Node2D
{
    public List<Vector2[]> points;
    public Color[] colors = new Color[] { new Color(0, 0, 0) };

    public override void _Ready()
    {
        points = new List<Vector2[]>();
    }
    public override void _Draw() {
        foreach(Vector2[] pointArray in points) {
            DrawPolygon(pointArray, colors);
        }
    }
}
