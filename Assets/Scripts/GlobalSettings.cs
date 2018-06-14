using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum InputModality{
    Mouse,
    Touch
}

public class GlobalSettings : SingletonBehaviour<GlobalSettings> {
	public InputModality modality = InputModality.Touch;
	public float clusterRadiusScaler = 2.0f;
	public float anchorTolerance = 0.1f;
	public float patternFitThreshold = 0.05f;
	public int numOfclusterPoints = 0;
	public int minNumOfPointsInCluster = 4;
	public float rotationAngle;
	public Text statusText;
	public bool flipRotation;
	public float minDistanceBetweenTouchPoints = 0.1f;
	private void Start ()
	{
		SetStatusText ();
	}

	private void Update()
    {
        if(Input.GetKeyDown(KeyCode.M)){
            modality++;
            if ((int)modality >= Enum.GetNames(typeof(InputModality)).Length){
                modality = 0;    
            }
            SetStatusText();
        }
    }

    void SetStatusText(){
		statusText.text = string.Format("modality: {0} cluster points: {1}; cluster radius scalar: {2}; tolerance: {3}; angle: {4}; flip: {5}; min dist: {6}; fitness threshold: {7}; screen DPI {8}",
		                                modality, 
		                                numOfclusterPoints, 
		                                clusterRadiusScaler, 
		                                anchorTolerance,
		                                rotationAngle,
		                               	flipRotation,
		                                minDistanceBetweenTouchPoints,
		                                patternFitThreshold,
		                                Screen.dpi);
    }

	public void SetModality(int m){
		modality = (InputModality)m;
		SetStatusText();
	}

	public void SetNumClusterPoints(int numPoints){
		numOfclusterPoints = numPoints;
		SetStatusText ();
	}

	public void SetClusterRadiusScaler(float scaler){
		clusterRadiusScaler = scaler;
		SetStatusText ();
	}

	public void SetAnchorTolerance (float scaler)
	{
		anchorTolerance = scaler;
		SetStatusText ();
	}


	public void SetRotationAngle (float angle)
	{
		rotationAngle = angle;
		SetStatusText ();
	}
	public void FlipRotation(){
		flipRotation = !flipRotation;
		SetStatusText ();
	}
	public void SetMinDistanceBetweenTouchPoints(float dist){
		minDistanceBetweenTouchPoints = dist;
		SetStatusText ();
	}

	public void SetPatternFitThreshold(float threshold){
		patternFitThreshold = threshold;
		SetStatusText();
	}
}
