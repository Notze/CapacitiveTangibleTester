using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OSCReceiver : MonoBehaviour{

    public static bool OSCDebug = true;
    public OSC osc;
    private string _tangibleID;
    public string TangibleID { get {
            if (!osc.isActiveAndEnabled)
                Start();
            return _tangibleID;
        } }

    // Use this for initialization
    private void Start () {
		osc.SetAddressHandler ("/ID", onReceiveID);
        if (OSCDebug) print("OSCReceiver set up");
	}

    private void onReceiveID(OscMessage message){
		_tangibleID =  message.GetFloat(0).ToString();
        if (OSCDebug) print("OSC: Tangible ID received");
    }
}
