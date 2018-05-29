using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
public class CapacitiveTangiblesRecognizer : MonoBehaviour {

    public List<TangiblePattern> patterns;
    void Start() {
        LoadTangiblesPatterns();
    }
    // Update is called once per frame
    void Update () {
        if(Input.touchCount >= 3){
            
        }
	}

    public void LoadTangiblesPatterns()
    {
        patterns = new List<TangiblePattern>();
        string[] filenames = TangiblesFileUtils.LoadTangiblesJSON();
        foreach(string filename in filenames){
            string json = System.IO.File.ReadAllText(filename);
            TangiblePattern pattern = JsonUtility.FromJson<TangiblePattern>(json);
            patterns.Add(pattern);
        }
    }


    public float RecognizeTangiblesPattern(List<Vector2> tangiblePoints, List<Vector2> touchPoints){
        float probability = 0;
        Vector2 touchCenter = MathHelper.ComputeCenter(touchPoints);

        for (int i = 0; i < tangiblePoints.Count; i++){
            tangiblePoints[i] += touchCenter;
        }

        return probability;
    }
}
