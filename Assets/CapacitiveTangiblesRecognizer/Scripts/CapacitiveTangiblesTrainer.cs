using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

namespace CTR {
	public class CapacitiveTangiblesTrainer : MonoBehaviour {

		public GameObject touchPointPrefab;
		public Text patternID;
		RectTransform rectTransform;

		List<GameObject> patternPointsVisuals;

		void Start() {
			patternID.text = "1";
			rectTransform = transform as RectTransform;
			patternPointsVisuals = new List<GameObject>();
		}

		private void Update ()
		{
			//if (Input.GetMouseButton(0)) {
			//	Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			//	Rect screenRect = MathHelper.RectTransformToScreenSpace(rectTransform);
			//	Vector2 panelCenter = Camera.main.ScreenToWorldPoint(screenRect.center);
			//	Debug.DrawLine(pos, panelCenter, Color.red, 10);
			//}

		}


		public void SavePattern () {
			List<Vector2> patternPoints = new List<Vector2> ();
			Touch [] touches = Input.touches;

			if (Input.GetMouseButtonDown (0)) {
				Vector2 pos = Input.mousePosition;
				if (RectTransformUtility.RectangleContainsScreenPoint (rectTransform, pos)) {
					patternPoints.Add (pos);
				}
			}

			foreach (Touch touch in touches) {
				Vector2 pos = touch.position;
				if (RectTransformUtility.RectangleContainsScreenPoint (rectTransform, pos)) {
					//patternPoints.Add(pos);
					patternPoints.Add (pos);
				}
			}


			Vector2 patternCenter = MathHelper.ComputeCenter (patternPoints, Color.red);
			float radius = 0;
			float meanDistance = 0;
			Vector2 panelCenter = MathHelper.RectTransformToScreenSpace(rectTransform).center;
			Vector2 centerOffset = patternCenter- panelCenter;
			print ("panelCenter: " + panelCenter);
			print ("patternCenter: " + patternCenter);
			print ("centerOffset: " + centerOffset);
			for (int i = 0; i < patternPoints.Count; i++) {
				Debug.DrawLine (patternPoints [i],
							   patternPoints [i] - patternCenter,
							   Color.green, 30);
				
				// compute radius
				float dist = Vector2.Distance (patternCenter, patternPoints [i]);
				meanDistance += dist;
				if (dist > radius) {
					radius = dist;
				}

				// move point relative to center
				//patternPoints [i] = patternPoints [i] - center;
			}
			meanDistance /= patternPoints.Count;
			MathHelper.DrawCircle (patternCenter, radius, 50, Color.blue);

			Tuple<int, int> minDistPair = new Tuple<int, int> ();
			float minDist = float.MaxValue;
			for (int i = 0; i < patternPoints.Count; i++) {
				for (int j = 0; j < patternPoints.Count; j++) {
					if (i != j) {
						float dist = Vector2.Distance (patternPoints [i], patternPoints [j]);
						if (dist < minDist) {
							minDist = dist;
							minDistPair.first = i;
							minDistPair.second = j;
						}
					}
				}
			}
			float firstDistFromCenter = Vector2.Distance (patternPoints [minDistPair.first], patternCenter);
			float secondDistFromCenter = Vector2.Distance (patternPoints [minDistPair.second], patternCenter);



			if (secondDistFromCenter > firstDistFromCenter && GlobalSettings.Instance.flipRotation) {
				int tmp = minDistPair.first;
				minDistPair.first = minDistPair.second;
				minDistPair.second = tmp;
			}

			print(minDistPair);

			TangiblePattern pattern = new TangiblePattern ();
			int id = 0;
			if(int.TryParse(patternID.text, out id)){
				pattern.id = id;
			}else{
				id = 0;
			}

			// rotate pattern vertical
			Vector2 a = patternPoints [minDistPair.first] - patternPoints [minDistPair.second];
			Vector2 b = Vector2.up;
			float angle = Vector2.SignedAngle (a, b);
			print ("angle:" + angle);
			GlobalSettings.Instance.SetRotationAngle (angle);

			Vector3 rot = new Vector3 (0, 0, angle);
			print ("angle:" + angle);

			foreach(GameObject pt in patternPointsVisuals){
				Destroy(pt);
			}

			for (int i = 0; i < patternPoints.Count; i++){
				CreateTouchPoint (patternPoints [i], Color.green);


				// move point relative to center
				patternPoints[i] = patternPoints[i] - centerOffset;
				CreateTouchPoint (patternPoints [i], Color.yellow);

				// rotate vertical
				patternPoints [i] = MathHelper.RotatePointAroundPivot (patternPoints[i], panelCenter, rot);
				CreateTouchPoint (patternPoints[i], Color.red);
			}

			pattern.points = patternPoints;
			pattern.radius = radius;
			pattern.meanDistance = meanDistance;
			pattern.anchorDistance = minDist;
			pattern.gridStep = minDist / 2;
			pattern.anchorPoint1 = minDistPair.first;
			pattern.anchorPoint2 = minDistPair.second;

			string json = JsonUtility.ToJson (pattern, true);
			string fullfilepath = TangiblesFileUtils.PatternFilename (patternID.text);
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


