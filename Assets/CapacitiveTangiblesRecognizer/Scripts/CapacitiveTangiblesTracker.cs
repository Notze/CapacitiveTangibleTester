using CTR;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CTR
{
    public class CapacitiveTangiblesTracker : CTRBasicFunctionality
    {
        public bool debug = false;

        public TangiblePattern.Type mode;
        public GameObject tangibleOutlinePrefab;
        public Toggle mockUpToggle;

        bool mockUp = false;

        //OSCReceiver oscReceiver;
        string logPathAndFilename; // this is handled by the Log function
        List<TangiblePattern> patterns;
        Dictionary<string, TangiblePattern> patternDictUDP;
        Dictionary<string, TangiblePattern> patternDictPlain;

        // Use this for initialization
        void Start()
        {
            //oscReceiver = (new GameObject("OSCReceiver container")).AddComponent<OSCReceiver>();
            //oscReceiver.osc = osc;
            //basicFunctions = (new GameObject("basicFunctionalityContainer")).AddComponent<CTRBasicFunctionality>() as CTRBasicFunctionality; //new CTRBasicFunctionality(transform as RectTransform);
            //basicFunctions.rectTransform = transform as RectTransform;
            patternDictUDP = LoadPatternDict(TangiblePattern.Type.UDP);
            patternDictPlain = LoadPatternDict(TangiblePattern.Type.PLAIN);
            //outlineManager = OutlineManager.Instance; //(new GameObject("outlineManagerContainer").AddComponent<OutlineManager>()) as OutlineManager;
            OutlineManager.Initialize(transform as RectTransform, tangibleOutlinePrefab);
            foreach (TangiblePattern pattern in patternDictPlain.Values)
                OutlineManager.InstantiateOutline(pattern);
            DebugText(patternDictPlain.Count.ToString() + " Plain-patterns loaded.");
            foreach (TangiblePattern pattern in patternDictUDP.Values)
                OutlineManager.InstantiateOutline(pattern);
            DebugText(patternDictUDP.Count.ToString()+" UDP-patterns loaded.", true);

            infoTextField.text = "";
        }

        // Update is called once per frame
        void Update() // continuous tracking 
        {
            TangiblePattern? recognition = RecognizePattern(mode, mockUp);
            if (recognition != null)
            {
                TangiblePattern pattern = (TangiblePattern) recognition;
                if (debug) print("calling updateOutline for " + pattern.ToString(true));
                OutlineManager.updateOutlinePosition(pattern);
                InfoText("Found Pattern with ID " + pattern.infoCoord.ToString() + " at " + pattern.Position.ToString() + " oriented to " + pattern.orientation.ToString());
            }
        }

        #region UI elements

        public void PrintKnownUDPPatterns()
        {
            PrintKnownPatterns(TangiblePattern.Type.UDP);
        }

        public void PrintKnownPlainPatterns()
        {
            PrintKnownPatterns(TangiblePattern.Type.PLAIN);
        }

        public void ToggleDebug()
        {
            debugTextToggle = debugTextToggle ? false : true;
        }

        public void ToggleMockUp()
        {
            mockUp = mockUp ? false : true;
        }

        public void toggleMode()
        {

            mode = (mode == TangiblePattern.Type.PLAIN) ? TangiblePattern.Type.UDP : TangiblePattern.Type.PLAIN;
            DebugText(mode.ToString() + " mode");
        }

        #endregion

        public void PrintKnownPatterns(TangiblePattern.Type type)
        {
            switch (type)
            {
                case TangiblePattern.Type.PLAIN:
                    foreach (KeyValuePair<string, TangiblePattern> pair in patternDictPlain)
                        DebugText(pair.Value.ToString());
                    break;
                case TangiblePattern.Type.UDP:
                    foreach (KeyValuePair<string, TangiblePattern> pair in patternDictUDP)
                        DebugText(pair.Value.ToString());
                    break;
            }
        }

        // Fills a persistent logfile.
        void LogFile(string logText)
        {
            // if there is no log file create a new logfile with timestamp TODO
            // write data to logfile TODO
        }

    }

}
