using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum InputModality{
    Mouse,
    Touch
}

public class GlobalSettings : SingletonBehaviour<GlobalSettings> {
    public InputModality modality;


    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.M)){
            modality++;
            if ((int)modality >= Enum.GetNames(typeof(InputModality)).Length){
                modality = 0;    
            }

        }
    }

}
