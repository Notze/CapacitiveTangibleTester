using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

namespace CTR {



	public class CapacitiveTangiblesTrainer : SingletonBehaviour<CapacitiveTangiblesTrainer> {

		public RectTransform patternMonitor;

		public GameObject patternPrefab;
		public GameObject patternFootPrefab;

		public GameObject touchPointPrefab;
		public Text patternIdText;
		public Text infoText;
		int patternId = 0;
		RectTransform rectTransform;

		List<TangiblePattern> patterns = new List<TangiblePattern>();
		List<GameObject> patternPointsVisuals;
		List<GameObject> monitorPatterns;

		void Start() {
			rectTransform = transform as RectTransform;
			patternPointsVisuals = new List<GameObject>();
			monitorPatterns = new List<GameObject>();
		}

		public void ClearPatternPoints(){
			foreach(GameObject vis in patternPointsVisuals){
				Destroy (vis);
			}
			foreach(GameObject mon in monitorPatterns){
				Destroy (mon);
			}
			patternPointsVisuals.Clear();
			monitorPatterns.Clear();
			patterns.Clear();
		}

		public void RemoveLastTrainedPattern(){
			if(monitorPatterns.Count > 0){
				Destroy (monitorPatterns[monitorPatterns.Count - 1]);
				monitorPatterns.RemoveAt(monitorPatterns.Count - 1);
			}
		}

		//void Update () {
		//	if(GlobalSettings.Instance.modality == InputModality.Mouse){
		//		if (Input.GetMouseButton (0)) {
		//			Vector2 pos = Input.mousePosition;
		//			if (RectTransformUtility.RectangleContainsScreenPoint (rectTransform, pos)) {
		//				patternPoints.Add (pos);
		//			}
		//		}	
		//	}
		//}


		public void SetPatternID(){
			int id;
			if (int.TryParse (patternIdText.text, out id)) {
				patternId = id;
			}
		}

		public void TrainPattern() {
			List<Vector2> patternPoints = new List<Vector2> ();


			Touch [] touches = Input.touches;

			foreach (Touch touch in touches) {
				Vector2 pos = touch.position;
				if (RectTransformUtility.RectangleContainsScreenPoint (rectTransform, pos)) {
					patternPoints.Add (pos);
				}
			}

			Vector2 patternCenter = MathHelper.ComputeCenter(patternPoints);
			float radius = 0;
			float meanDistance = 0;
			Vector2 panelCenter = MathHelper.RectTransformToScreenSpace(rectTransform).center;
			Vector2 centerOffset = patternCenter - panelCenter;
			print ("panelCenter: " + panelCenter);
			print ("patternCenter: " + patternCenter);
			print ("centerOffset: " + centerOffset);
			for (int i = 0; i < patternPoints.Count; i++) {
				// compute radius
				float dist = Vector2.Distance (patternCenter, patternPoints [i]);
				meanDistance += dist;
				if (dist > radius) {
					radius = dist;
				}
			}

			meanDistance /= patternPoints.Count;
			MathHelper.DrawCircle (patternCenter, radius, 50, Color.blue);
			float minDist = 0;
			Tuple<int, int> minDistPair = MathHelper.FindMinDistancePair(patternPoints, patternCenter, out minDist);

			print(minDistPair);
			TangiblePattern pattern = new TangiblePattern();


			// rotate pattern vertical
			Vector2 a = patternPoints[minDistPair.first] - patternPoints[minDistPair.second];
			Vector2 b = Vector2.down;
			float angle = Vector2.SignedAngle(a, b);

			GlobalSettings.Instance.SetRotationAngle(angle);

			Vector3 rot = new Vector3 (0, 0, angle);

			foreach(GameObject pt in patternPointsVisuals){
				Destroy(pt);
			}

			for (int i = 0; i < patternPoints.Count; i++) {
				CreateTouchPoint (patternPoints[i], Color.green);

				// move point relative to center
				patternPoints[i] = patternPoints[i] - centerOffset;
				CreateTouchPoint(patternPoints[i], Color.yellow);

				// rotate vertical
				patternPoints[i] = MathHelper.RotatePointAroundPivot(patternPoints[i], panelCenter, rot);
				CreateTouchPoint(patternPoints[i], Color.red);

				// transform point into local space:
				patternPoints[i] = patternPoints[i] - panelCenter;
			}
			// find info point 1:
			int infoPoint1 = 0;
			float maxY = float.MinValue;
			for (int i = 0; i < patternPoints.Count; i++){
				if(patternPoints[i].y > maxY){
					infoPoint1 = i;
					maxY = patternPoints[i].y;
				}
			}
			// find info pont 2:
			int infoPoint2 = -1;
			for (int i = 0; i < patternPoints.Count; i++) {
				if (i != minDistPair.first && i != minDistPair.second && i != infoPoint1) {
					infoPoint2 = i;
					break;
				}
			}
			print("infoPoint1: " + infoPoint1 + " infoPoint2: " + infoPoint2);

			pattern.id = patternId;
			pattern.points = patternPoints;
			pattern.radius = radius;
			pattern.anchorDistance = minDist;
			pattern.anchorPoint1 = minDistPair.first;
			pattern.anchorPoint2 = minDistPair.second;
			pattern.infoPoint1 = infoPoint1;
			pattern.infoPoint2 = infoPoint2;

			patterns.Add(pattern);



			GameObject patternObj = CTRUtils.InstantiateTangibleObject(pattern, patternPrefab, patternFootPrefab, patternMonitor, false);
			monitorPatterns.Add(patternObj);

			Transform [] feet = patternObj.GetComponentsInChildren<Transform> ();
			float d = 2 * radius;
			float s = patternMonitor.GetComponent<GridLayoutGroup> ().cellSize.x;
			foreach (Transform foot in feet) {
				if (foot.CompareTag ("Foot")) {
					foot.transform.localPosition *= s/d;
				}
			}

		}

