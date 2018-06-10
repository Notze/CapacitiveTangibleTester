﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

using System.IO;


public class CapacitiveTangiblesRecognizer : MonoBehaviour{
	
	public static long pointID;
    public GameObject patternPrefab;
    public GameObject patternFootPrefab;
	public GameObject touchPrefab;
    public List<TangiblePattern> patterns;
    public List<DbscanPoint> dbscanPoints = new List<DbscanPoint>();
    public int clusterCount = 0;
    public List<Color> clusterColors = new List<Color>();

    Dictionary<int, List<DbscanPoint>> clusterPointsDict = new Dictionary<int, List<DbscanPoint>>();
    public List<Tangible> tangibles;
	public List<ClusterTouch> touchObjects = new List<ClusterTouch>();

    public List<RectTransform> avoidRecognitionAreas;

	//GameObject debugTangibleObj;
	//List<Vector2> debugClusterPoints;
	//List<Vector2> debugFeetPoints;
	//int debugCurrentRotationIdx = 0;

	//public void DebugRotateTangible(){
	//	RotateTangible (debugTangibleObj, debugClusterPoints [debugCurrentRotationIdx]);
	//	debugCurrentRotationIdx++;
	//	debugCurrentRotationIdx %= debugClusterPoints.Count;


	//	Transform [] feet = debugTangibleObj.GetComponentsInChildren<Transform> ();
	//	debugFeetPoints = new List<Vector2> ();
	//	foreach (Transform foot in feet) {
	//		debugFeetPoints.Add (foot.position);
	//	}


	//	GlobalSettings.Instance.SetRotationIndex (debugCurrentRotationIdx);
	//	float dist = EvaluateTangiblePose (debugFeetPoints, debugClusterPoints);
	//	GlobalSettings.Instance.SetDistanceSum (dist);

	//	//StartCoroutine (DebugRotateAnim ());

	//}



    void OnEnable()
    {

    }

    void Start()
    {

        LoadTangiblesPatterns();
    }
    
    void HandleMouseInput(ref List<Vector2> touchPoints){
        if (Input.GetMouseButtonDown(0)) {
            Vector2 pos = Input.mousePosition;
            bool registered = RegisterInputPoint(ref touchPoints, pos);
			if (registered) {
				RecognizeTangiblesPattern (patterns [0], touchPoints);
			}
        }
    }

    void HandleTouchInput(ref List<Vector2> touchPoints){
		if (Input.touchCount > 0) {
			bool registered = false;
			foreach (Touch touch in Input.touches) {
				bool r = false;
				if (touch.phase == TouchPhase.Began) {
					r = RegisterInputPoint (ref touchPoints, touch.position);
					if(r){
						registered = true;
					}
				}
			}
			if(registered){
				//print ("rotationPoint:" + rotationPoint);
				RecognizeTangiblesPattern (patterns [0], touchPoints);
			}
			if (Input.touches [0].phase == TouchPhase.Ended) {
				//rotationPoint++;
				//rotationPoint %= clusterPointsDict[1].Count;
				//GlobalSettings.Instance.SetRotationIndex (rotationPoint);
			}


		}
        
    }

    bool RegisterInputPoint(ref List<Vector2> touchPoints, Vector2 screenPoint){
        bool inAvoidArea = false;
        foreach (RectTransform avoidArea in avoidRecognitionAreas) {
			if(avoidArea.gameObject.activeSelf){
				if (RectTransformUtility.RectangleContainsScreenPoint (avoidArea, screenPoint)) {
					inAvoidArea = true;
				}	
			}

        }
        if (!inAvoidArea) {
            touchPoints.Add(Camera.main.ScreenToWorldPoint(screenPoint));
        }
		return !inAvoidArea;

    }

	//int rotationPoint = 0;

