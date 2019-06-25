using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CTR
{
    public class OutlineManager : MonoBehaviour
    {
        static bool debug = true;

        static RectTransform rectTransform;
        static GameObject tangibleOutlinePrefab;

        //// hardcoded configuration for the current tangible layout
        //static float padD = 24; // adjust this so the outline is extended far enough
        //static int gridWidth = 5; // pattern grid dimensions of physical tangibles
        //static int gridHeight = 6;

        public static Dictionary<string, GameObject> outlineDict = new Dictionary<string, GameObject>();
        public static List<Vector2> OutlinePositions { get {
                List<Vector2> output = new List<Vector2>();
                foreach (GameObject p in outlineDict.Values)
                    output.Add((Vector2) p.transform.position);
                return output; } }
        


        // Removes all Outline Prefabs and clears the Outline Dictionary.
        public static void Clear()
        {
            foreach (GameObject o in outlineDict.Values)
                Destroy(o);
            outlineDict.Clear();
        }

        // Initialize should be called when the scene changes. In which case 
        // all previously referenced objects already have been destroyed.
        public static void Initialize(RectTransform rt, GameObject top)
        {
            rectTransform = rt;
            tangibleOutlinePrefab = top;
            outlineDict.Clear();
        }

        // Moves the correspoding Outline object for a given pattern.
        // The Outline is set to the position of the pattern.
        public static void updateOutlinePosition(TangiblePattern pattern)
        {
            if(debug) print("UpdateOutline invoced");
            if (outlineDict.ContainsKey(pattern.id))
            {
                GameObject outline = outlineDict[pattern.id];
                outline.SetActive(true);
                outline.transform.position = pattern.Position;
                outline.transform.eulerAngles = new Vector3(0,0,pattern.orientation);
                if (debug) print("UpdateOutline: outline position updated: " + pattern.id.ToString() + pattern.Position.ToString() + pattern.orientation.ToString());
            }
        }

        // Moves the correspoding Outline object for a given pattern.
        // The Outline is set to a given position and orientation.
        public static void SetOutlinePosition(string patternID, Vector2 position, float orientation)
        {
            if (debug) print("MoveOutline invoced");
            if (outlineDict.ContainsKey(patternID))
            {
                GameObject outline = outlineDict[patternID];
                outline.SetActive(true);
                outline.transform.position = position;
                outline.transform.eulerAngles = new Vector3(0, 0, orientation);
                if (debug) print("MoveOutline: outline position set: " + patternID + ";" + position.ToString() + ";" + orientation.ToString());
            }
        }

        // Instantiates Outline objects for a given list of patterns
        public static void InstantiateOutlines(List<TangiblePattern> patterns)
        {
            foreach (TangiblePattern pattern in patterns)
                InstantiateOutline(pattern);
        }

        // Instantiates an Outline object for a given 'pattern'
        public static void InstantiateOutline(TangiblePattern pattern)
        {
            if (debug) print("InstantiateOutline invoced with: " + pattern.ToString(true));
            if (!outlineDict.ContainsKey(pattern.id))
            {
                //Vector2 anchorVector = pattern.anchorPoint1 - pattern.anchorPoint2;
                //Vector2 widthVector = anchorVector * pattern.gridWidth / 2;
                //Vector2 anchorVector1 = RotateVector(anchorVector, -90);
                //Vector2 heightVector = anchorVector1 * gridHeight / 2;
                //float width = widthVector.magnitude + padD;
                //float height = heightVector.magnitude + padD;
                Vector2 pivot = new Vector2( // pivot point is at second (most right) base point
                    (pattern.OutlineWidth - TangiblePattern.padD) / pattern.OutlineWidth,
                    TangiblePattern.padD / pattern.OutlineHeight
                    );

                GameObject outline = Instantiate(tangibleOutlinePrefab);
                RectTransform rt = outline.transform as RectTransform;
                rt.pivot = pivot;
                rt.position = pattern.Position;
                rt.SetParent(rectTransform);
                rt.sizeDelta = new Vector2(pattern.OutlineWidth, pattern.OutlineHeight);
                rt.Rotate(0, 0, pattern.orientation);
                outline.GetComponentInChildren<Text>().text = pattern.id.ToString();

                outline.SetActive(false);
                outlineDict.Add(pattern.id, outline);

                if (debug) print("outline drawn at "+pattern.Position.ToString());
            }
        }

    }

}
