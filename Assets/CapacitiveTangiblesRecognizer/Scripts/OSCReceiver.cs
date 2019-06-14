using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OSCReceiver : MonoBehaviour {

	public OSC osc;
	public Text debugText;
	
	// Use this for initialization
	void Start () {
		osc.SetAddressHandler ("/ID", onReceiveID);
		debugText.text = "OSC-Receiver has been set up!";
	}
	
	// Update is called once per frame
	void Update () {
		//debugText.text += "waiting for signal from tangible";
	}

	void onReceiveID(OscMessage message){
		float tangID = message.GetFloat(0);
		debugText.text += "\ntouched tangible ID: " + tangID.ToString();
	}
}