    void Update() {

		List<Vector2> touchPoints = new List<Vector2> ();
		switch (GlobalSettings.Instance.modality) {
		case InputModality.Mouse:
			HandleMouseInput (ref touchPoints);
			break;
		case InputModality.Touch:
			HandleTouchInput (ref touchPoints);
			break;
		}


		if (Input.GetKeyDown(KeyCode.C))
        {
            DoClustering(patterns[0].radius * GlobalSettings.Instance.clusterRadiusScaler);
            //print(clusterPointsDict);
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetClusters();
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            ClearClusters();
        }
    }

    public void LoadTangiblesPatterns()
    {
		if(tangibles != null){
			foreach(Tangible t in tangibles){
				Destroy (t.gameObject);
			}
		}

        patterns = new List<TangiblePattern>();
        tangibles = new List<Tangible>();
        string[] filenames = TangiblesFileUtils.LoadTangiblesJSON();
        foreach (string filename in filenames)
        {
            string json = System.IO.File.ReadAllText(filename);
            TangiblePattern pattern = JsonUtility.FromJson<TangiblePattern>(json);
            patterns.Add(pattern);
            GameObject patternObj = Instantiate(patternPrefab);
			Tangible tangible = patternObj.GetComponent<Tangible> ();
			tangible.pattern = pattern;
            Vector2 center = MathHelper.ComputeCenter(pattern.points, Color.green);
            float xSize = patternObj.GetComponent<SpriteRenderer>().bounds.size.x/2;
            patternObj.transform.localScale = new Vector3(pattern.radius, pattern.radius, 1)/xSize;
            foreach(Vector2 point in pattern.points){
                GameObject footObj = Instantiate(patternFootPrefab);
                Vector3 pos = patternObj.transform.position + new Vector3(point.x, point.y, 0);
                footObj.transform.position = pos;
                footObj.transform.SetParent(patternObj.transform);
            }
            MathHelper.DrawCircle(center, pattern.radius, 50, Color.blue);
            patternObj.transform.position = Vector3.zero;
			tangibles.Add(tangible);
        }
    }

    private void OnDrawGizmos() {
        foreach (DbscanPoint point in dbscanPoints) {
            if(point.ClusterId > -1){
                Gizmos.color = clusterColors[point.ClusterId-1];    
            }else{
                Gizmos.color = Color.black;
            }
            Gizmos.DrawSphere(point.point, 0.25f);
        }
    }



    public float RecognizeTangiblesPattern(TangiblePattern pattern, List<Vector2> touchPoints)
	{
		
        ResetClusters();
        //recognizerTouches = new List<RecognizerTouch>();
        float probability = 0;

        foreach (Vector2 tp in touchPoints)
        {
			DbscanPoint dbscanPoint = new DbscanPoint(tp, pointID++);
            dbscanPoints.Add(dbscanPoint);
			GameObject touchObj = Instantiate (touchPrefab);
			touchObj.transform.position = tp;

			ClusterTouch clusterTouch = touchObj.GetComponent<ClusterTouch> ();
			clusterTouch.pointID = dbscanPoint.pointID;
			clusterTouch.dbscanPoint = dbscanPoint;
			dbscanPoint.clusterTouch = clusterTouch;
			touchObjects.Add (clusterTouch);
        }

        DoClustering(pattern.radius);
		if(clusterCount > 0){
			RecognizeClusterPattern (pattern, touchPoints, 1);	
		}

        //foreach(int clusterId in clusterPointsDict.Keys){
			
        //}
        return probability;
    }

	public float RecognizeClusterPattern (TangiblePattern pattern, List<Vector2> touchPoints, int clusterId)
	{
		float probability = 0;
		GameObject tangibleObj = tangibles.Find (t => t.pattern.id == pattern.id).gameObject;
		List<Vector2> clusterPoints = new List<Vector2> ();

		List<DbscanPoint> clsPts = clusterPointsDict [clusterId];
		clusterPoints.AddRange (clsPts.Select (item => item.point).ToList<Vector2> ());
		Vector2 clusterCenter = MathHelper.ComputeCenter (clusterPoints, clusterColors [clusterId - 1]);

		// translate tangible to cluster center:
		tangibleObj.transform.position = clusterCenter;


		RotateTangible360 (tangibleObj, clusterPoints);


		return probability;
	}


