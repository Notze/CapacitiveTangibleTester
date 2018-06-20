using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

namespace CTR {
	public struct ClusterAssociation{
		public Vector3 position;
		public Vector3 rotation;
		public float distance;
		public int clusterId;
		public TangiblePattern pattern;
	}


	public class CapacitiveTangiblesRecognizer : SingletonBehaviour<CapacitiveTangiblesRecognizer> {

		public Action<List<TangiblePattern>> OnPatternsLoaded;

		public static long pointID;
		public RectTransform recognizerPanel;

		public bool patternsLoaded;
		public GameObject patternPrefab;
		public GameObject patternFootPrefab;
		public GameObject touchPrefab;
		public List<TangiblePattern> patterns;
		public List<DbscanPoint> dbscanPoints = new List<DbscanPoint> ();

		public List<Color> clusterColors = new List<Color> ();
		public float clusterRadius;

		Dictionary<int, List<DbscanPoint>> clusterPointsDict = new Dictionary<int, List<DbscanPoint>> ();
		public List<Tangible> tangibles;
		public List<ClusterTouch> touchObjects = new List<ClusterTouch> ();

		public List<RectTransform> avoidRecognitionAreas;

		Dictionary<TangiblePattern, List<ClusterAssociation>> patternFitnessDict = new Dictionary<TangiblePattern, List<ClusterAssociation>> ();


		public event Action<int, Vector3, Quaternion> OnTangibleUpdated;


		int clusterCount;
		public int ClusterCount {
			get {
				return clusterCount;
			}
		}

		void OnEnable ()
		{

		}

		void Start ()
		{
			GameObject [] avoidGOs = GameObject.FindGameObjectsWithTag ("Avoid");
			avoidRecognitionAreas = new List<RectTransform> ();
			foreach (GameObject avoidGO in avoidGOs) {
				avoidRecognitionAreas.Add (avoidGO.transform as RectTransform);
			}

			LoadTangiblesPatterns ();
		}

		void HandleMouseInput ()
		{
			if (Input.GetMouseButtonDown (0)) {
				Vector2 pos = Input.mousePosition;
				bool registered = RegisterInputPoint (pos);
			}

		}

		void HandleTouchInput ()
		{
			if (Input.touchCount > 0) {
				foreach (Touch touch in Input.touches) {
					if (touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Moved) {
						RegisterInputPoint (touch.position);
					}
				}
			}
		}

		bool RegisterInputPoint (Vector2 screenPoint)
		{
			bool inAvoidArea = false;
			bool inAcceptableDistance = true;
			foreach (RectTransform avoidArea in avoidRecognitionAreas) {
				if (avoidArea.gameObject.activeSelf) {
					if (RectTransformUtility.RectangleContainsScreenPoint (avoidArea, screenPoint)) {
						inAvoidArea = true;
					}
				}
			}
			if (!inAvoidArea) {
				for (int i = 0; i < touchObjects.Count; i++) {
					if (Vector2.Distance (touchObjects [i].transform.position, screenPoint) < GlobalSettings.Instance.minDistanceBetweenTouchPoints) {
						inAcceptableDistance = false;
						break;
					}
				}

				DbscanPoint dbscanPoint = new DbscanPoint (screenPoint, pointID++);
				dbscanPoints.Add (dbscanPoint);
				GameObject touchObj = Instantiate (touchPrefab);
				Vector3 wPos = Camera.main.ScreenToWorldPoint (screenPoint);
				wPos.z = 0;
				touchObj.transform.position = wPos;

				ClusterTouch clusterTouch = touchObj.GetComponent<ClusterTouch> ();
				clusterTouch.pointID = dbscanPoint.pointID;
				clusterTouch.dbscanPoint = dbscanPoint;
				dbscanPoint.clusterTouch = clusterTouch;
				touchObjects.Add (clusterTouch);
			}
			return !inAvoidArea && inAcceptableDistance;
		}

