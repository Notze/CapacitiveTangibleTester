using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum InputModality{
    Mouse,
    Touch
}

public class GlobalSettings : SingletonBehaviour<GlobalSettings> {
    public InputModality modality;
    public Text statusText;

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.M)){
            modality++;
            if ((int)modality >= Enum.GetNames(typeof(InputModality)).Length){
                modality = 0;    
            }
            SetStatusText();
        }
    }

    void SetStatusText(){
        statusText.text = string.Format("modality: {0}", modality);
    }

	public void SetModality(int m){
		modality = (InputModality)m;
		SetStatusText();
	}

}
