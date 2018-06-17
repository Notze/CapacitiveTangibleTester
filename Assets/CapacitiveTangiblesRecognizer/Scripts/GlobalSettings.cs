﻿using System;
using UnityEngine;
using UnityEngine.UI;

namespace CTR{
	public enum InputModality {
		Mouse,
		Touch
	}

	public class GlobalSettings : SingletonBehaviour<GlobalSettings> {

		public const string kModality = "kModality";
		public const string kClusterRadiusScaler = "kClusterRadiusScaler";
		public const string kAnchorTolerance = "kAnchorTolerance";
		public const string kPatternFitThreshold = "kPatternFitThreshold";
		public const string kMinNumOfPointsInCluster = "kMinNumOfPointsInCluster";
		public const string kFlipRotation = "kFlipRotation";
		public const string kMinDistanceBetweenTouchPoints = "kMinDistanceBetweenTouchPoints";

		public int numOfClusterPoints = 0;
		public float rotationAngle;
		public Text statusText;

		public InputModality modality = InputModality.Touch;
		public float clusterRadiusScaler = 2.0f;
		public float anchorTolerance = 0.1f;
		public float patternFitThreshold = 0.001f;
		public int minNumOfPointsInCluster = 4;
		public bool flipRotation;
		public float minDistanceBetweenTouchPoints = 0.1f;


		private void Start () {

			if(PlayerPrefs.HasKey(kModality)){
				modality = (InputModality)PlayerPrefs.GetInt (kModality);	
			}
			if (PlayerPrefs.HasKey (kClusterRadiusScaler)) {
				clusterRadiusScaler = PlayerPrefs.GetFloat(kClusterRadiusScaler);
			}
			if (PlayerPrefs.HasKey (kAnchorTolerance)) {
				anchorTolerance = PlayerPrefs.GetFloat (kAnchorTolerance);
			}
			if (PlayerPrefs.HasKey (kPatternFitThreshold)) {
				patternFitThreshold = PlayerPrefs.GetFloat (kPatternFitThreshold);
			}
			if (PlayerPrefs.HasKey (kMinNumOfPointsInCluster)) {
				minNumOfPointsInCluster = PlayerPrefs.GetInt(kMinNumOfPointsInCluster);
			}
			if (PlayerPrefs.HasKey (kFlipRotation)) {
				flipRotation = PlayerPrefs.GetInt (kFlipRotation) > 0;
			}
			if (PlayerPrefs.HasKey (kMinDistanceBetweenTouchPoints)) {
				minDistanceBetweenTouchPoints = PlayerPrefs.GetFloat(kMinDistanceBetweenTouchPoints);
			}


			SetStatusText ();
		}

		private void Update ()
		{
			if (Input.GetKeyDown (KeyCode.M)) {
				modality++;
				if ((int)modality >= Enum.GetNames (typeof (InputModality)).Length) {
					modality = 0;
				}
				SetStatusText ();
			}
		}

		void SetStatusText ()
		{
			statusText.text = string.Format ("modality: {0} cluster points: {1}; cluster radius scalar: {2}; tolerance: {3}; angle: {4}; flip: {5}; min dist: {6}; fitness threshold: {7}; screen DPI {8}",
											modality,
											numOfClusterPoints,
											clusterRadiusScaler,
											anchorTolerance,
											rotationAngle,
											   flipRotation,
											minDistanceBetweenTouchPoints,
											patternFitThreshold,
											Screen.dpi);


			PlayerPrefs.SetInt (kModality, (int)modality);
			PlayerPrefs.SetFloat (kClusterRadiusScaler, clusterRadiusScaler);
			PlayerPrefs.SetFloat (kAnchorTolerance, anchorTolerance);
			PlayerPrefs.SetFloat(kPatternFitThreshold, patternFitThreshold);
			PlayerPrefs.SetFloat (kMinNumOfPointsInCluster, minNumOfPointsInCluster);
			PlayerPrefs.SetInt (kFlipRotation, flipRotation == true ? 1: 0);
			PlayerPrefs.SetFloat (kMinDistanceBetweenTouchPoints, minDistanceBetweenTouchPoints);
		}

		public void SetModality (int m)
		{
			modality = (InputModality)m;
			SetStatusText ();
		}

		public void SetNumClusterPoints (int numPoints)
		{
			numOfClusterPoints = numPoints;
			SetStatusText ();
		}

		public void SetClusterRadiusScaler (float scaler)
		{
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
		public void FlipRotation ()
		{
			flipRotation = !flipRotation;
			SetStatusText ();
		}
		public void SetMinDistanceBetweenTouchPoints (float dist)
		{
			minDistanceBetweenTouchPoints = dist;
			SetStatusText ();
		}

		public void SetPatternFitThreshold (float threshold)
		{
			patternFitThreshold = threshold;
			SetStatusText ();
		}
	}
}
