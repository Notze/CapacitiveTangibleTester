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

        // hardcoded configuration for the current tangible layout
        static float padD = 24; // adjust this so the outline is extended far enough
        static int gridWidth = 5; // pattern grid dimensions of physical tangibles
        static int gridHeight = 6;

        static Dictionary<string, GameObject> outlineDict = new Dictionary<string, GameObject>();


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

        // Rotates a Vector2.
        static Vector2 RotateVector(Vector2 v, float degrees)
        {
            float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
            float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

            float tx = v.x;
            float ty = v.y;
            v.x = (cos * tx) - (sin * ty);
            v.y = (sin * tx) + (cos * ty);
            return v;
        }

        // Moves the correspoding Outline object for a given pattern.
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

        // Instantiates an Outline object for a given 'pattern'
        public static void InstantiateOutline(TangiblePattern pattern)
        {
            if (debug) print("InstantiateOutline invoced with: " + pattern.ToString(true));
            if (!outlineDict.ContainsKey(pattern.id))
            {
                Vector2 anchorVector = pattern.anchorPoint1 - pattern.anchorPoint2;
                Vector2 widthVector = anchorVector * gridWidth / 2;
                Vector2 anchorVector1 = RotateVector(anchorVector, -90);
                Vector2 heightVector = anchorVector1 * gridHeight / 2;
                float width = widthVector.magnitude + padD;
                float height = heightVector.magnitude + padD;
                Vector2 pivot = new Vector2( // pivot point is at second (most right) base point
                    (width - padD) / width,
                    padD / height
                    );

                GameObject outline = Instantiate(tangibleOutlinePrefab);
                RectTransform rt = outline.transform as RectTransform;
                rt.pivot = pivot;
                rt.position = pattern.Position;
                rt.SetParent(rectTransform);
                rt.sizeDelta = new Vector2(width, height);
                rt.Rotate(0, 0, pattern.orientation);
                outline.GetComponentInChildren<Text>().text = pattern.id.ToString();

                outline.SetActive(false);
                outlineDict.Add(pattern.id, outline);

                if (debug) print("outline drawn at "+pattern.Position.ToString());
            }
        }

    }

}
