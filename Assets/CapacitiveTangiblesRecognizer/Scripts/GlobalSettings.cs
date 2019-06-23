﻿//using System;
//using UnityEngine;
//using UnityEngine.UI;

//namespace CTR{
//	public enum InputModality {
//		Mouse,
//		Touch
//	}

//	public class GlobalSettings : SingletonBehaviour<GlobalSettings> {

//		public const string kModality = "kModality";
//		public const string kClusterRadiusScaler = "kClusterRadiusScaler";
//		public const string kAnchorTolerance = "kAnchorTolerance";
//		public const string kPatternFitThreshold = "kPatternFitThreshold";
//		//public const string kMinNumOfPointsInCluster = "kMinNumOfPointsInCluster";
//		public const string kFlipRotation = "kFlipRotation";
//		public const string kMinDistanceBetweenTouchPoints = "kMinDistanceBetweenTouchPoints";
//		public const string kAnchorWeight = "kAnchorWeight";
//		public const string kPositionWeight = "kPositionWeight";
//		public const string kDebugOutput = "kDebugOutput";

//		public int numOfClusterPoints = 0;
//		public float rotationAngle;
//		public Text statusText;

//		public InputModality modality = InputModality.Touch;
//		public float clusterRadiusScaler = 2.0f;
//		public float anchorTolerance = 0.1f;
//		public float patternFitThreshold = 0.001f;
//		public int minNumOfPointsInCluster = 3;
//		public bool flipRotation;
//		public float minDistanceBetweenTouchPoints = 0.1f;
//		public float anchorWeight = 0.1f;
//		public float positionWeight = 0.3f;
//		public bool debugOutput;

//		private void Start () {

//			if(PlayerPrefs.HasKey(kModality)){
//				modality = (InputModality)PlayerPrefs.GetInt (kModality);	
//			}
//			if (PlayerPrefs.HasKey (kClusterRadiusScaler)) {
//				clusterRadiusScaler = PlayerPrefs.GetFloat(kClusterRadiusScaler);
//			}
//			if (PlayerPrefs.HasKey (kAnchorTolerance)) {
//				anchorTolerance = PlayerPrefs.GetFloat (kAnchorTolerance);
//			}
//			if (PlayerPrefs.HasKey (kPatternFitThreshold)) {
//				patternFitThreshold = PlayerPrefs.GetFloat (kPatternFitThreshold);
//			}
//			//if (PlayerPrefs.HasKey (kMinNumOfPointsInCluster)) {
//			//	minNumOfPointsInCluster = PlayerPrefs.GetInt(kMinNumOfPointsInCluster);
//			//}
//			if (PlayerPrefs.HasKey (kFlipRotation)) {
//				flipRotation = PlayerPrefs.GetInt (kFlipRotation) > 0;
//			}
//			if (PlayerPrefs.HasKey (kMinDistanceBetweenTouchPoints)) {
//				minDistanceBetweenTouchPoints = PlayerPrefs.GetFloat(kMinDistanceBetweenTouchPoints);
//			}
//			if (PlayerPrefs.HasKey (kAnchorWeight)) {
//				anchorWeight = PlayerPrefs.GetFloat (kAnchorWeight);
//			}
//			if (PlayerPrefs.HasKey (kPositionWeight)) {
//				positionWeight = PlayerPrefs.GetFloat (kPositionWeight);
//			}
//			if (PlayerPrefs.HasKey (kDebugOutput)) {
//				debugOutput = PlayerPrefs.GetInt (kDebugOutput) > 0;
//			}


//			SetStatusText ();
//		}

//		private void Update ()
//		{

//			if (Input.GetKeyDown (KeyCode.C)) {
//				CapacitiveTangiblesRecognizer.Instance.DoClustering ();
//				//print(clusterPointsDict);
//			}

