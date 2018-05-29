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


        for (int i = 0; i < rectTransform.childCount; i++){
            Transform child = rectTransform.GetChild(i);
            if (child.CompareTag("Touch")){
                Destroy(child.gameObject);
            }
        }

        string fullfilepath = TangiblesFileUtils.PatternFilename(patternID.text);
        string json = File.ReadAllText(fullfilepath);
        TangiblePattern pattern = JsonUtility.FromJson<TangiblePattern>(json);

        Vector2 center = MathHelper.ComputeCenter(pattern.points);
        MathHelper.DrawCircle(center, pattern.radius, 50);

        foreach(Vector2 point in pattern.points){
            GameObject touchGO = Instantiate(TouchPrefab);
            touchGO.transform.SetParent(this.transform);
            Vector2 thisPos = new Vector2(rectTransform.position.x - rectTransform.sizeDelta.x/2,
                                          rectTransform.position.y + rectTransform.sizeDelta.y/2);
            (touchGO.transform as RectTransform).position = thisPos + point;
        }
    }
}
