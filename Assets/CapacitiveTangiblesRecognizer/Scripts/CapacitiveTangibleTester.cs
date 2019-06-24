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

        public GameObject patternPrefab;
        public GameObject outlinePrefab;

        float gridSize;
        List<TangiblePattern> patterns = new List<TangiblePattern>();
        List<GameObject> monitorPatterns;
        Dictionary<string,TangiblePattern> recognizedPatternDict;
        bool mockUp = false;
        bool testing = false;
        Dictionary<string, TangiblePattern> knownPatternDictUDP;
        Dictionary<string, TangiblePattern> knownPatternDictPlain;
        List<string> testLog;

        enum LogMessageType { CONFIRMATION, RECOGNITION };


        void Start()
        {
            rectTransform = transform as RectTransform;
            OutlineManager.Initialize(rectTransform, outlinePrefab);

            knownPatternDictUDP = LoadPatternDict(TangiblePattern.Type.UDP);
            knownPatternDictPlain = LoadPatternDict(TangiblePattern.Type.PLAIN);
            recognizedPatternDict = new Dictionary<string, TangiblePattern>();

        }

        void Update() // continuous tracking 
        {
            TangiblePattern? recognition = RecognizePattern(mode, mockUp);
            if (recognition != null)
            {
                TangiblePattern pattern = (TangiblePattern)recognition;
                Testlog(pattern.ToLogString(),LogMessageType.RECOGNITION); // log recognized position

                OutlineManager.updateOutlinePosition(pattern);
                InfoText("Found Pattern with ID " + pattern.infoCoord.ToString() + " at " + pattern.Position.ToString() + " oriented to " + pattern.orientation.ToString());
            }
        }

        // Executes one Testcycle
        public void StartTestrun()
        {
            testing = true;

            // TODO

            testing = false;
        }

        // Places the corresponding Outline to a given pattern to a random 
        // position within the boundaries of the recognition panel.
        public void RadomizeOutlinePosition(TangiblePattern pattern)
        {
            // minDist to screen edges, other tangibles and new pattern positions
        }

        // Pushes entries to logfile.
        private void Testlog(string message, LogMessageType type)
        {
            testLog.Add("[" + type.ToString() + "]" + message);
        }

        // Makes log data persistent.
        public void SaveLogfile()
        {
            //TODO
        }

        #region UI elements

        public void ConfirmTangiblesArePositioned()
        {
            // TODO
            // save position and orientation of recognition and targets
            // set assumed positions to target positions
            // randomize target positions
            // repeat
        }

        public void ToggleMockUp()
        {
            mockUp = mockUp ? false : true;
        }

        // TODO will be removed later
        public void TrainPattern()
        {
            Clear();

            TangiblePattern? recognition = RecognizePattern(mode, mockUp);
            if (recognition != null)
            {
                TangiblePattern pattern = (TangiblePattern)recognition;
                InfoText(pattern.ToString(), true);
                OutlineManager.InstantiateOutline(pattern);
                OutlineManager.updateOutlinePosition(pattern);
            }
        }

        public void Clear()
        {
            OutlineManager.Clear();
            InfoText("cleared", true);
        }

        #endregion
    }

}