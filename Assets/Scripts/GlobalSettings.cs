using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum InputModality{
    Mouse,
    Touch
}

public class GlobalSettings : SingletonBehaviour<GlobalSettings> {
    public InputModality modality;

}