		//int rotationPoint = 0;
		void Update ()
		{

			// validate old tangible position
			foreach (Tangible tangible in tangibles) {
				tangible.SavePosition ();
			}

			// handle input. prefilter it
			List<Vector2> touchPoints = new List<Vector2> ();
			switch (GlobalSettings.Instance.modality) {
			case InputModality.Mouse:
				HandleMouseInput ();
				break;
			case InputModality.Touch:
				HandleTouchInput ();
				break;
			}

			// do the recognition
			//if (touchPoints.Count >= GlobalSettings.Instance.minNumOfPointsInCluster) {
				//print ("rotationPoint:" + rotationPoint);
				//RecognizeTangibles (patterns, touchPoints);

				// move tangibles to the new position
				//AssignTangiblesPositions ();
			//}




			if (Input.GetKeyDown (KeyCode.C)) {
				DoClustering (clusterRadius * GlobalSettings.Instance.clusterRadiusScaler, GlobalSettings.Instance.minNumOfPointsInCluster);
				//print(clusterPointsDict);
			}

			if (Input.GetKeyDown (KeyCode.R)) {
				ResetClusters ();
			}
			if (Input.GetKeyDown (KeyCode.T)) {
				RecognizeTangibles(patterns);
			}

			if (Input.GetKeyDown (KeyCode.X)) {
				ClearClusters ();
			}
		}

		public void DeleteTangiblesPatterns ()
		{
			TangiblesFileUtils.DeleteTangibles ();
		}

		public void LoadTangiblesPatterns ()
		{
			if (tangibles != null) {
				foreach (Tangible t in tangibles) {
					Destroy (t.gameObject);
				}
			}

			patterns = new List<TangiblePattern> ();
			tangibles = new List<Tangible> ();
			string [] filenames = TangiblesFileUtils.LoadTangiblesJSON ();
			if(filenames != null){
				foreach (string filename in filenames) {
					string json = System.IO.File.ReadAllText (filename);
					TangiblePattern pattern = JsonUtility.FromJson<TangiblePattern> (json);
					patterns.Add (pattern);
					GameObject patternObj = Instantiate (patternPrefab);
					Tangible tangible = patternObj.GetComponent<Tangible> ();
					tangible.SetIDText (pattern.id.ToString());
					tangible.pattern = pattern;
					Vector2 center = MathHelper.ComputeCenter (pattern.points, Color.green);
					RectTransform patternRectTransform = patternObj.transform as RectTransform;
					patternRectTransform.SetParent (recognizerPanel);
					patternRectTransform.sizeDelta = new Vector2 (2*pattern.radius, 2*pattern.radius);
					patternRectTransform.localPosition = new Vector3(0, 0, 0);


					for (int i = 0; i < pattern.points.Count; i++) {
						Vector2 point = pattern.points [i];

						GameObject footObj = Instantiate (patternFootPrefab);
						RectTransform footRectTransform = footObj.transform as RectTransform;
						Vector3 pos = patternObj.transform.position + new Vector3 (point.x, point.y, 0);
						footRectTransform.position = pos;
						footRectTransform.SetParent (patternRectTransform);
						Image footImage = footObj.GetComponent<Image> ();
						if (i == pattern.anchorPoint1) {
							footImage.color = Color.green;
							tangible.anchor1 = footObj.transform;
						}else if (i == pattern.anchorPoint2) {
							footImage.color = Color.yellow;
							tangible.anchor2 = footObj.transform;
						} else {
							footImage.color = Color.grey;
						}
					}
					MathHelper.DrawCircle (center, pattern.radius, 50, Color.blue);
					tangibles.Add (tangible);
				}
			}
			if(patterns.Count > 0){
				clusterRadius = patterns.Max (ptn => ptn.radius);	
			}
			print ("clusterRadius: " + clusterRadius);
			patternsLoaded = true;
			if(OnPatternsLoaded != null){
				OnPatternsLoaded.Invoke (patterns);	
			}
		}

		// private void OnDrawGizmos() {
		//     foreach (DbscanPoint point in dbscanPoints) {
		//if(!point.IsNoise && point.ClusterId != -1){
		//            Gizmos.color = clusterColors[point.ClusterId-1];    
		//        }else{
		//            Gizmos.color = Color.black;
		//        }
		//        Gizmos.DrawSphere(point.point, 0.25f);
		//    }
		//}


