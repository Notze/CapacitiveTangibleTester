using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


namespace CTR{
	[Serializable]
	public struct TangiblePattern {
		[NonSerialized]
		public const int numOfPoints = 4; 
		public int id;
		public List<Vector2> points;
		public List<float> standardDeviations;
		public List<float> meanDistances;
		public float radius;
		public float anchorDistance;

		public int anchorPoint1;
		public int anchorPoint2;
		public int infoPoint1;
		public int infoPoint2;

		public override string ToString(){
			string str = string.Empty;
			str += "id: " + id + "\n";
			str += "radius: " + radius.ToString("0.000") + "\n";
			str += "anchor dist: " + anchorDistance.ToString ("0.000") + "\n";
			str += "anchor1 SD: " + standardDeviations[anchorPoint1].ToString ("0.000") + "\n";
			str += "anchor2 SD: " + standardDeviations [anchorPoint2].ToString ("0.000") + "\n";
			str += "info1 SD: " + standardDeviations [infoPoint1].ToString ("0.000") + "\n";
			str += "info2 SD: " + standardDeviations [infoPoint2].ToString ("0.000") + "\n";
			return str;
		}
	}

}
