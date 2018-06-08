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


	//// Update is called once per frame
	//void Update () {
		
	//}

}
