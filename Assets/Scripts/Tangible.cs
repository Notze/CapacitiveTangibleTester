using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tangible : MonoBehaviour {

	public TangiblePattern pattern;
	public Text idText;

	public void SetIDText (string id){
		idText.text = id;
	}

	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
