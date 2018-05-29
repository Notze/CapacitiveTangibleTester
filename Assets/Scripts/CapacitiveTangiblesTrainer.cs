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


        Vector2 center = MathHelper.ComputeCenter(patternPoints);
        float radius = 0;
        for (int i = 0; i < patternPoints.Count; i++){
            //Debug.DrawLine(Camera.main.ScreenToWorldPoint(patternPoints[i]), 
                           //Camera.main.ScreenToWorldPoint(patternPoints[i] - center), 
                           //Color.green, 30);

            patternPoints[i] = patternPoints[i] - center;
            float dist = Vector2.Distance(center, patternPoints[i]);
            if(dist > radius){
                radius = dist;
            }
        }

        DrawCircle(center, radius, 50);

        TangiblePattern pattern = new TangiblePattern();
        pattern.id = patternID.text;
        pattern.points = patternPoints;
        pattern.radius = radius;

        string json = JsonUtility.ToJson(pattern, true);
        string fullfilepath = TangiblesFileUtils.PatternFilename(patternID.text);
        print(fullfilepath);
        File.WriteAllText(fullfilepath, json);
    }

    void DrawCircle(Vector2 center, float radius, int segments){
        float angle = 360 / segments;
        List<Vector3> points = new List<Vector3>();
        for (int i = 0; i < (segments + 1); i++)
        {
            float x = center.x + Mathf.Sin(Mathf.Deg2Rad * angle) * radius;
            float y = center.y + Mathf.Cos(Mathf.Deg2Rad * angle) * radius;
            points.Add(( new Vector3(x, y, 0)));

            angle += (360f / segments);
        }
        for (int i = 1; i < points.Count; i++){
            Debug.DrawLine(points[i - 1], points[i], Color.blue, 30);
        }
    }

}
