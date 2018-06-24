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
		public int trainingSamples;
		public int anchorPoint1;
		public int anchorPoint2;
		public int infoPoint1;
		public int infoPoint2;

		public override string ToString(){
			string str = string.Empty;
			str += "id: " + id + "\n";
			str += "training samples: " + trainingSamples + "\n";
			str += "radius: " + radius.ToString ("0.000") + "\n";
			str += "anchor dist: " + anchorDistance.ToString ("0.000") + "\n";
			str += "anchor1 mean:" + meanDistances[anchorPoint1].ToString("0.000") + " SD: " + standardDeviations[anchorPoint1].ToString ("0.000") + "\n";
			str += "anchor2 mean:" + meanDistances[anchorPoint2].ToString("0.000") + " SD: " + standardDeviations [anchorPoint2].ToString ("0.000") + "\n";
			str += "info1 mean:" + meanDistances[infoPoint1].ToString("0.000") + " SD: " + standardDeviations [infoPoint1].ToString ("0.000") + "\n";
			str += "info2 mean:" + meanDistances [infoPoint2].ToString("0.000") + " SD: " + standardDeviations [infoPoint2].ToString ("0.000") + "\n";
			return str;
		}
	}

}
