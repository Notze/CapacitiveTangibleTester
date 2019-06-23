using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OSCReceiver {

    public static bool OSCDebug = true;
    public static OSC osc;
    private static string _tangibleID;
	public string TangibleID { get {
            if (!osc.isActiveAndEnabled)
                Start();
            return _tangibleID;
        } }

    public OSCReceiver(OSC o)
    {
        osc = o;
    }

	// Use this for initialization
	private static void Start () {
		osc.SetAddressHandler ("/ID", onReceiveID);
        //if (OSCDebug) print("OSCReceiver initialized");
	}

	private static void onReceiveID(OscMessage message){
		_tangibleID =  message.GetFloat(0).ToString();
	}
}
