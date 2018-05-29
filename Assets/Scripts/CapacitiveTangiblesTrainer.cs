using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CapacitiveTangiblesTrainer : MonoBehaviour {

    RectTransform rectTransform;
	void Start () {
        rectTransform = transform as RectTransform;
	}
	
	// Update is called once per frame
	void Update () {
        Touch[] touches = Input.touches;
        foreach(Touch touch in touches){
            Vector2 pos = touch.position;
            if(RectTransformUtility.RectangleContainsScreenPoint(rectTransform, pos)){
                GetComponent<Image>().color = Color.red;
            }
        }
        //
	}
}
