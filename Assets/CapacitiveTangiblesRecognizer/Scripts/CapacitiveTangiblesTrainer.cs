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
		public Text patternID;

		RectTransform rectTransform;

		List<TangiblePattern> patterns = new List<TangiblePattern>();
		List<GameObject> patternPointsVisuals;

		void Start() {
			patternID.text = "1";
			rectTransform = transform as RectTransform;
			patternPointsVisuals = new List<GameObject>();
		}

		public void ClearPatternPoints(){
			foreach(GameObject vis in patternPointsVisuals){
				Destroy (vis);
			}
			patternPointsVisuals.Clear();
			patterns.Clear();
		}

		//void Update () {
		//	//if(GlobalSettings.Instance.modality == InputModality.Mouse){
		//	//	if (Input.GetMouseButton (0)) {
		//	//		Vector2 pos = Input.mousePosition;
		//	//		if (RectTransformUtility.RectangleContainsScreenPoint (rectTransform, pos)) {
		//	//			patternPoints.Add (pos);
		//	//		}
		//	//	}	
		//	//}
		//}

		public void TrainPattern() {
			int id = 0;
			int patternId = 0;
			if (int.TryParse(patternID.text, out id)) {
				patternId = id;
			}
			TrainPattern(patternId);
		}

		public void TrainPattern (int patternId)
		{
			List<Vector2> patternPoints = new List<Vector2> ();
			Touch [] touches = Input.touches;

			foreach (Touch touch in touches) {
				Vector2 pos = touch.position;
				if (RectTransformUtility.RectangleContainsScreenPoint (rectTransform, pos)) {
					patternPoints.Add (pos);
				}
			}


			Vector2 patternCenter = MathHelper.ComputeCenter(patternPoints, Color.red);
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
					maxY = patternPoints [i].y;
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
			print ("infoPoint1:" + infoPoint1 + " infoPoint2: " + infoPoint2);

			pattern.id = patternId;
			pattern.points = patternPoints;
			pattern.radius = radius;
			pattern.meanDistance = meanDistance;
			pattern.anchorDistance = minDist;
			pattern.gridStep = minDist / 2;
			pattern.anchorPoint1 = minDistPair.first;
			pattern.anchorPoint2 = minDistPair.second;
			pattern.infoPoint1 = infoPoint1;
			pattern.infoPoint2 = infoPoint2;

			patterns.Add(pattern);



			GameObject patternObj = CTRUtils.InstantiateTangibleObject(pattern, patternPrefab, patternFootPrefab, patternMonitor, false);


			Transform [] feet = patternObj.GetComponentsInChildren<Transform> ();
			float d = 2 * radius;
			float s = patternMonitor.GetComponent<GridLayoutGroup> ().cellSize.x;
			foreach (Transform foot in feet) {
				if (foot.CompareTag ("Foot")) {
					foot.transform.localPosition *= s/d;
				}
			}

		}

		public void SavePattern(){
			TangiblePattern pattern = new TangiblePattern();
			// compute mean values and SDs for each point

			// create ne pattern with computed values

			// save ne pattern to json
			string json = JsonUtility.ToJson(pattern, true);
			string fullfilepath = TangiblesFileUtils.PatternFilename(patternID.text);
			print (fullfilepath);
			File.WriteAllText (fullfilepath, json);
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