//			if (Input.GetKeyDown (KeyCode.M)) {
//				modality++;
//				if ((int)modality >= Enum.GetNames (typeof (InputModality)).Length) {
//					modality = 0;
//				}
//				SetStatusText ();
//			}
//			if (Input.GetKeyDown (KeyCode.N)) {
//				float m = 2f;
//				float s = 0.4f;
//				Vector3 lastPos = new Vector3 (-2*m, 0, 0);
//				for (float x = -2*m; x < 2*m; x += 0.1f){
//					float y = MathHelper.NormalDistribution(x, m, s);
//					Vector3 nextPos = new Vector3 (x-m, y, 0);
//					Debug.DrawLine (lastPos, nextPos, Color.yellow, 100);
//					lastPos = nextPos;
//				}
//			}

//			if (Input.GetKeyDown (KeyCode.R)) {
//				CapacitiveTangiblesRecognizer.Instance.ResetClusters ();
//			}
//			if (Input.GetKeyDown (KeyCode.T)) {
//				CapacitiveTangiblesTrainer.Instance.TrainPattern();
//			}
//			if (Input.GetKeyDown (KeyCode.X)) {
//				CapacitiveTangiblesRecognizer.Instance.ClearClusters ();
//			}
//			if (Input.GetKeyDown (KeyCode.Z)) {
//				CapacitiveTangiblesRecognizer.Instance.RecognizeTangibles ();
//			}
//		}

//		void SetStatusText ()
//		{
//			statusText.text = string.Format ("cluster points: {0};  anchor tolerance: {1}; min dist move: {2}; fitness threshold: {3}; anchor weight: {4}; position weight: {5}",
//			                                 numOfClusterPoints,
//											 anchorTolerance,
//			                                 minDistanceBetweenTouchPoints.ToString("0.000"),
//			                                 patternFitThreshold.ToString("0.000"),
//			                                 anchorWeight.ToString("0.000"), 
//			                                 positionWeight.ToString("0.000"));


//			PlayerPrefs.SetInt (kModality, (int)modality);
//			PlayerPrefs.SetFloat (kClusterRadiusScaler, clusterRadiusScaler);
//			PlayerPrefs.SetFloat (kAnchorTolerance, anchorTolerance);
//			PlayerPrefs.SetFloat(kPatternFitThreshold, patternFitThreshold);
//			PlayerPrefs.SetInt (kFlipRotation, flipRotation == true ? 1: 0);
//			PlayerPrefs.SetFloat (kMinDistanceBetweenTouchPoints, minDistanceBetweenTouchPoints);
//			PlayerPrefs.SetFloat (kAnchorWeight, anchorWeight);
//			PlayerPrefs.SetFloat (kPositionWeight, positionWeight);
//			PlayerPrefs.SetInt (kDebugOutput, debugOutput == true ? 1 : 0);
//		}

//		public void SetModality (int m)
//		{
//			modality = (InputModality)m;
//			SetStatusText ();
//		}

//		public void SetNumClusterPoints (int numPoints)
//		{
//			numOfClusterPoints = numPoints;
//			SetStatusText ();
//		}

//		public void SetClusterRadiusScaler (float scaler)
//		{
//			clusterRadiusScaler = scaler;
//			SetStatusText ();
//		}

//		public void SetAnchorTolerance (float scaler)
//		{
//			anchorTolerance = scaler;
//			SetStatusText ();
//		}


//		public void SetRotationAngle (float angle)
//		{
//			rotationAngle = angle;
//			SetStatusText ();
//		}
//		public void ToggleFlipRotation()
//		{
//			flipRotation = !flipRotation;
//			SetStatusText();
//		}
//		public void SetMinDistanceBetweenTouchPoints (float dist)
//		{
//			minDistanceBetweenTouchPoints = dist;
//			SetStatusText ();
//		}

//		public void SetPatternFitThreshold (float threshold)
//		{
//			patternFitThreshold = threshold;
//			SetStatusText ();
//		}

//		public void ToggleDebugOutput(){
//			debugOutput = !debugOutput;
//			SetStatusText();
//		}

//		public void SetAnchorWeight(float weight){
//			anchorWeight = weight;
//			SetStatusText();
//		}

//		public void SetPositionWeight (float weight)
//		{
//			positionWeight = weight;
//			SetStatusText ();
//		}
//	}
//}

