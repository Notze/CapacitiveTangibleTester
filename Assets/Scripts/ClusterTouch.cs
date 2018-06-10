using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClusterTouch : MonoBehaviour {

	public long pointID;
	public int ClusterId;
	public DbscanPoint dbscanPoint;

	void Start () {
		
	}


	public void SetClusterColor(Color color){
		GetComponent<SpriteRenderer> ().color = color;
	}

	public void Reset () {
		ClusterId = -1;
		GetComponent<SpriteRenderer> ().color = Color.black;
	}

	//// Update is called once per frame
	//void Update () {

	//}

}
