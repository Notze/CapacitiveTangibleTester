using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuButton : MonoBehaviour {

	public void changeScene (string scene){
		Application.LoadLevel (scene);
	}

	public void changeScene (UnityEngine.UI.Dropdown list){
		Application.LoadLevel (list.options[list.value].text);
	}
}
