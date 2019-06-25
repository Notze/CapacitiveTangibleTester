using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CTR
{

    public class CapacitiveTangibleTester : CTRBasicFunctionality
    {

        public bool debug = false;
        public bool outline = true;
        public TangiblePattern.Type mode;
        public static int testCycles = 10;
        
        public GameObject outlinePrefab;
        public Button startButton;

        float gridSize;
        List<TangiblePattern> patterns = new List<TangiblePattern>();
        List<GameObject> monitorPatterns;
        Dictionary<string, TangiblePattern> recognizedPatternDict;
        List<Vector2> outlinePositionSnapshot;
        bool mockUp = false;
        bool testing = false;
        string currentTestingLogfile;
        float panelWidth;
        float panelHeight;

        enum LogMessageType { CONFIRMATION, RECOGNITION, ERROR, UNREGISTERED, INIT };


        void Start()
        {
            debugTextField.text = "";
            infoTextField.text = "";
            rectTransform = transform as RectTransform;
            panelWidth = rectTransform.rect.width;
            panelHeight = rectTransform.rect.height;
            OutlineManager.Initialize(rectTransform, outlinePrefab);
            textPrefix = "CTTester";
            
            recognizedPatternDict = LoadPatternDict(mode);
            DebugText(recognizedPatternDict.Count.ToString() + " " + mode.ToString() + " patterns loaded.");
            foreach(TangiblePattern pattern in recognizedPatternDict.Values)
                OutlineManager.InstantiateOutline(pattern);

            RandomizeOutlinePositions();

        }

        void Update() // continuous tracking 
        {
            TangiblePattern? recognition = RecognizePattern(mode, mockUp);
            if (recognition != null)
            {
                TangiblePattern pattern = (TangiblePattern)recognition;
                Testlog(pattern.ToLogString(),LogMessageType.RECOGNITION); // log recognized position

                if (recognizedPatternDict.ContainsKey(pattern.id))
                {
                    recognizedPatternDict[pattern.id] = pattern;
                    Testlog(pattern.ToLogString(), LogMessageType.RECOGNITION);
                }
                else
                {
                    Testlog(pattern.ToLogString(), LogMessageType.UNREGISTERED);
                }
                InfoText("Found Pattern with ID " + pattern.infoCoord.ToString() + " at " + pattern.Position.ToString() + " oriented to " + pattern.orientation.ToString());
            }
        }


        // Randomizes the position of the corresponding outlines for all known patterns.
        public void RandomizeOutlinePositions()
        {
            outlinePositionSnapshot = OutlineManager.OutlinePositions;
            if (debug)
                foreach (Vector2 v in outlinePositionSnapshot)
                    print(v.ToString());
            foreach (TangiblePattern pattern in recognizedPatternDict.Values)
                RandomizeOutlinePosition(pattern);
        }

        // Returns true if the given position keeps the minimal distance to all
        // other patterns. Assuming the actual tangibles are positioned on 
        // their corresponding outlines.
        // Flase otherwise.
        bool IsGoodPosition(Vector2 newPosition, float minDist)
        {
            // new outlines should not overlap each other
            foreach (Vector2 pos in OutlineManager.OutlinePositions)
                if ((newPosition - pos).magnitude < minDist)
                    return false;
            // neither should they appear under a set tangible
            foreach (Vector2 pos in outlinePositionSnapshot)
                if ((newPosition - pos).magnitude < minDist)
                    return false;

            return true;
        }

        // Places the corresponding Outline to a given pattern to a random 
        // position within the boundaries of the recognition panel.
        // CAUTION! if there are too many outlines to fit on the screen so that
        // no good positions can be found after a fixed number of tries 
        // overlapping is accepted to prevent a crash
        private void RandomizeOutlinePosition(TangiblePattern pattern)
        {
            // minDist to screen edges, other tangibles and new pattern positions
            // since this value should describe the distance from the pattern 
            // anchorpoint to the outmost edge of the outline which is a 
            // diagonal line the outline height is pretty close to the desired 
            // value
            float minDist = pattern.OutlineHeight * 1.2f; 

            float randA = Random.value;
            float newAngle = 360f * randA;
            Vector2 newPosition;
            int maxTries = 100000; // dead loop prevention
            int tries = 0; // dead loop prevention
            do {
                float randX = Random.value;
                float randY = Random.value;

                newPosition = new Vector2(
                    minDist + (panelWidth - 2 * minDist) * randX,
                    minDist + (panelHeight - 2 * minDist) * randY);

                tries++; // dead loop prevention
                if (tries >= maxTries) // dead loop prevention
                {
                    Testlog("unable to find good tangible positions", LogMessageType.ERROR);
                    break;
                }
            } while (!IsGoodPosition(newPosition, minDist*2)); // 2 x minDist because this is between two patterns

            OutlineManager.SetOutlinePosition(pattern.id, newPosition, newAngle);

            DebugText("new position: " + newPosition.ToString() + " "+ newAngle.ToString());

        }

        // Pushes entries to logfile.
        private void Testlog(string message, LogMessageType type, bool createNewFile = false)
        {
            // Recognized tangible positions should be logged continously while
            // distances between recognized positions and target positions should
            // be logged when the test proband confirms the placement.

            string typeString = "";
            switch (type)
            {
                case LogMessageType.CONFIRMATION:
                    typeString = "[CONFIRMATION] ";
                    break;
                case LogMessageType.ERROR:
                    typeString = "[ERROR] ";
                    break;
                case LogMessageType.RECOGNITION:
                    typeString = "[RECOGNITION] ";
                    break;
                case LogMessageType.UNREGISTERED:
                    typeString = "[UNREGISTERED] ";
                    break;
                case LogMessageType.INIT:
                    typeString = "[INIT] ";
                    break;
            }
            if (testing) SaveToLogfile(typeString + message, createNewFile);
            if (debug) print(typeString + message);
        }

        // Writes test log data to a file.
        public void SaveToLogfile(string message, bool createNewFile = false)
        {
            if (createNewFile || currentTestingLogfile == null)
            {
                currentTestingLogfile = TangiblesFileUtils.TestingFilename();
                if (debug) print(currentTestingLogfile);
            }

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(currentTestingLogfile, true))
            {
                file.WriteLine(message);
            }
        }

        #region UI elements

        public void StopTestrun()
        {
            if (!testing) return;
            Testlog("End of Test", LogMessageType.INIT);
            testing = false;
            startButton.gameObject.SetActive(true);
            logLengthSlider.gameObject.SetActive(true);
        }

        public void ConfirmTangiblesArePositioned()
        {
            // log deltas of position and orientation for recognition and targets
            foreach (TangiblePattern pattern in recognizedPatternDict.Values)
            {
                RectTransform ort = OutlineManager.outlineDict[pattern.id].transform as RectTransform;
                Vector2 targetPosition = (Vector2)ort.position;
                float targetOrientation = ort.eulerAngles.z;
                float deltaPosition = (pattern.Position - targetPosition).magnitude;
                float deltaOrientation = pattern.orientation - targetOrientation;
                Testlog(pattern.id + ";" + targetPosition.ToString() + ":" + deltaPosition.ToString() + ";" + targetOrientation.ToString() + ":" + deltaOrientation.ToString(), LogMessageType.CONFIRMATION);
            }

            RandomizeOutlinePositions();

            if(testing)
                if (testCycles > 0)
                {
                    testCycles--;
                }
                else
                {
                    StopTestrun();
                }
        }

        public void ToggleMockUp()
        {
            mockUp = mockUp ? false : true;
        }

        public void Clear()
        {
            OutlineManager.Clear();
            InfoText("cleared", true);
        }

        // Initializes one testrun.
        public void StartTestrun()
        {
            testing = true;
            startButton.gameObject.SetActive(false);
            logLengthSlider.gameObject.SetActive(false);
            Testlog(panelWidth + ":" + panelHeight, LogMessageType.INIT, true);
        }

        #endregion
    }

}