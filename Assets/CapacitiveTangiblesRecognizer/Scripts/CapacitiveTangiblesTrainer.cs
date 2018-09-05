using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

namespace CTR
{

	public class CapacitiveTangiblesTrainer : SingletonBehaviour<CapacitiveTangiblesTrainer>
	{
		// 0 = none
        // 1 = mark anchor and info
        // 2 = show rotated points before flip
        // 3 = show rotated points after flip
		private int debugType = 3;

		public RectTransform patternMonitor;

		public GameObject patternPrefab;
		public GameObject patternFootPrefab;

		public GameObject touchPointPrefab;
		public Text patternIdText;
		public Text infoText;
		public Text debugText;
		int patternId = 0;
		RectTransform rectTransform;

		List<TangiblePattern> patterns = new List<TangiblePattern>();
		List<GameObject> patternPointsVisuals;
		List<GameObject> monitorPatterns;

		void Start()
		{
			rectTransform = transform as RectTransform;
			patternPointsVisuals = new List<GameObject>();
			monitorPatterns = new List<GameObject>();
		}

		public void ClearPatternPoints()
		{
			foreach (GameObject vis in patternPointsVisuals)
			{
				Destroy(vis);
			}
			foreach (GameObject mon in monitorPatterns)
			{
				Destroy(mon);
			}
			patternPointsVisuals.Clear();
			monitorPatterns.Clear();
			patterns.Clear();
		}

		public void RemoveLastTrainedPattern()
		{
			if (monitorPatterns.Count > 0)
			{
				Destroy(monitorPatterns[monitorPatterns.Count - 1]);
				monitorPatterns.RemoveAt(monitorPatterns.Count - 1);
				patterns.RemoveAt(patterns.Count - 1);
			}
		}      

		public void SetPatternID()
		{
			int id;
			if (int.TryParse(patternIdText.text, out id))
			{
				patternId = id;
			}
		}


