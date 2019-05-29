using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuButton : MonoBehaviour {

	public void changeScene (string scene){
		//Application.LoadLevel (scene); // deprecated
        UnityEngine.SceneManagement.SceneManager.LoadScene(scene);
    }

	public void changeScene (UnityEngine.UI.Dropdown list){
		Application.LoadLevel (list.options[list.value].text);
	}

    public void closeApplication()
    {
        Application.Quit();
    }
}