		public void SavePattern ()
		{
			TangiblePattern pattern = new TangiblePattern ();
			pattern.id = patternId;
			pattern.trainingSamples = patterns.Count;

			// compute mean values and SDs for each point
			List<Vector2> anchor1Points = new List<Vector2> ();
			List<Vector2> anchor2Points = new List<Vector2> ();
			List<Vector2> info1Points = new List<Vector2> ();
			List<Vector2> info2Points = new List<Vector2> ();

			for (int i = 0; i < patterns.Count; i++) {
				TangiblePattern ptn = patterns [i];
				anchor1Points.Add (ptn.points [ptn.anchorPoint1]);
				anchor2Points.Add (ptn.points [ptn.anchorPoint2]);
				info1Points.Add (ptn.points [ptn.infoPoint1]);
				info2Points.Add (ptn.points [ptn.infoPoint2]);
			}

			Vector2 anchor1Center = MathHelper.ComputeCenter(anchor1Points);
			Vector2 anchor2Center = MathHelper.ComputeCenter(anchor2Points);
			Vector2 info1Center = MathHelper.ComputeCenter(info1Points);
			Vector2 info2Center = MathHelper.ComputeCenter(info2Points);
			// create a pattern with computed values
			pattern.points = new List<Vector2> (TangiblePattern.numOfPoints);
			pattern.points.Add(anchor1Center);
			pattern.points.Add(anchor2Center);
			pattern.points.Add(info1Center);
			pattern.points.Add(info2Center);

			pattern.anchorPoint1 = 0;
			pattern.anchorPoint2 = 1;
			pattern.infoPoint1 = 2;
			pattern.infoPoint2 = 3;

			pattern.anchorDistance = Vector2.Distance(anchor1Center, anchor2Center);

			#region statistics
			pattern.standardDeviations = new List<float> (TangiblePattern.numOfPoints);
			pattern.meanDistances = new List<float>();
			List<float> anchor1Dist = new List<float> ();
			foreach (Vector2 v in anchor1Points) {
				anchor1Dist.Add (Vector2.Distance (anchor1Center, v));
			}
			pattern.meanDistances.Add (anchor1Dist.Mean());
			pattern.standardDeviations.Add (anchor1Dist.StandardDeviation ());
			print ("sd 0: " + pattern.standardDeviations [0]);

			List<float> anchor2Dist = new List<float> ();
			foreach (Vector2 v in anchor2Points) {
				anchor2Dist.Add (Vector2.Distance (anchor2Center, v));
			}
			pattern.meanDistances.Add (anchor2Dist.Mean ());
			pattern.standardDeviations.Add (anchor2Dist.StandardDeviation ());
			print ("sd 1: " + pattern.standardDeviations [1]);

			List<float> info1Dist = new List<float> ();
			foreach (Vector2 v in info1Points) {
				info1Dist.Add (Vector2.Distance (info1Center, v));
			}
			pattern.meanDistances.Add(info1Dist.Mean());
			pattern.standardDeviations.Add(info1Dist.StandardDeviation ());
			print ("sd 2: " + pattern.standardDeviations [2]);

			List<float> info2Dist = new List<float> ();
			foreach (Vector2 v in info2Points) {
				info2Dist.Add (Vector2.Distance (info2Center, v));
			}
			pattern.meanDistances.Add(info2Dist.Mean());
			pattern.standardDeviations.Add (info2Dist.StandardDeviation ());
			print ("sd 3: " + pattern.standardDeviations [3]);
			#endregion
			Vector2 patternCenter = MathHelper.ComputeCenter (pattern.points);

			for (int i = 0; i < pattern.points.Count; i++) {
				// compute radius
				float dist = Vector2.Distance (patternCenter, pattern.points [i]);
				if (dist > pattern.radius) {
					pattern.radius = dist;
				}
			}

			infoText.text = pattern.ToString();

			#region save ne pattern to json
			string json = JsonUtility.ToJson (pattern, true);
			string fullfilepath = TangiblesFileUtils.PatternFilename (patternId.ToString ());
			print (fullfilepath);
			File.WriteAllText (fullfilepath, json);
			#endregion
		}


		void CreateTouchPoint (Vector2 screenPos, Color color) {
			GameObject touchPoint = Instantiate (touchPointPrefab);
			RectTransform rt = (touchPoint.transform as RectTransform);
			rt.position = screenPos;
			rt.SetParent (rectTransform);
			touchPoint.GetComponent<Image>().color = color;
			print (rt.position);
			patternPointsVisuals.Add (touchPoint);
		}
	}


}


