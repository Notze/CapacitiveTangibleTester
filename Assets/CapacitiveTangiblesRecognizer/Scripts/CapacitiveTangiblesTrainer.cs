using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

namespace CTR
{
	public class CapacitiveTangiblesTrainer : SingletonBehaviour<CapacitiveTangiblesTrainer>
	{
        public ArrayList tangTypes = new ArrayList(new string[] { "plain", "controller" });
        

        // 0 = none
        // 1 = mark anchor and info
        // 2 = show rotated points before flip
        // 3 = show rotated points after flip
        // 4 = fake tangible
        public int debugType = 3;
		public bool outline = true;
        public Camera camera; // needed for fake tangible

		public RectTransform patternMonitor; // TODO obsolete?

		public GameObject patternPrefab;
		public GameObject patternFootPrefab;

		public GameObject touchPointPrefab;
		public Text patternIdText;
		public Text infoText;
		public Text debugText;
		public Dropdown debugTypeList;
		int patternId = -1;
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


		//TODO make a nicer outline; there's not even an outline yet! :D
		public void drawOutline(List<Vector2> patternPoints)
		{
			if (outline)
			{
				if (patternPoints.Count == 3) // errorcheck because this function is hardcoded to 3 points
				{
					Debug.DrawLine(patternPoints[0], patternPoints[1]);
					Debug.DrawLine(patternPoints[1], patternPoints[2]);
					Debug.DrawLine(patternPoints[2], patternPoints[0]);
				}
			}
		}

		private void flipPattern(List<Vector2> patternPoints, Tuple<int, int> anchorPair)
		{
			for (int j = 0; j < patternPoints.Count; j++)
			{
				patternPoints[j] = MathHelper.RotatePointAroundPivot(patternPoints[j], patternPoints[anchorPair.first], new Vector3(0, 0, 180));
				if (debugType >= 3)
					if (j == anchorPair.first || j == anchorPair.second)
						CreateTouchPoint(patternPoints[j], Color.blue);
					else
						CreateTouchPoint(patternPoints[j], Color.green);
			}
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

        // should be obsolete after complete adaption of other classes
        public void TrainPattern()
        {
            TrainPattern(TangiblePattern.Type.PLAIN);
        }

        public void TrainPattern(TangiblePattern.Type type)
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

            if (debugType == 4)
            {
                Vector2 middleOfScreen = camera.ViewportToWorldPoint(new Vector3(.5f, .5f, 0f));
                Vector2 point2 = middleOfScreen + new Vector2(0.1f, .0f);
                Vector2 point3 = middleOfScreen + new Vector2(0.1f, .3f);
                patternPoints.Add(middleOfScreen);
                patternPoints.Add(point2);
                patternPoints.Add(point3);
            }

            InfoText("Detected Touchpoints: " + patternPoints.Count + "\n"); // little dirty to use 'patternPoints' but should be ok

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
                InfoText("Pattern does not contain anchor points!", true, Color.red);
            }
            else
            {
                TangiblePattern pattern = new TangiblePattern { type = type };

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

                if (debugType == 1)
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
                        DebugText("Rotation angle: " + angle);
                    }
                }

                // infopoint(s) should be above base points
                for (int i = 0; i < patternPoints.Count; i++)
                    if (i != anchorPair.first && i != anchorPair.second)
                    {
                        if (patternPoints[i].y < patternPoints[anchorPair.first].y)
                        {
                            flipPattern(patternPoints, anchorPair);
                        }
                    }

                // make sure the most left base point is point one 
                if (patternPoints[anchorPair.first].x > patternPoints[anchorPair.second].x)
                {
                    int tmp = anchorPair.first;
                    anchorPair.first = anchorPair.second;
                    anchorPair.second = tmp;
                }

                // get info (grid-)coordinates as Tuple<int, int>
                Tuple<int, int> infoCoord = new Tuple<int, int>(0, 0);
                for (int i = 0; i < patternPoints.Count; i++)
                    if (i != anchorPair.first && i != anchorPair.second)
                    {
                        float rawX = patternPoints[i].x - patternPoints[anchorPair.first].x;
                        float rawY = patternPoints[i].y - patternPoints[anchorPair.first].y;
                        int coordX = Mathf.RoundToInt(rawX / (minDist / 2));
                        int coordY = Mathf.RoundToInt(rawY / (minDist / 2));
                        if (coordY == 0 && coordX < 0)
                        { // special case: if pattern is linear then the info point should have positive coordinates
                            coordX = 2 - coordX; // e.g. (-3,0) -> (5,0)
                        }
                        infoCoord.first = coordX;
                        infoCoord.second = coordY;
                    }


