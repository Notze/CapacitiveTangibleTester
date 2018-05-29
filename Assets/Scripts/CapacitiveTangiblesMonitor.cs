using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;


public class CapacitiveTangiblesMonitor : MonoBehaviour {

    public Text patternID;
    public GameObject TouchPrefab;

	
	
	// Update is called once per frame
	void Update () {
		
	}

    public void LoadPattern(){
        string fullfilepath = TangiblesFileUtils.PatternFilename(patternID.text);
        string json = File.ReadAllText(fullfilepath);
        TangiblePattern pattern = JsonUtility.FromJson<TangiblePattern>(json);

        foreach(Vector2 point in pattern.points){
            GameObject touchGO = Instantiate(TouchPrefab);
            touchGO.transform.SetParent(this.transform);
            (touchGO.transform as RectTransform).localPosition = point;
        }
    }
}
