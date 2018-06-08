using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour {

	public Dropdown modalityDropdown;
	bool isOpen;
	public List<RectTransform> menuPanels;
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ToggleMenu(){
		if(isOpen){
			CloseMenu();
		}else{
			OpenMenu ();
		}
		isOpen = !isOpen;
			
	}

	void OpenMenu(){
		foreach(RectTransform rt in menuPanels){
			rt.gameObject.SetActive (true);
		}
	}

	void CloseMenu(){
		foreach (RectTransform rt in menuPanels) {
			rt.gameObject.SetActive (false);
		}
	}

	public void SetModality(){
		GlobalSettings.Instance.SetModality (modalityDropdown.value);
	}

}
