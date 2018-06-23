using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


namespace CTR{
	[Serializable]
	public struct TangiblePattern {
		public int id;
		public List<Vector2> points;
		public List<float> standardDeviations;
		public float radius;
		public float meanDistance;
		public float anchorDistance;
		public float gridStep;
		public int anchorPoint1;
		public int anchorPoint2;
		public int infoPoint1;
		public int infoPoint2;
	}

}
