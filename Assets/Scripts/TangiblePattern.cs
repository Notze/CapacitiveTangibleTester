using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public struct TangiblePattern{

    public string id;
    public List<Vector2> points;
    public float radius;
	public float meanDistance;
	public float anchorDistance;
	public int anchorPoint1;
	public int anchorPoint2;
}
