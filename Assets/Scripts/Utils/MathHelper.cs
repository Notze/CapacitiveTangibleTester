using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MathHelper{

    public static Vector2 ComputeCenter(List<Vector2> points)
    {
        Vector2 center = Vector2.zero;
        foreach (Vector2 p in points)
        {
            center += p;
        }
        center /= points.Count;

        foreach (Vector2 p in points)
        {
            Debug.DrawLine(center, p, Color.red, 30);
        }

        return center;
    }
}