		private void OnGUI ()
		{
			if(!GlobalSettings.Instance.debugOutput){
				return;
			}

			GUIStyle style = new GUIStyle ();
			style.fontSize = 32;
			GUILayout.BeginVertical (style);
			string header = "";
			header += "pattern\t";
			for (int i = 1; i <= clusterPointsDict.Count; i++) {
				header += i + "\t";
			}
			GUILayout.Label (header, style);

			if (patternFitnessDict != null) {
				for (int i = 0; i < patterns.Count; i++) {
					TangiblePattern pattern = patterns [i];
					if (patternFitnessDict.ContainsKey (pattern)) {
						string fitnessString = pattern.id + "\t";
						List<ClusterAssociation> associations = patternFitnessDict [pattern];
						foreach (ClusterAssociation association in associations) {
							fitnessString += association.distance.ToString ("0.0000") + "\t";
						}
						GUILayout.Label (fitnessString, style);
					}
				}
			}

			GUILayout.EndVertical ();
		}


		public void RecognizeTangibles (List<TangiblePattern> patterns)
		{
			patternFitnessDict = new Dictionary<TangiblePattern, List<ClusterAssociation>> ();

			//ResetClusters();
			//ClearClusters ();

			DoClustering (clusterRadius * GlobalSettings.Instance.clusterRadiusScaler,
						  GlobalSettings.Instance.minNumOfPointsInCluster);

			foreach (TangiblePattern pattern in patterns) {
				List<ClusterAssociation> associations = RecognizeTangiblesPattern (pattern);
				patternFitnessDict.Add (pattern, associations);
			}
		}
		/// <summary>
		/// Recognizes the tangibles pattern.
		/// </summary>
		/// <returns>Fites dictionary <clusterID distance> </returns>
		/// <param name="pattern">Pattern.</param>
		List<ClusterAssociation> RecognizeTangiblesPattern (TangiblePattern pattern)
		{
			List<ClusterAssociation> associations = new List<ClusterAssociation> ();


			foreach (int clusterId in clusterPointsDict.Keys) {
				ClusterAssociation association = RecognizeClusterPattern (pattern, clusterId);
				associations.Add (association);
			}
			return associations;
		}



		ClusterAssociation RecognizeClusterPattern (TangiblePattern pattern, int clusterId)
		{
			ClusterAssociation association = new ClusterAssociation ();


			Tangible tangible = tangibles.Find (t => t.pattern.id == pattern.id);
//#warning reset position of tangible
			//tangible.transform.position = Vector3.zero;
			//tangible.transform.rotation = Quaternion.identity;


			List<Vector2> clusterPoints = new List<Vector2> ();

			List<DbscanPoint> clsPts = clusterPointsDict [clusterId];
			clusterPoints.AddRange (clsPts.Select (item => item.point).ToList<Vector2> ());
			Vector2 clusterCenter = MathHelper.ComputeCenter (clusterPoints, clusterColors [clusterId - 1]);
			foreach (DbscanPoint scanPoint in clsPts) {
				if (!scanPoint.IsNoise) {
					scanPoint.clusterTouch.clusterCenter = clusterCenter;
				}
			}

			// find anchor points:
			float anchorDistance = float.MaxValue;
			int firstAnchor = 0;
			int secondAnchor = 0;
			for (int i = 0; i < clsPts.Count; i++) {
				for (int j = 0; j < clsPts.Count; j++) {
					if (i == j) {
						continue;
					}
					float distance = Vector2.Distance (clsPts [i].point, clsPts [j].point);
					if (Mathf.Abs (distance - pattern.anchorDistance) < GlobalSettings.Instance.anchorTolerance) {
						clsPts [i].clusterTouch.GetComponent<SpriteRenderer> ().color = Color.white;
						clsPts [j].clusterTouch.GetComponent<SpriteRenderer> ().color = Color.white;
						anchorDistance = distance;
						float firstDistFromCenter = Vector2.Distance (clsPts [i].point, clusterCenter);
						float secondDistFromCenter = Vector2.Distance (clsPts [j].point, clusterCenter);
						firstAnchor = i;
						secondAnchor = j;
						if (secondDistFromCenter > firstDistFromCenter) {
							int tmp = firstAnchor;
							firstAnchor = secondAnchor;
							secondAnchor = tmp;
						}
						clsPts [firstAnchor].clusterTouch.GetComponent<SpriteRenderer> ().color = Color.green;
						clsPts [secondAnchor].clusterTouch.GetComponent<SpriteRenderer> ().color = Color.yellow;
					}
				}
			}

			// find translation and rotation:

			Vector2 a = clsPts [firstAnchor].point - clsPts [secondAnchor].point;
			Vector2 b = tangible.anchor1.position - tangible.anchor2.position;

			float angle = Vector2.SignedAngle (b, a);

			GlobalSettings.Instance.SetRotationAngle (angle);
			Vector3 rot = new Vector3(0, 0, angle);



			(tangible.transform as RectTransform).eulerAngles = rot;
			Vector2 tangibleOffset = new Vector2 (tangible.transform.position.x - tangible.anchor1.position.x,
												  tangible.transform.position.y - tangible.anchor1.position.y);
			Vector2 pos = clsPts [firstAnchor].point + tangibleOffset;
			tangible.transform.position = pos;


			Transform [] feet = tangible.GetComponentsInChildren<Transform> ();
			List<Vector2> feetPoints = new List<Vector2> ();
			foreach (Transform foot in feet) {
				if (foot.CompareTag ("Foot")) {
					feetPoints.Add (foot.position);
				}
			}

			float minDistSum = 0;
			for (int i = 0; i < feetPoints.Count; i++) {
				float minDist = float.MaxValue;
				for (int j = 0; j < clsPts.Count; j++) {
					float dist = (feetPoints [i] - clsPts [j].point).sqrMagnitude;
					if (dist < minDist) {
						minDist = dist;
					}
				}
				minDistSum += 100 * minDist * minDist; // * Screen.dpi/Screen.width; // * minDist;
			}


			association.pattern = pattern;
			association.position = pos;
			association.rotation = rot;
			association.distance = minDistSum;
			association.clusterId = clusterId;

			return association;
		}


