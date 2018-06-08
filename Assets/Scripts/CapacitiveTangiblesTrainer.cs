﻿using System.Collections;
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
        for (int i = 0; i < patternPoints.Count; i++){
            Debug.DrawLine(patternPoints[i],
                           patternPoints[i] - center, 
                           Color.green, 30);
            

            patternPoints[i] = patternPoints[i] - center;
            float dist = Vector2.Distance(center, patternPoints[i]);
            if(dist > radius){
                radius = dist;
            }
        }

        MathHelper.DrawCircle(center, radius, 50);

        TangiblePattern pattern = new TangiblePattern();
        pattern.id = patternID.text;
        pattern.points = patternPoints;
        pattern.radius = radius;

        string json = JsonUtility.ToJson(pattern, true);
        string fullfilepath = TangiblesFileUtils.PatternFilename(patternID.text);
        print(fullfilepath);
        File.WriteAllText(fullfilepath, json);
    }



}
