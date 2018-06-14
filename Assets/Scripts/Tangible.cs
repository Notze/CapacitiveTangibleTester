using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tangible : MonoBehaviour {

	public TangiblePattern pattern;
	public Text idText;
	public Vector3 lastKnownPosition;
	public Quaternion lastKnownRotation;
	public Transform anchor1;
	public Transform anchor2;
	public void SetIDText (string id){
		idText.text = id;
	}

	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void UpdatePosition(Vector3 pos, Quaternion rot){
		gameObject.transform.position = pos;
		gameObject.transform.rotation = rot;
	}

	public void SavePosition(){
		lastKnownPosition = gameObject.transform.position;
		lastKnownRotation = gameObject.transform.rotation;
	}

	public void ResetPosition(){
		gameObject.transform.position = lastKnownPosition;
		gameObject.transform.rotation = lastKnownRotation;
	}
}
