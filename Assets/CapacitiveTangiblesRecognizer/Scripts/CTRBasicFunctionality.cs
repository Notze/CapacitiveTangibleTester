using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace CTR
{
    public class CTRBasicFunctionality : MonoBehaviour
    {
        public bool debugBasicFunctionality = false;

        public RectTransform rectTransform;


        public Dictionary<string, TangiblePattern> LoadPatternDict(TangiblePattern.Type? type = null) {
            Dictionary<string, TangiblePattern> dict = new Dictionary<string, TangiblePattern>();

            foreach (TangiblePattern pattern in LoadPatterns())
                switch (type)
                {
                    case TangiblePattern.Type.PLAIN:
                        if (pattern.type == TangiblePattern.Type.PLAIN)
                            dict.Add(pattern.id, pattern);
                        break;
                    case TangiblePattern.Type.UDP:
                        if (pattern.type == TangiblePattern.Type.UDP)
                            dict.Add(pattern.id, pattern);
                        break;
                    case null:
                        dict.Add(pattern.id, pattern);
                        break;
                }

            return dict;
        }

        // Removes all persistent Patterns.
        public void ClearPatternStorage()
        {
            try
            {
                TangiblesFileUtils.DeleteTangibles();
            }
            catch (System.Exception e)
            {

            }
        }

        // Makes a given Pattern persistent.
        // Returns 0 on success, 1 otherwise.
        public int SavePattern(TangiblePattern pattern)
        {
            try
            {
                string json = JsonUtility.ToJson(pattern, true);
                string fullfilepath = TangiblesFileUtils.PatternFilename(pattern.id.ToString());
                if (debugBasicFunctionality) print(fullfilepath);
                File.WriteAllText(fullfilepath, json);
                return 0;
            }
            catch (System.Exception e)
            {
                string error = e.GetType().ToString();
                return 1;
            }
        }

        // loads previously stored tangible patterns
        public List<TangiblePattern> LoadPatterns()
        {
            List<TangiblePattern> list = new List<TangiblePattern>();

            string[] filenames = TangiblesFileUtils.LoadTangiblesJSON();
            if (filenames != null)
                foreach (string filename in filenames)
                {
                    string json = File.ReadAllText(filename);
                    TangiblePattern pattern = JsonUtility.FromJson<TangiblePattern>(json);
                    list.Add(pattern);
                }

            return list;
        }

        // Analyses touch input and returns type- and nameless pattern if any.
        // TODO type auswerten bzw. udp pattern implementieren
        public Nullable<TangiblePattern> RecognizePattern( TangiblePattern.Type type, bool mockUp = false )
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

            if (mockUp) // Mock-Up
            {
                Vector2 middleOfScreen = Camera.main.ViewportToScreenPoint(new Vector3(.5f, .5f, 0f)); // base1
                Vector2 point2 = middleOfScreen + new Vector2(30f, +30f); // base2
                Vector2 point3 = middleOfScreen + new Vector2(-30f, 100f); // id
                patternPoints.Add(middleOfScreen);
                patternPoints.Add(point2);
                patternPoints.Add(point3);
            }

            // find the two closest points
            Vector2 patternCenter = MathHelper.ComputeCenter(patternPoints);
            Vector2 panelCenter = MathHelper.RectTransformToScreenSpace(rectTransform).center;
            Vector2 centerOffset = patternCenter - panelCenter;
            if (debugBasicFunctionality) print("panelCenter: " + panelCenter);
            if (debugBasicFunctionality) print("patternCenter: " + patternCenter);
            if (debugBasicFunctionality) print("centerOffset: " + centerOffset);
            float minDist = 0; // double grid (cell) size value
            Tuple<int, int> anchorPair = MathHelper.FindMinDistancePair(patternPoints, patternCenter, out minDist);
            int idIndex = -1;
            float gridSize = minDist / 2;
            if (anchorPair.first == -1 || anchorPair.second == -1)
                return null;
            else
            {
                TangiblePattern pattern = new TangiblePattern { };

                // closest points are "base" of pattern and determine grid (cell) size and rotation
                // rotate to align "base" horizontally
                Vector2 anchorVector = new Vector2(
                    patternPoints[anchorPair.first].x - patternPoints[anchorPair.second].x,
                    patternPoints[anchorPair.first].y - patternPoints[anchorPair.second].y);
                float angle = -Vector2.SignedAngle(new Vector2(1, 0), anchorVector);
                Vector3 rotation = new Vector3(0, 0, angle);

                for (int i = 0; i < patternPoints.Count; i++)
                    if (i != anchorPair.first && i != anchorPair.second)
                        idIndex = i;

                // fill pattern with data
                pattern.anchorPoint1 = patternPoints[anchorPair.first];
                pattern.anchorPoint2 = patternPoints[anchorPair.second];
                pattern.idPoint = patternPoints[idIndex];

                // continue 'rotate to align "base" horizontally'
                for (int i = 0; i < patternPoints.Count; i++)
                    patternPoints[i] = MathHelper.RotatePointAroundPivot(patternPoints[i], patternPoints[anchorPair.first], rotation);

                // infopoint(s) should be above base points
                if (patternPoints[idIndex].y < patternPoints[anchorPair.first].y)
                {
                    FlipPattern(patternPoints, anchorPair);
                    angle += 180f;
                }

                // make sure the most left base point is point one 
                if (patternPoints[anchorPair.first].x > patternPoints[anchorPair.second].x)
                {
                    int tmpI = anchorPair.first;
                    anchorPair.first = anchorPair.second;
                    anchorPair.second = tmpI;

                    Vector2 tmpV = pattern.anchorPoint1;
                    pattern.anchorPoint1 = pattern.anchorPoint2;
                    pattern.anchorPoint2 = tmpV;
                }

                // get info (grid-)coordinates as Tuple<int, int>
                Tuple<int, int> infoCoord = new Tuple<int, int>(0, 0);
                for (int i = 0; i < patternPoints.Count; i++)
                    if (i != anchorPair.first && i != anchorPair.second)
                    {
                        float rawX = patternPoints[i].x - patternPoints[anchorPair.first].x;
                        float rawY = patternPoints[i].y - patternPoints[anchorPair.first].y;
                        int coordX = Mathf.RoundToInt(rawX / gridSize);
                        int coordY = Mathf.RoundToInt(rawY / gridSize);
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
                    return null;
                }
                else
                {
                    pattern.infoCoord = infoCoord;
                    pattern.gridSize = gridSize;
                    pattern.orientation = -angle;

                    return pattern;
                }
            }
        }

        // Rotates patternPoints by 180°.
        private void FlipPattern(List<Vector2> patternPoints, Tuple<int, int> anchorPair)
        {
            for (int j = 0; j < patternPoints.Count; j++)
                patternPoints[j] = MathHelper.RotatePointAroundPivot(patternPoints[j], patternPoints[anchorPair.first], new Vector3(0, 0, 180));

        }

    }
}
