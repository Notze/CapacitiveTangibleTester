using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tangible : MonoBehaviour {

	public TangiblePattern pattern;
	public Text idText;
	public Vector3 lastKnownPosition;
	//public Quaternion lastKnownRotation;

	public void SetIDText (string id){
		idText.text = id;
	}

	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void UpdatePosition(Vector3 pos){
		lastKnownPosition = gameObject.transform.position;
		gameObject.transform.position = pos;
	}


	public void ResetPosition(){
		gameObject.transform.position = lastKnownPosition;
	}

	//public void UpdateRotation(Quaternion rot){
	//	lastKnownRotation = gameObject.transform.rotation;
	//	gameObject.transform.rotation = rot;
	//}

}