                // put coords in 'pattern' and add 'pattern' to 'patterns'
                if (infoCoord.first == 0 && infoCoord.second == 0)
                {
                    InfoText("Pattern does not contain enought information!", true, Color.red);
                }
                else
                {
                    InfoText("Trainig successful! Info point coordinate is (" + infoCoord.first + "," + infoCoord.second + ")", true, Color.green);

                    pattern.id = patternId;
                    pattern.infoCoord = infoCoord;

                    patterns.Add(pattern);
                    drawOutline(patternPoints);
                }
            }
        }

        #region CapacitiveTangiblesTrainer Button functions + other GUI elements
        public void InfoText(string text, bool append = false, Color? color = null)
        {
            infoText.color = color ?? Color.green;
            infoText.text = append ? infoText.text+text: text;
        }

        public void DebugText(string text, bool append = false, Color? color = null)
        {
            infoText.color = color ?? Color.green;
            infoText.text = append ? infoText.text + text : text;
        }

        public void ToggleOutline()
        {
            if (this.outline)
                outline = false;
            else
                outline = true;
        }

        public void TrainPlainPattern() {
            TrainPattern(TangiblePattern.Type.PLAIN);
        }

        public void TrainUDPPattern() {
            TrainPattern(TangiblePattern.Type.UDP);
        }

		public void SavePattern()
		{
            if (patternId == -1)
            {
                InfoText("PatternID not set! >:-(", false, Color.red);
                return;
            }
            InfoText("saving...");
            TangiblePattern pattern = new TangiblePattern
            {
                id = patternId
            };
            
            List<Tuple<int, int>> infoCoords = new List<Tuple<int, int>>();

			for (int i = 0; i < patterns.Count; i++)
			{
				TangiblePattern ptn = patterns[i];
				infoCoords.Add(ptn.infoCoord);
			}

            InfoText("\nchecking samples...",true);
			// check if samples match 
            if(infoCoords.Count <= 0)
            {
                InfoText("There are no training samples!!! >:-(", false, Color.red);
                return;
            }
			bool match = true;
			for (int i = 0; i < infoCoords.Count - 1; i++)
				if (infoCoords[i].first != infoCoords[i + 1].first
					|| infoCoords[i].second != infoCoords[i + 1].second)
					match = false;

			if (match)
			{
				pattern.infoCoord = infoCoords[0];
				InfoText(pattern.ToString());

                #region CapacitiveTangiblesTrainer saveToJson
                try
                {
                    string json = JsonUtility.ToJson(pattern, true);
                    string fullfilepath = TangiblesFileUtils.PatternFilename(patternId.ToString());
                    print(fullfilepath);
                    File.WriteAllText(fullfilepath, json);
                    InfoText("\nSuccess: pattern saved B-)", true);
                }
                catch (System.Exception e)
                {
                    string error = e.GetType().ToString();
                    InfoText("An error occured while trying to save the pattern: "+error, true, Color.red);
                }
                #endregion

                
            }
            else
			{
				InfoText("There are multiple unmatching training samples! :'(", false, Color.red);
			}

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
            InfoText("Patternpoints cleared! :)");
		}

        public void RemoveLastTrainedPattern()
		{
			if (monitorPatterns.Count > 0)
			{
				Destroy(monitorPatterns[monitorPatterns.Count - 1]);
				monitorPatterns.RemoveAt(monitorPatterns.Count - 1);
				patterns.RemoveAt(patterns.Count - 1);
                InfoText("Last pattern removed! :)");
            }
            else
            {
                InfoText("Nothing to do! :D");
            }
		}
        #endregion

        #region CapacitiveTangiblesTrainer getters&setters
        public void SetPatternID()
		        {
			        int id;
			        if (int.TryParse(patternIdText.text, out id))
			        {
				        patternId = id;
			        }
		        }

		public void SetDebugType()
		{
			this.debugType = debugTypeList.value;
		}
        #endregion
    }
}