using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace CTR{
	public class ClusterTouch : MonoBehaviour {

		public long pointID;
		public int ClusterId;
		public DbscanPoint dbscanPoint;

		public Vector2 clusterCenter;


		public void SetColor(Color color)
		{
			GetComponent<SpriteRenderer> ().color = color;
		}

		public void Reset ()
		{
			ClusterId = -1;
			GetComponent<SpriteRenderer> ().color = Color.black;
		}
	}
}

