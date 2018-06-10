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
    public InputModality modality;
	public float clusterRadiusScaler = 1.5f;
	public int numOfclusterPoints = 0;
	public int rotationIdx;
	public float tangibleDistance = float.MaxValue;
	public Text statusText;

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
		statusText.text = string.Format("modality: {0} cluster points: {1}; cluster radius scalar: {2}; rotation idx: {3} distance: {4}",
		                                modality, 
		                                numOfclusterPoints, 
		                                clusterRadiusScaler, 
		                                rotationIdx,
		                                tangibleDistance);
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
	public void SetRotationIndex (int idx)
	{
		rotationIdx = idx;
		SetStatusText ();
	}
	public void SetDistanceSum(float dist){
		tangibleDistance = dist;
		SetStatusText ();
	}

}
