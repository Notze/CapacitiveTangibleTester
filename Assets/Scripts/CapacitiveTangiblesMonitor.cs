using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;


public class CapacitiveTangiblesMonitor : MonoBehaviour {

    public Text patternID;
    public GameObject TouchPrefab;

    RectTransform rectTransform;
	
	// Update is called once per frame
	void Update () {
        this.rectTransform = this.transform as RectTransform;
	}

    public void LoadPattern(){
        string fullfilepath = TangiblesFileUtils.PatternFilename(patternID.text);
        string json = File.ReadAllText(fullfilepath);
        TangiblePattern pattern = JsonUtility.FromJson<TangiblePattern>(json);

        foreach(Vector2 point in pattern.points){
            GameObject touchGO = Instantiate(TouchPrefab);
            touchGO.transform.SetParent(this.transform);
            Vector2 thisPos = new Vector2(rectTransform.position.x - rectTransform.sizeDelta.x * rectTransform.pivot.x/2,
                                          rectTransform.position.y + rectTransform.sizeDelta.y * rectTransform.pivot.y/2);
            (touchGO.transform as RectTransform).position = thisPos + point;
        }
    }
}