		public void AssignTangiblesPositions ()
		{

			//List<ClusterAssociation> bestFits = new List<ClusterAssociation> ();
			//foreach (TangiblePattern pattern in patternFitnessDict.Keys) {
			//	List<ClusterAssociation> associations = patternFitnessDict [pattern];
			//	float minDist = float.MaxValue;
			//	int minDistI = 0;
			//	for (int i = 0; i < associations.Count; i++) {
			//		if (associations [i].distance < minDist) {
			//			minDistI = i;
			//			minDist = associations [i].distance;
			//		}
			//	}
			//	Tangible tangible = tangibles.Find (t => t.pattern.id == pattern.id);
			//	if (minDist < GlobalSettings.Instance.patternFitThreshold) {
			//		tangible.UpdatePosition (associations [minDistI].position, associations [minDistI].rotation);
			//	} else {
			//		tangible.ResetPosition ();
			//	}
			//}
		}

		public void DoClustering (float radius, int minNumOfPoints)
		{
			ResetClusters ();
			GlobalSettings.Instance.SetNumClusterPoints (dbscanPoints.Count);
			clusterCount = DensityBasedClustering.DBScan (dbscanPoints, radius, minNumOfPoints);
			if(clusterCount > 0){
				clusterColors = ClusterColors (ClusterCount);
				//clusterPointsDict = new Dictionary<int, List<DbscanPoint>> ();
				//for (int i = 1; i <= ClusterCount; i++) {
				//	clusterPointsDict.Add (i, new List<DbscanPoint> ());
				//}
				//foreach (DbscanPoint p in dbscanPoints) {
				//	if (!p.IsNoise && p.ClusterId != -1) {
				//		clusterPointsDict [p.ClusterId].Add (p);
				//	} else {
				//		//print ("is Noise");
				//	}
				//}
				foreach (ClusterTouch ct in touchObjects) {
					Color color = Color.black;
					if (!ct.dbscanPoint.IsNoise && ct.dbscanPoint.ClusterId != -1) {
						ct.ClusterId = ct.dbscanPoint.ClusterId;
						color = clusterColors [ct.ClusterId - 1];
					}
					ct.SetClusterColor (color);
				}
			}


		}

		public void ResetClusters ()
		{
			foreach (DbscanPoint dsp in dbscanPoints) {
				dsp.Reset ();
			}
			foreach (ClusterTouch ct in touchObjects) {
				ct.Reset ();
			}
		}

		public void ClearClusters ()
		{
			dbscanPoints.Clear ();
			foreach (ClusterTouch ct in touchObjects) {
				Destroy (ct.gameObject);
			}
			touchObjects.Clear ();
		}


		List<Color> ClusterColors (int N)
		{
			List<Color> colors = new List<Color> ();
			float step = 360.0f / N;
			for (int i = 0; i < N; i++) {
				colors.Add (Color.HSVToRGB ((i * step) / 360.0f, 1, 1));
			}
			return colors;
		}
	}
}

