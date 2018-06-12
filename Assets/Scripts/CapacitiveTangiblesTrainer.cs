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

        if(Input.GetMouseButtonDown(0)){
            Vector2 pos = Input.mousePosition;
            if (RectTransformUtility.RectangleContainsScreenPoint(rectTransform, pos))
            {
                patternPoints.Add(Camera.main.ScreenToWorldPoint(pos));
            }
        }

        foreach (Touch touch in touches)
        {
            Vector2 pos = touch.position;
            if (RectTransformUtility.RectangleContainsScreenPoint(rectTransform, pos))
            {
                //patternPoints.Add(pos);
                patternPoints.Add (Camera.main.ScreenToWorldPoint (pos));
            }
        }


        Vector2 center = MathHelper.ComputeCenter(patternPoints, Color.red);
        float radius = 0;
		float meanDistance = 0;
        for (int i = 0; i < patternPoints.Count; i++){
            Debug.DrawLine(patternPoints[i],
                           patternPoints[i] - center, 
                           Color.green, 30);
            

            // compute radius
            float dist = Vector2.Distance(center, patternPoints[i]);
            MathHelper.DrawCircle (center, dist, 50, Color.red);
			meanDistance += dist;
            if(dist > radius){
                radius = dist;
            }

            // move point relative to center
            patternPoints [i] = patternPoints [i] - center;
        }
		meanDistance /= patternPoints.Count;
        MathHelper.DrawCircle (center, radius, 50, Color.blue);

		Tuple<int, int> minDistancePair = new Tuple<int, int>();
		float minDist = float.MaxValue;
		for (int i = 0; i < patternPoints.Count; i++){
			for (int j = 0; j < patternPoints.Count; j++){
				if(i != j){
					float dist = Vector2.Distance (patternPoints [i], patternPoints [j]);
					if(dist < minDist){
						minDist = dist;
						minDistancePair.first = i;
						minDistancePair.second = j;
					}
				}
			}
		}
		float firstDistFromCenter = Vector2.Distance (patternPoints [minDistancePair.first], center);
		float secondDistFromCenter = Vector2.Distance (patternPoints [minDistancePair.second], center);

		if(secondDistFromCenter > firstDistFromCenter){
			int tmp = minDistancePair.first;
			minDistancePair.first = minDistancePair.second;
			minDistancePair.second = tmp;
		}


        TangiblePattern pattern = new TangiblePattern();
        
		pattern.id = patternID.text;
        pattern.points = patternPoints;
        pattern.radius = radius;
		pattern.meanDistance = meanDistance;
		pattern.anchorDistance = minDist;
		pattern.gridStep = minDist / 2;
		pattern.anchorPoint1 = minDistancePair.first;
		pattern.anchorPoint2 = minDistancePair.second;

        string json = JsonUtility.ToJson(pattern, true);
        string fullfilepath = TangiblesFileUtils.PatternFilename(patternID.text);
        print(fullfilepath);
        File.WriteAllText(fullfilepath, json);
    }



}
