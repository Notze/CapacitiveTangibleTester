using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class CapacitiveTangiblesTrainer : MonoBehaviour {


    public Text patternID;
    RectTransform rectTransform;
	void Start () {
        rectTransform = transform as RectTransform;
	}
	
	// Update is called once per frame
	void Update () {
        
	}


    public void SavePattern() {
        List<Vector2> patternPoints = new List<Vector2>();
        Touch[] touches = Input.touches;
        foreach (Touch touch in touches)
        {
            Vector2 pos = touch.position;
            if (RectTransformUtility.RectangleContainsScreenPoint(rectTransform, pos))
            {
                patternPoints.Add(pos);
            }
        }


        Vector2 center = ComputeCenter(patternPoints);
        for (int i = 0; i < patternPoints.Count; i++){
            patternPoints[i] = patternPoints[i] - center;
        }

        TangiblePattern pattern = new TangiblePattern();
        pattern.id = patternID.text;


        pattern.points = patternPoints;


        string json = JsonUtility.ToJson(pattern, true);
        string fullfilepath = TangiblesFileUtils.PatternFilename(patternID.text);
        print(fullfilepath);
        File.WriteAllText(fullfilepath, json);
    }

    Vector2 ComputeCenter(List<Vector2> points){
        Vector2 center = Vector2.zero;
        foreach(Vector2 p in points){
            center += p;
        }
        center /= points.Count;
        return center;
    }

}