	float EvaluateTangiblePose(List<Vector2> feetPoints, List<Vector2> clusterPoints){
		float minDistanceSum = 0;
		for (int i = 0; i < feetPoints.Count; i++){
			float minDist = float.MaxValue;
			for (int j = 0; j < clusterPoints.Count; j++){
				float dist = Vector2.Distance (feetPoints [i], clusterPoints [j]);
				if(dist < minDist){
					minDist = dist;
				}
			}
			minDistanceSum += minDist;
		}

		return minDistanceSum;
	}

	void RotateTangible(GameObject tangibleObj, Vector2 rotateTo){
		tangibleObj.transform.rotation = Quaternion.identity;
		Vector2 pos = tangibleObj.transform.position;
		Vector2 up = tangibleObj.transform.up;
		Vector2 a = pos - up;
		Vector2 b = pos - rotateTo;
		float angle = Vector2.Angle (a, b);
		//print ("angle: " + angle);
		tangibleObj.transform.RotateAround (tangibleObj.transform.position, Vector3.forward, angle);
	}


	float RotateTangible360(GameObject tangibleObj, List<Vector2> clusterPoints)
	{
		int minAngle = 0;
		float minDist = float.MaxValue;
		for (int i = 0; i < 360; i++) {

			tangibleObj.transform.rotation = Quaternion.identity;
			tangibleObj.transform.RotateAround (tangibleObj.transform.position, Vector3.forward, i);

			Transform [] feet = tangibleObj.GetComponentsInChildren<Transform> ();
			List<Vector2> feetPoints = new List<Vector2> ();
			foreach (Transform foot in feet) {
				feetPoints.Add (foot.position);
			}

			float dist = EvaluateTangiblePose (feetPoints, clusterPoints);
			if (dist < minDist) {
				minDist = dist;
				minAngle = i;
			}
		}
		tangibleObj.transform.rotation = Quaternion.identity;
		tangibleObj.transform.RotateAround (tangibleObj.transform.position, Vector3.forward, minAngle);
		GlobalSettings.Instance.SetDistanceSum (minDist);

		return minDist;
	}

    public void DoClustering(float radius){
		GlobalSettings.Instance.SetNumClusterPoints(dbscanPoints.Count);
		clusterCount = DensityBasedClustering.DBScan(dbscanPoints, radius, 3);
        clusterColors = ClusterColors(clusterCount);
        clusterPointsDict = new Dictionary<int, List<DbscanPoint>>();
        for (int i = 1; i <= clusterCount; i++){
            clusterPointsDict.Add(i, new List<DbscanPoint>());
        }
        foreach(DbscanPoint p in dbscanPoints){
            if(!p.IsNoise){
                clusterPointsDict[p.ClusterId].Add(p);
            }
        }
		foreach(ClusterTouch ct in touchObjects){
			Color color = Color.black;
			if(!ct.dbscanPoint.IsNoise){
				ct.ClusterId = ct.dbscanPoint.ClusterId;
				color = clusterColors [ct.ClusterId - 1];
			}
			ct.SetClusterColor (color);
		}
    }

    public void ResetClusters(){
        foreach (DbscanPoint dsp in dbscanPoints)
        {
            dsp.Reset();
        }
		foreach (ClusterTouch ct in touchObjects) {
			ct.Reset();
		}
    }

    public void ClearClusters(){
        dbscanPoints.Clear();
		foreach(ClusterTouch ct in touchObjects){
			Destroy(ct.gameObject);
		}
		touchObjects.Clear();
    }


    List<Color> ClusterColors(int N){
        List<Color> colors = new List<Color>();
        float step = 360.0f / N;
        for (int i = 0; i < N; i++){
            colors.Add(Color.HSVToRGB((i * step)/360.0f, 1, 1));
        }
        return colors;
    }



}