		public void TrainPattern()
		{

			// get touch inputs from rectangle and put in a list
			Touch[] touches = Input.touches;
			List<Vector2> patternPoints = new List<Vector2>();
			foreach (Touch touch in touches)
			{
				Vector2 pos = touch.position;
				if (RectTransformUtility.RectangleContainsScreenPoint(rectTransform, pos))
				{
					patternPoints.Add(pos);
				}
			}

			// find the two closest points
			Vector2 patternCenter = MathHelper.ComputeCenter(patternPoints);
			Vector2 panelCenter = MathHelper.RectTransformToScreenSpace(rectTransform).center;
			Vector2 centerOffset = patternCenter - panelCenter;
			print("panelCenter: " + panelCenter);
			print("patternCenter: " + patternCenter);
			print("centerOffset: " + centerOffset);
			float minDist = 0; // double grid (cell) size value
			Tuple<int, int> anchorPair = MathHelper.FindMinDistancePair(patternPoints, patternCenter, out minDist);
			if (anchorPair.first == -1 || anchorPair.second == -1)
			{
				infoText.text = "Pattern does not contain anchor points!";
				infoText.color = Color.red;
			}
			else
			{
				TangiblePattern pattern = new TangiblePattern();

				// closest points are "base" of pattern and determine grid (cell) size and rotation
				// rotate to align "base" horizontally
				Vector2 anchorVector = new Vector2(
					patternPoints[anchorPair.first].x - patternPoints[anchorPair.second].x,
					patternPoints[anchorPair.first].y - patternPoints[anchorPair.second].y);
				float angle = -Vector2.SignedAngle(new Vector2(1, 0), anchorVector);
				Vector3 rotation = new Vector3(0, 0, angle);

				// visualize touch points (and remove the old ones)
				foreach (GameObject pt in patternPointsVisuals)
				{
					Destroy(pt);
				}
            
				if(debugType==1)
					for (int i = 0; i < patternPoints.Count; i++)
                        if (i == anchorPair.first || i == anchorPair.second)
                            CreateTouchPoint(patternPoints[i], Color.blue);
                        else
                            CreateTouchPoint(patternPoints[i], Color.green);

				// continue 'rotate to align "base" horizontally'
				for (int i = 0; i < patternPoints.Count; i++)
				{
					patternPoints[i] = MathHelper.RotatePointAroundPivot(patternPoints[i], patternPoints[anchorPair.first], rotation);
					if (debugType == 2)
					{
						if (i == anchorPair.first || i == anchorPair.second)
							CreateTouchPoint(patternPoints[i], Color.blue);
						else
							CreateTouchPoint(patternPoints[i], Color.green);
						debugText.text = "Rotation angle: " + angle;
						debugText.color = Color.green;
					}
				}

				// infopoint(s) should be above base points
				for (int i = 0; i < patternPoints.Count; i++)
					if (i != anchorPair.first && i != anchorPair.second)
						if (patternPoints[i].y < patternPoints[anchorPair.first].y)
							for (int j = 0; j < patternPoints.Count; j++)
							{
								patternPoints[j] = MathHelper.RotatePointAroundPivot(patternPoints[j], patternPoints[anchorPair.first], new Vector3(0, 0, 180));
						        if (debugType == 3)
                                    if (j == anchorPair.first || j == anchorPair.second)
                                        CreateTouchPoint(patternPoints[j], Color.blue);
                                    else
                                        CreateTouchPoint(patternPoints[j], Color.green);
							}

				// make sure the most left base point is point one
				if (patternPoints[anchorPair.first].x > patternPoints[anchorPair.second].x)
				{
					int tmp = anchorPair.first;
					anchorPair.first = anchorPair.second;
					anchorPair.second = tmp;
				}

				// get info (grid-)coordinates as Tuple<int, int>
				Tuple<int, int> infoCoord = new Tuple<int, int>(0,0);
				for (int i = 0; i < patternPoints.Count; i++)
					if (i != anchorPair.first && i != anchorPair.second)
					{
						float rawX = patternPoints[i].x - patternPoints[anchorPair.first].x;
						float rawY = patternPoints[i].y - patternPoints[anchorPair.first].y;
						int coordX = Mathf.RoundToInt(rawX / (minDist / 2));
						int coordY = Mathf.RoundToInt(rawY / (minDist / 2));
						infoCoord.first = coordX;
						infoCoord.second = coordY;
					}

				// put coords in 'pattern' and add 'pattern' to 'patterns'
				if (infoCoord.first==0 && infoCoord.second==0)
				{
					infoText.text = "Pattern does not contain enought information!";
					infoText.color = Color.red;
				}
				else
				{
					infoText.text = "Pattern trainig successful! Info Point is (" + infoCoord.first + "," + infoCoord.second + ")";
					infoText.color = Color.green;

					pattern.id = patternId;
					pattern.infoCoord = infoCoord;

					patterns.Add(pattern);
				}
			}
		}

		public void SavePattern()
		{
			TangiblePattern pattern = new TangiblePattern();
			pattern.id = patternId;
			pattern.trainingSamples = patterns.Count;

			//check if all samples are equal
			List<Tuple<int, int>> infoCoords = new List<Tuple<int, int>>();

			for (int i = 0; i < patterns.Count; i++)
			{
				TangiblePattern ptn = patterns[i];
				infoCoords.Add(ptn.infoCoord);
			}

			// check if samples match 
			bool match = true;
			for (int i = 0; i < infoCoords.Count - 1; i++)
				if (infoCoords[i].first != infoCoords[i + 1].first
					|| infoCoords[i].second != infoCoords[i + 1].second)
					match = false;
				
			if (match)
			{
				pattern.infoCoord = infoCoords[0];
				infoText.text = "Training samples match!";
				infoText.text += pattern.ToString();
				infoText.color = Color.green;
			}
			else
			{
				infoText.text = "Training samples do not match!";
				infoText.color = Color.red;
			}
         
			#region save ne pattern to json
			string json = JsonUtility.ToJson(pattern, true);
			string fullfilepath = TangiblesFileUtils.PatternFilename(patternId.ToString());
			print(fullfilepath);
			File.WriteAllText(fullfilepath, json);

			//SavePatternStatistics();
			#endregion
		}
      
		void CreateTouchPoint(Vector2 screenPos, Color color)
		{
			GameObject touchPoint = Instantiate(touchPointPrefab);
			RectTransform rt = (touchPoint.transform as RectTransform);
			rt.position = screenPos;
			rt.SetParent(rectTransform);
			touchPoint.GetComponent<Image>().color = color;
			//print (rt.position);
			patternPointsVisuals.Add(touchPoint);
		}
	}


}