using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

namespace CTR {
	public struct ClusterAssociation{
		public Vector3 position;
		public Vector3 rotation;
		public float probability;
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

			LoadTangiblesPatterns();
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
				if(inAcceptableDistance){
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
			}
			return !inAvoidArea && inAcceptableDistance;
		}

		//int rotationPoint = 0;
		void Update ()
		{

			// validate old tangible position
			//foreach (Tangible tangible in tangibles) {
			//	tangible.SavePosition ();
			//}

			// handle input. prefilter it
			switch (GlobalSettings.Instance.modality) {
			case InputModality.Mouse:
				HandleMouseInput();
				break;
			case InputModality.Touch:
				HandleTouchInput();
				break;
			}

			// do the recognition
			if (dbscanPoints.Count >= GlobalSettings.Instance.minNumOfPointsInCluster) {
				RecognizeTangibles();
				AssignTangiblesPositions();
				//ClearClusters();
			}
		}

		public void DeleteTangiblesPatterns() {
			TangiblesFileUtils.DeleteTangibles ();
		}

		public void LoadTangiblesPatterns() {
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
					TangiblePattern pattern = JsonUtility.FromJson<TangiblePattern>(json);
					patterns.Add (pattern);

					GameObject patternObj = CTRUtils.InstantiateTangibleObject(pattern, patternPrefab, patternFootPrefab, recognizerPanel);
					Tangible tangible = patternObj.GetComponent<Tangible>();
					tangibles.Add(tangible);
				}
			}
			if(patterns.Count > 0){
				clusterRadius = patterns.Max(ptn => ptn.radius);	
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
							fitnessString += association.probability.ToString ("0.0000") + "\t";
						}
						GUILayout.Label (fitnessString, style);
					}
				}
			}

			GUILayout.EndVertical ();
		}

		public void RecognizeTangibles(){

			patternFitnessDict = new Dictionary<TangiblePattern, List<ClusterAssociation>> ();

			DoClustering();

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
			clusterPoints.AddRange (clsPts.Select (item => item.point).ToList<Vector2>());
			Vector2 clusterCenter = MathHelper.ComputeCenter(clusterPoints);
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
					float distance = Vector2.Distance (clsPts[i].point, clsPts[j].point);
					print ("anchor dist: " + distance);
					float anchorTolerance = pattern.anchorDistance * GlobalSettings.Instance.anchorTolerance;
					if (Mathf.Abs (distance - pattern.anchorDistance) < anchorTolerance) {
						print ("found anchor" + i + " " + j + " " + distance);
						//clsPts[i].clusterTouch.SetColor(Color.white);
						//clsPts[j].clusterTouch.SetColor(Color.white);
						anchorDistance = distance;
						float firstDistFromCenter = Vector2.Distance(clsPts[i].point, clusterCenter);
						float secondDistFromCenter = Vector2.Distance(clsPts[j].point, clusterCenter);
						firstAnchor = i;
						secondAnchor = j;
						if (secondDistFromCenter > firstDistFromCenter) {
							int tmp = firstAnchor;
							firstAnchor = secondAnchor;
							secondAnchor = tmp;
						}
						if(clsPts[firstAnchor].clusterTouch != null) {
							clsPts[firstAnchor].clusterTouch.SetColor(Color.green);
						} else {
							Debug.LogWarning("Cluster Touch does not exist");
						}
						if(clsPts[secondAnchor].clusterTouch != null) {
							clsPts[secondAnchor].clusterTouch.SetColor(Color.yellow);
						} else {
							Debug.LogWarning("Cluster Touch does not exist");
						}
					}
				}
			}
			// find info points
			float maxDistanceFromAnchor1 = float.MinValue;
			float minDistanceFromAnchor1 = float.MaxValue;
			int firstInfo = 0;
			int secondInfo = 0;

			for (int i = 0; i < clsPts.Count; i++) {
				if (i != firstAnchor && i != secondAnchor) {
					float dist = Vector2.Distance (clsPts [firstAnchor].point, clsPts [i].point);
					if (dist < minDistanceFromAnchor1) {
						minDistanceFromAnchor1 = dist;
						secondInfo = i;
					}
				}
			}

			for (int i = 0; i < clsPts.Count; i++){
				if(i != firstAnchor && i != secondAnchor && i != secondInfo){
					float dist = Vector2.Distance(clsPts[firstAnchor].point, clsPts[i].point);
					if(dist > maxDistanceFromAnchor1){
						maxDistanceFromAnchor1 = dist;
						firstInfo = i;
					}
				}
			}
			if(clsPts[firstInfo].clusterTouch != null){
				clsPts[firstInfo].clusterTouch.SetColor(Color.blue);	
			}else{
				Debug.LogWarning("Cluster Touch does not exist");
			}
			if(clsPts[secondInfo].clusterTouch != null){
				clsPts[secondInfo].clusterTouch.SetColor(Color.red);
			}else{
				Debug.LogWarning("Cluster Touch does not exist");
			}


			// find translation and rotation:

			Vector2 a = clsPts[firstAnchor].point - clsPts[secondAnchor].point;
			Vector2 b = tangible.anchor1.localPosition - tangible.anchor2.localPosition;

			float angle = Vector2.SignedAngle(b, a);
			print("angle:" + angle);
			GlobalSettings.Instance.SetRotationAngle(angle);
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

			float distAnchor1 = Vector2.Distance (tangible.anchor1.position, clsPts [firstAnchor].point);
			float distAnchor2 = Vector2.Distance (tangible.anchor2.position, clsPts [secondAnchor].point);

			float distInfo1 = Vector2.Distance (tangible.info1.position, clsPts [firstInfo].point);
			float distInfo2 = Vector2.Distance (tangible.info2.position, clsPts [secondInfo].point);

			float zAnchor1 = (distAnchor1 - pattern.meanDistances [0]) / pattern.standardDeviations [0];
			float zAnchor2 = (distAnchor2 - pattern.meanDistances [1]) / pattern.standardDeviations [1];
			float zInfo1 = (distInfo1 - pattern.meanDistances [2]) / pattern.standardDeviations [2];
			float zInfo2 = (distInfo2 - pattern.meanDistances [3]) / pattern.standardDeviations [3];


			float pAnchor1 = 2 * MathHelper.NormalDistribution(Mathf.Abs (zAnchor1), 0, 1);
			float pAnchor2 = 2 * MathHelper.NormalDistribution(Mathf.Abs (zAnchor2), 0, 1);
			float pInfo1 = 2 * MathHelper.NormalDistribution(Mathf.Abs (zInfo1), 0, 1);
			float pInfo2 = 2 * MathHelper.NormalDistribution(Mathf.Abs (zInfo2), 0, 1);

			//float pAnchor1 = MathHelper.ProbabilityOfValue(Mathf.Abs(zAnchor1),0,1);
			//float pAnchor2 = MathHelper.ProbabilityOfValue(Mathf.Abs(zAnchor2),0,1);
			//float pInfo1 = MathHelper.ProbabilityOfValue(Mathf.Abs(zInfo1),0,1);
			//float pInfo2 = MathHelper.ProbabilityOfValue(Mathf.Abs(zInfo2),0,1);

			//print ("dist anchor1: " + distAnchor1 + " z: " + zAnchor1 + " p: " + pAnchor1);
			//print ("dist anchor2: " + distAnchor2 + " z: " + zAnchor2 + " p: " + pAnchor2);
			//print ("dist info1: " + distInfo1 + " z: " + zInfo1 + " p: " + pInfo1);
			//print ("dist info2: " + distInfo2 + " z: " + zInfo2 + " p: " + pInfo2);

			float xDist = Mathf.Pow ((tangible.lastKnownPosition.x - clusterCenter.x) / Screen.width, 2);
			float yDist = Mathf.Pow ((tangible.lastKnownPosition.y - clusterCenter.y) / Screen.height, 2);
			float distTangibleCluster = 1.0f - Mathf.Sqrt(xDist + yDist);

			float aWeight = GlobalSettings.Instance.anchorWeight;
			float posWeight = GlobalSettings.Instance.positionWeight;

			//float recognitionProbability = aWeight/2 * pAnchor1 + aWeight/2 * pAnchor2 + (1.0f-aWeight)/2 * pInfo1 + (1.0f - aWeight)/2 * pInfo2;
			//float positionPorbability = tangible.positionProbability * distTangibleCluster;

			//float probability = posWeight * positionPorbability + (1.0f - posWeight) * recognitionProbability;

			float probability = 0.5f*pInfo1 + 0.5f*pInfo2;

			association.pattern = pattern;
			association.position = pos;
			association.rotation = rot;
			association.probability = probability;
			association.clusterId = clusterId;

			return association;
		}


		public void AssignTangiblesPositions ()
		{
			Dictionary<int, ClusterAssociation> clusterFitnessDict = new Dictionary<int, ClusterAssociation>();
			foreach(Tangible tangible in tangibles){
				tangible.ResetPosition();
			}

			foreach (TangiblePattern pattern in patternFitnessDict.Keys) {
				List<ClusterAssociation> associations = patternFitnessDict[pattern];
				foreach(ClusterAssociation association in associations){
					if(!clusterFitnessDict.ContainsKey(association.clusterId)){
						clusterFitnessDict.Add (association.clusterId, association);
					}else{
						if(clusterFitnessDict[association.clusterId].probability < association.probability){
							clusterFitnessDict [association.clusterId] = association;
						}
					}
				}
			}

			foreach(int clusterId in clusterFitnessDict.Keys){
				ClusterAssociation association = clusterFitnessDict [clusterId];
				Tangible tangible = tangibles.Find (t => t.pattern.id == association.pattern.id);
				if(association.probability > GlobalSettings.Instance.patternFitThreshold){
					tangible.UpdatePosition(association.position, association.rotation, association.probability);
				}
			}
		}

		public void DoClustering(){
			DoClustering (clusterRadius * GlobalSettings.Instance.clusterRadiusScaler,
						  GlobalSettings.Instance.minNumOfPointsInCluster);
		}

		void DoClustering (float radius, int minNumOfPoints)
		{
			ResetClusters ();
			GlobalSettings.Instance.SetNumClusterPoints (dbscanPoints.Count);
			clusterCount = DensityBasedClustering.DBScan (dbscanPoints, radius, minNumOfPoints);
			if(clusterCount > 0){
				clusterColors = ClusterColors (ClusterCount);
				clusterPointsDict = new Dictionary<int, List<DbscanPoint>> ();
				for (int i = 1; i <= ClusterCount; i++) {
					clusterPointsDict.Add (i, new List<DbscanPoint> ());
				}
				foreach (DbscanPoint p in dbscanPoints) {
					if (!p.IsNoise && p.ClusterId != -1) {
						clusterPointsDict [p.ClusterId].Add (p);
					} else {
						//print ("is Noise");
					}
				}
				foreach (ClusterTouch ct in touchObjects) {
					Color color = Color.black;
					if (!ct.dbscanPoint.IsNoise && ct.dbscanPoint.ClusterId != -1) {
						ct.ClusterId = ct.dbscanPoint.ClusterId;
						color = clusterColors [ct.ClusterId - 1];
					}
					ct.SetColor(color);
				}
			}
		}

		public void ResetClusters ()
		{
			foreach (DbscanPoint dsp in dbscanPoints) {
				dsp.Reset();
			}
			foreach (ClusterTouch ct in touchObjects) {
				ct.Reset();
			}
		}

		public void ClearClusters() {
			foreach (DbscanPoint point in dbscanPoints) {
				DestroyImmediate(point.clusterTouch.gameObject);
			}
			dbscanPoints.Clear();
			touchObjects.Clear();
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

		public void NotifyTangibleUpdate(int id, Vector2 position, Quaternion rotation){
			if(OnTangibleUpdated != null){
				OnTangibleUpdated.Invoke (id, position, rotation);	
			}
		}
	}
}

