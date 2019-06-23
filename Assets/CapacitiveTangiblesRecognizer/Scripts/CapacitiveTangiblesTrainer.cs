using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

namespace CTR
{
	public class CapacitiveTangiblesTrainer : CTRBasicFunctionality
	{
        public bool debug = false;
		public bool outline = true;
        public TangiblePattern.Type mode;

		public GameObject patternPrefab;
		public GameObject patternFootPrefab;
        public GameObject outlinePrefab;
		public GameObject touchPointPrefab;
		public Text infoText;
		public Text debugText;
		public Dropdown debugTypeList;


        // 0 = none
        // 1 = mark anchor and info
        // 2 = show rotated points before flip
        // 3 = show rotated points after flip
        // 4 = fake tangible
        int debugType = 3;
        float gridSize;
		List<TangiblePattern> patterns = new List<TangiblePattern>();
		List<GameObject> patternPointsVisuals;
		List<GameObject> monitorPatterns;
        static TangiblePattern pattern;


		void Start()
		{
			rectTransform = transform as RectTransform;
			patternPointsVisuals = new List<GameObject>();
			monitorPatterns = new List<GameObject>();
            OutlineManager.Initialize(rectTransform, outlinePrefab);
		}

        // Creates a visual representation of a touch.
		void CreateTouchPoint(Vector2 screenPos, Color color)
		{
            GameObject touchPoint = Instantiate(touchPointPrefab);
            RectTransform rt = (touchPoint.transform as RectTransform);
            rt.position = screenPos;
            rt.SetParent(rectTransform);
            touchPoint.GetComponent<Image>().color = color;
            patternPointsVisuals.Add(touchPoint);
            if (debugType > 0) DebugText("Point vector: " + screenPos.ToString(), true);
        }

        // Fills the InfoTextField.
        public void InfoText(string text, bool append = false, Color? color = null)
        {
            infoText.color = color ?? Color.green;
            infoText.text = append ? infoText.text + "\n" + text: text;
        }

        // Fills the DebugTextField.
        public void DebugText(string text, bool append = false, Color? color = null)
        {
            if (debugType > 0)
            {
                debugText.color = color ?? Color.green;
                debugText.text = append ? debugText.text + "\n" + text : text;
            }
        }

        #region UI elements

        public void TrainPattern()
        {
            TangiblePattern? recognition = RecognizePattern(mode, (debugType == 4));
            if (recognition != null)
                pattern = (TangiblePattern) recognition;
            InfoText(pattern.ToString());
            OutlineManager.InstantiateOutline(pattern);
            OutlineManager.updateOutlinePosition(pattern);
        }

		public void SavePattern()
		{
            if (SavePattern(pattern) == 0)
                InfoText("Success: pattern saved", true);
            else
                InfoText("ERROR: saving pattern failed", true, Color.red);
        }

        public void Clear()
		{
            OutlineManager.Clear();
		}

		public void SetDebugType()
		{
			this.debugType = debugTypeList.value;
            if (debugType <= 0) debugText.text = "";
		}

        #endregion
    }
}