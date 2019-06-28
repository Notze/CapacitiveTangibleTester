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
        public bool debugBasicFunctionality = true;

        public RectTransform rectTransform;
        public OSC osc;

        public string textPrefix; // (optional) prefix for info and debug text messages is set in the class that uses logtexts and should be set to the class name
        public Text debugTextField;
        public Text infoTextField;
        public Slider logLengthSlider;
        public bool infoTextToggle = true;
        public bool debugTextToggle = true;
        int logTextsMaxCapacity = 40; // size of log text fields
        int infoTextMaxEntries = 5; // current log length
        int debugTextMaxEntries = 5; // current log length
        List<string> infoText = new List<string>();
        List<string> debugText = new List<string>();


        #region OSC Receiver
        OSCReceiver oscReceiver;
        string _oscTangibleID;
        public string OSCTangibleID { get
            {
                if (oscReceiver == null || !oscReceiver.isActiveAndEnabled)
                    InitializeOSC();
                return _oscTangibleID;
            } }

        private void InitializeOSC()
        {
            oscReceiver = (new GameObject("OSCReceiver container")).AddComponent<OSCReceiver>();
            oscReceiver.osc = osc;
            osc.SetAddressHandler("/ID", oscOnReceiveID);
            if (debugBasicFunctionality) print("OSCReceiver set up");
        }

        private void oscOnReceiveID(OscMessage message)
        {
            _oscTangibleID = message.GetFloat(0).ToString();
            if (debugBasicFunctionality) print("OSC: Tangible ID received");
        }
        #endregion

        public Dictionary<string, TangiblePattern> LoadPatternDict(TangiblePattern.Type? type = null) {
            Dictionary<string, TangiblePattern> dict = new Dictionary<string, TangiblePattern>();

            foreach (TangiblePattern pattern in LoadPatterns(type))
                dict.Add(pattern.id, pattern);

            return dict;
        }

        public void DebugText(string text, bool clear = false, Color? color = null)
        {
            if (debugTextToggle)
            {
                if (clear)
                    debugText.Clear();
                while (debugText.Count >= debugTextMaxEntries)
                    debugText.RemoveAt(0);
                if (textPrefix != null)
                    text = "[" + textPrefix + "] " + text;
                debugText.Add(text);

                debugTextField.text = "";
                debugTextField.color = color ?? Color.green;
                foreach (string message in debugText)
                    debugTextField.text += message + "\n";
            }
        }

        public void InfoText(string text, bool clear = false, Color? color = null)
        {
            if (infoTextToggle)
            {
                if (clear)
                    infoText.Clear();
                while (infoText.Count >= infoTextMaxEntries)
                    infoText.RemoveAt(0);
                if (textPrefix != null)
                    text = "[" + textPrefix + "] " + text;
                infoText.Add(text);

                infoTextField.text = "";
                infoTextField.color = color ?? Color.green;
                foreach (string message in infoText)
                    infoTextField.text += "\n" + message;
            }
        }

        public void SetLogLength()
        {
            int newLength = (int)(logLengthSlider.value * logTextsMaxCapacity);
            debugTextMaxEntries = (newLength > 0) ? newLength : 1;
            infoTextMaxEntries = (newLength > 0) ? newLength : 1;
        }

        // Removes all persistent Patterns.
        public void ClearPatternStorage()
        {
            try
            {
                TangiblesFileUtils.DeleteTangibles();
            }
            catch (Exception e)
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
            catch (Exception e)
            {
                string error = e.GetType().ToString();
                return 1;
            }
        }

        // loads previously stored tangible patterns
        public List<TangiblePattern> LoadPatterns(TangiblePattern.Type? type = null)
        {
            List<TangiblePattern> list = new List<TangiblePattern>();

            string[] filenames = TangiblesFileUtils.LoadTangiblesJSON();
            if (filenames != null)
                foreach (string filename in filenames)
                {
                    string json = File.ReadAllText(filename);
                    TangiblePattern pattern = JsonUtility.FromJson<TangiblePattern>(json);
                    if(type == null)
                        list.Add(pattern);
                    else
                        if (pattern.type == type)
                            list.Add(pattern);
                }

            return list;
        }

        // Analyses touch input and returns type- and nameless pattern if any.
        public TangiblePattern? RecognizePattern( TangiblePattern.Type type, bool doClustering = false, bool mockUp = false )
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

            DebugText("Touchpoint count: " + patternPoints.Count);

            float minDist = 0; // double grid (cell) size value of tangible
            if (patternPoints.Count < 3) return null;
            if (patternPoints.Count == 3 || !doClustering)
                patternPoints = MathHelper.SortPatternPointsByDistance(patternPoints, out minDist);
            else // patternPoints.Count > 3 && doClustering
                patternPoints = MathHelper.FindMindDistTriplet(patternPoints, out minDist);


            if (patternPoints == null) return null;
            float gridSize = minDist / 2;
            TangiblePattern pattern = new TangiblePattern { type = type };

            if (type == TangiblePattern.Type.UDP)
                if (OSCTangibleID!=null)
                    pattern._id = OSCTangibleID;
                else
                    return null;

            // closest points are "base" of pattern and are used to determine 
            // grid (cell) size and rotation:

            // rotate to align "base" horizontally
            Vector2 anchorVector = new Vector2(
                patternPoints[0].x - patternPoints[1].x,
                patternPoints[0].y - patternPoints[1].y);
            float patternAngle = Vector2.SignedAngle(new Vector2(1, 0), anchorVector);
            Vector3 rotationVector = new Vector3(0, 0, -patternAngle);

            //for (int i = 0; i < patternPoints.Count; i++)
            //    if (i != anchorPair.first && i != anchorPair.second)
            //        idIndex = i;

            // fill pattern with original touchpoint data
            pattern.anchorPoint1 = patternPoints[0];
            pattern.anchorPoint2 = patternPoints[1];
            pattern.idPoint = patternPoints[2];

            // continue 'rotate to align "base" horizontally'
            for (int i = 1; i < patternPoints.Count; i++)
                patternPoints[i] = MathHelper.RotatePointAroundPivot(patternPoints[i], patternPoints[0], rotationVector);

            // infopoint should be above base points
            if (patternPoints[2].y < patternPoints[0].y)
            {
                FlipPattern(patternPoints);
                patternAngle = (patternAngle + 180f) % 360;
            }

            // make sure the most left base point is first point
            if (patternPoints[0].x > patternPoints[1].x)
            {
                // change original touchpoints stored in pattern
                Vector2 tmpV = pattern.anchorPoint1;
                pattern.anchorPoint1 = pattern.anchorPoint2;
                pattern.anchorPoint2 = tmpV;
                // change rotated points
                tmpV = patternPoints[0];
                patternPoints[0] = patternPoints[1];
                patternPoints[1] = tmpV;
            }

            // get info (grid-)coordinates as Tuple<int, int>
            Tuple<int, int> infoCoord = new Tuple<int, int>(0, 0);
            float rawX = patternPoints[2].x - patternPoints[0].x;
            float rawY = patternPoints[2].y - patternPoints[0].y;
            int coordX = Mathf.RoundToInt(rawX / gridSize);
            int coordY = Mathf.RoundToInt(rawY / gridSize);
            if (coordY == 0 && coordX < 0)
            { // special case: if pattern is 'linear pattern' then the info point should have positive coordinates
                coordX = 2 - coordX; // e.g. (-3,0) -> (5,0)
            }
            infoCoord.first = coordX;
            infoCoord.second = coordY;

            if (infoCoord.first == 0 && infoCoord.second == 0)
                return null;
            else
            {
                // for evaluation purposes the angle of pattern should always be displayed as a positive value
                if (patternAngle < 0) patternAngle = 360 + patternAngle;

                pattern.infoCoord = infoCoord;
                pattern.gridSize = gridSize;
                pattern.orientation = patternAngle;

                return pattern;
            }
        }

        // Rotates patternPoints by 180°.
        private void FlipPattern(List<Vector2> patternPoints)
        {
            for (int i = 1; i < patternPoints.Count; i++)
                patternPoints[i] = MathHelper.RotatePointAroundPivot(patternPoints[i], patternPoints[0], new Vector3(0, 0, 180));
        }

    }
}
