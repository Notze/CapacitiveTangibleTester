using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;





public class CapacitiveTangiblesRecognizer : MonoBehaviour{
	
	public static long pointID;
    public GameObject patternPrefab;
    public GameObject patternFootPrefab;
	public GameObject touchPrefab;
    public List<TangiblePattern> patterns;
    public List<DbscanPoint> dbscanPoints = new List<DbscanPoint>();
    public int clusterCount = 0;
    public List<Color> clusterColors = new List<Color>();
	public float clusterRadius;
	public int minNumOfPointsInCluster = 4;

    Dictionary<int, List<DbscanPoint>> clusterPointsDict = new Dictionary<int, List<DbscanPoint>>();
    public List<Tangible> tangibles;
	public List<ClusterTouch> touchObjects = new List<ClusterTouch>();

    public List<RectTransform> avoidRecognitionAreas;
	public float castRadius;
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
		GameObject [] avoidGOs = GameObject.FindGameObjectsWithTag("Avoid");
		avoidRecognitionAreas = new List<RectTransform>();
		foreach(GameObject avoidGO in avoidGOs){
			avoidRecognitionAreas.Add (avoidGO.transform as RectTransform);
		}

		castRadius = touchPrefab.GetComponent<CircleCollider2D> ().radius;

        LoadTangiblesPatterns();
    }
    
    void HandleMouseInput(ref List<Vector2> touchPoints){
        if (Input.GetMouseButtonDown(0)) {
            Vector2 pos = Input.mousePosition;
            bool registered = RegisterInputPoint(ref touchPoints, pos);
			if (registered) {
				RecognizeTangibles(patterns, touchPoints);
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
				RecognizeTangibles(patterns, touchPoints);
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
			DoClustering(clusterRadius * GlobalSettings.Instance.clusterRadiusScaler, minNumOfPointsInCluster);
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

	public void DeleteTangiblesPatterns(){
		TangiblesFileUtils.DeleteTangibles();
	}

    public void LoadTangiblesPatterns() {
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
			tangible.SetIDText(pattern.id);
			tangible.pattern = pattern;
            Vector2 center = MathHelper.ComputeCenter(pattern.points, Color.green);
            float xSize = patternObj.GetComponent<SpriteRenderer>().bounds.size.x/2;
            patternObj.transform.localScale = new Vector3(pattern.radius, pattern.radius, 1)/xSize;
			for (int i = 0; i < pattern.points.Count; i++){
				Vector2 point = pattern.points [i];
			
                GameObject footObj = Instantiate(patternFootPrefab);
                Vector3 pos = patternObj.transform.position + new Vector3(point.x, point.y, 0);
                footObj.transform.position = pos;
                footObj.transform.SetParent(patternObj.transform);
				if(i == pattern.anchorPoint1){
					footObj.GetComponent<SpriteRenderer> ().color = Color.green;
					tangible.anchor1 = footObj.transform;
				}
				if (i == pattern.anchorPoint2) {
					footObj.GetComponent<SpriteRenderer> ().color = Color.yellow;
					tangible.anchor2 = footObj.transform;
				}
            }
            MathHelper.DrawCircle(center, pattern.radius, 50, Color.blue);
            patternObj.transform.position = Vector3.zero;
			tangibles.Add(tangible);
        }
		clusterRadius = patterns.Max (ptn => ptn.radius);
		print ("clusterRadius: " + clusterRadius);
    }

   // private void OnDrawGizmos() {
   //     foreach (DbscanPoint point in dbscanPoints) {
			//if(!point.IsNoise && point.ClusterId != -1){
    //            Gizmos.color = clusterColors[point.ClusterId-1];    
    //        }else{
    //            Gizmos.color = Color.black;
    //        }
    //        Gizmos.DrawSphere(point.point, 0.25f);
    //    }
    //}


	private void OnGUI ()
	{
		GUIStyle style = new GUIStyle ();
		style.fontSize = 32;
		GUILayout.BeginVertical (style);
		string header = "";
		header += "pattern\t";
		for (int i = 1; i <= clusterPointsDict.Count; i++) {
			header += i + "\t";
		}
		GUILayout.Label (header, style);

		if (patternFitnessList != null) {
			for (int i = 0; i < patternFitnessList.Count; i++){
				string fitnessString = patterns[i].id + "\t";
				Dictionary<int, float> fitnessDict = patternFitnessList [i];
				foreach(int clusterID in fitnessDict.Keys){
					fitnessString += fitnessDict[clusterID].ToString("0.00") + "\t";
				}
				GUILayout.Label (fitnessString, style);
			}
		}
		GUILayout.EndVertical ();
	}

	List<Dictionary<int, float>> patternFitnessList;
	public void RecognizeTangibles(List<TangiblePattern> patterns, List<Vector2> touchPoints){
		patternFitnessList = new List<Dictionary<int, float>> ();

		ResetClusters();
		foreach (Vector2 tp in touchPoints) {
			DbscanPoint dbscanPoint = new DbscanPoint (tp, pointID++);
			dbscanPoints.Add (dbscanPoint);
			GameObject touchObj = Instantiate (touchPrefab);
			touchObj.transform.position = tp;

			ClusterTouch clusterTouch = touchObj.GetComponent<ClusterTouch> ();
			clusterTouch.pointID = dbscanPoint.pointID;
			clusterTouch.dbscanPoint = dbscanPoint;
			dbscanPoint.clusterTouch = clusterTouch;
			touchObjects.Add (clusterTouch);
		}
		DoClustering (clusterRadius * GlobalSettings.Instance.clusterRadiusScaler, minNumOfPointsInCluster);

		foreach(TangiblePattern pattern in patterns){
			Dictionary<int, float> fitnessDict = RecognizeTangiblesPattern (pattern, touchPoints);
			patternFitnessList.Add(fitnessDict);
		}
		for (int i = 0; i < patternFitnessList.Count; i++) {
			Dictionary<int, float> fitnessDict = patternFitnessList [i];
			int minClusterID = 0;
			float minDist = float.MaxValue;
			foreach (int clusterId in fitnessDict.Keys) {
				if (fitnessDict [clusterId] < minDist) {
					minDist = fitnessDict [clusterId];
					minClusterID = clusterId;
				}
			}
			//print (string.Format ("pattern: {0} cluster: {1} distance: {2}", i, minClusterID, minDist));
			//if (minDist < 2.5f){
			//	if (clusterPointsDict.ContainsKey (minClusterID)) {
			//		tangibles [i].UpdatePosition (clusterPointsDict [minClusterID] [0].clusterTouch.clusterCenter);
			//	}	
			//}else{
			//	tangibles[i].ResetPosition();
			//}





			//if(fitnessDict.Count > 1){
				
			//	//
			//	//print (clusterID);	
			//}
		}

	}
	/// <summary>
	/// Recognizes the tangibles pattern.
	/// </summary>
	/// <returns>Fites dictionary <clusterID distance> </returns>
	/// <param name="pattern">Pattern.</param>
	/// <param name="touchPoints">Touch points.</param>
	Dictionary<int, float> RecognizeTangiblesPattern(TangiblePattern pattern, List<Vector2> touchPoints)
	{
		Dictionary<int, float> clusterDistances = new Dictionary<int, float> ();
        

        foreach(int clusterId in clusterPointsDict.Keys){
			float dist = RecognizeClusterPattern (pattern, clusterId);
			clusterDistances.Add (clusterId, dist);
        }
		return clusterDistances;
    }



	float RecognizeClusterPattern (TangiblePattern pattern, int clusterId)
	{
		
		Tangible tangible = tangibles.Find (t => t.pattern.id == pattern.id);
#warning reset position of tangible
		tangible.transform.position = Vector3.zero;
		tangible.transform.rotation = Quaternion.identity;


		List<Vector2> clusterPoints = new List<Vector2> ();

		List<DbscanPoint> clsPts = clusterPointsDict [clusterId];
		clusterPoints.AddRange (clsPts.Select (item => item.point).ToList<Vector2> ());
		Vector2 clusterCenter = MathHelper.ComputeCenter (clusterPoints, clusterColors [clusterId - 1]);
		foreach(DbscanPoint scanPoint in clsPts){
			if(!scanPoint.IsNoise){
				scanPoint.clusterTouch.clusterCenter = clusterCenter;	
			}
		}

		// find anchor points:
		float anchorDistance = float.MaxValue;
		int firstAnchor = 0;
		int secondAnchor = 0;
		for (int i = 0; i < clsPts.Count; i++){
			for (int j = 0; j < clsPts.Count; j++){
				if(i == j){
					continue;
				}
				float distance = Vector2.Distance (clsPts [i].point, clsPts [j].point);
				if(Mathf.Abs(distance - pattern.anchorDistance) < GlobalSettings.Instance.anchorTolerance){
					clsPts [i].clusterTouch.GetComponent<SpriteRenderer> ().color = Color.white;
					clsPts [j].clusterTouch.GetComponent<SpriteRenderer> ().color = Color.white;
					anchorDistance = distance;
					float firstDistFromCenter = Vector2.Distance(clsPts [i].point, clusterCenter);
					float secondDistFromCenter = Vector2.Distance(clsPts [j].point, clusterCenter);
					firstAnchor = i;
					secondAnchor = j;
					if(secondDistFromCenter > firstDistFromCenter){
						int tmp = firstAnchor;
						firstAnchor = secondAnchor;
						secondAnchor = tmp;
					}
					clsPts [firstAnchor].clusterTouch.GetComponent<SpriteRenderer> ().color = Color.green;
					clsPts [secondAnchor].clusterTouch.GetComponent<SpriteRenderer> ().color = Color.yellow;
				}
			}
		}

		// find translation and rotation:

		Vector2 a = clsPts [firstAnchor].point - clsPts[secondAnchor].point;
		Vector2 b = tangible.anchor1.position - tangible.anchor2.position;
		float angle = Vector2.SignedAngle(b, a);
		GlobalSettings.Instance.SetRotationAngle(angle);
		Quaternion rot = Quaternion.Euler(0, 0, angle);



		tangible.transform.rotation = rot;
		Vector2 tangibleOffset = new Vector2 (tangible.transform.position.x - tangible.anchor1.position.x, 
		                                      tangible.transform.position.y - tangible.anchor1.position.y);
		Vector2 pos = clsPts [firstAnchor].point + tangibleOffset;
		tangible.transform.position = pos;


		Transform [] feet = tangible.GetComponentsInChildren<Transform> ();
		List<Vector2> feetPoints = new List<Vector2> ();
		foreach (Transform foot in feet) {
			if (foot.CompareTag ("Foot")) {
				feetPoints.Add (foot.position);
			}
		}

		float minDistSum = 0;
		for (int i = 0; i < feetPoints.Count; i++){
			float minDist = float.MaxValue;
			for (int j = 0; j < clsPts.Count; j++){
				float dist = (feetPoints [i] - clsPts [j].point).sqrMagnitude;
				if(dist < minDist){
					minDist = dist;
				}
			}
			minDistSum += minDist;
			//RaycastHit2D[] hits = Physics2D.CircleCastAll(feetPoints [i], castRadius, Vector2.zero);
			//foreach(RaycastHit2D hit in hits){
			//	if (hit.transform.CompareTag ("Touch")) {
			//		hitCount++;
			//		hit.transform.GetComponent<SpriteRenderer> ().color = Color.blue;
			//	}	
			//}
		}
		minDistSum /= clusterRadius* GlobalSettings.Instance.clusterRadiusScaler;


		// translate tangible to cluster center:
		//tangible.transform.position = clusterCenter;

		//float minDistanceSum = RotateTangible360 (tangible.gameObject, clusterPoints);
		//return minDistanceSum;

		return minDistSum;
	}


	float RotateTangible360 (GameObject tangibleObj, List<Vector2> clusterPoints)
	{
		int minAngle = 0;
		float minDist = float.MaxValue;
		List<Tuple<int, int>> closestPoints = null;
		for (int i = 0; i < 360; i++) {
			List<Tuple<int, int>> tmpClosestPoints;
			tangibleObj.transform.rotation = Quaternion.identity;
			tangibleObj.transform.RotateAround (tangibleObj.transform.position, Vector3.forward, i);

			Transform [] feet = tangibleObj.GetComponentsInChildren<Transform> ();
			List<Vector2> feetPoints = new List<Vector2> ();
			foreach (Transform foot in feet) {
				if (foot.CompareTag ("Foot")) {
					feetPoints.Add (foot.position);
				}
			}

			float dist = EvaluateTangiblePose (feetPoints, clusterPoints, out tmpClosestPoints);
			if (dist < minDist) {
				minDist = dist;
				minAngle = i;
				closestPoints = tmpClosestPoints;
			}
		}
		tangibleObj.transform.rotation = Quaternion.identity;
		tangibleObj.transform.RotateAround (tangibleObj.transform.position, Vector3.forward, minAngle);
		Transform [] feet2 = tangibleObj.GetComponentsInChildren<Transform> ();
		List<Vector2> feetPoints2 = new List<Vector2> ();
		foreach (Transform foot in feet2) {
			feetPoints2.Add (foot.position);
		}

		foreach (Tuple<int, int> pair in closestPoints) {
			Debug.DrawLine (feetPoints2 [pair.first], clusterPoints [pair.second], Color.cyan, 30);
		}
		return minDist;
	}


	float EvaluateTangiblePose(List<Vector2> feetPoints, List<Vector2> clusterPoints, out List<Tuple<int, int>> closestPoints){
		float minDistanceSum = 0;
		closestPoints = new List<Tuple<int, int>> ();
		for (int i = 0; i < feetPoints.Count; i++){
			Tuple<int, int> tuple = new Tuple<int, int> ();
			tuple.first = i;
			float minDist = float.MaxValue;
			for (int j = 0; j < clusterPoints.Count; j++){
				float dist = Vector2.Distance (feetPoints [i], clusterPoints [j]);
				if(dist < minDist){
					minDist = dist;
					tuple.second = j;
				}
			}
			closestPoints.Add (tuple);
			minDistanceSum += minDist;
		}
		//minDistanceSum /= feetPoints.Count;
		minDistanceSum /= clusterRadius * GlobalSettings.Instance.clusterRadiusScaler;
		return minDistanceSum;
	}

	//void RotateTangible(GameObject tangibleObj, Vector2 rotateTo){
	//	tangibleObj.transform.rotation = Quaternion.identity;
	//	Vector2 pos = tangibleObj.transform.position;
	//	Vector2 up = tangibleObj.transform.up;
	//	Vector2 a = pos - up;
	//	Vector2 b = pos - rotateTo;
	//	float angle = Vector2.Angle (a, b);
	//	//print ("angle: " + angle);
	//	tangibleObj.transform.RotateAround (tangibleObj.transform.position, Vector3.forward, angle);
	//}




    public void DoClustering(float radius, int minNumOfPoints){
		GlobalSettings.Instance.SetNumClusterPoints(dbscanPoints.Count);
		clusterCount = DensityBasedClustering.DBScan(dbscanPoints, radius, minNumOfPoints);
        clusterColors = ClusterColors(clusterCount);
        clusterPointsDict = new Dictionary<int, List<DbscanPoint>>();
        for (int i = 1; i <= clusterCount; i++){
            clusterPointsDict.Add(i, new List<DbscanPoint>());
        }
        foreach(DbscanPoint p in dbscanPoints){
            if(!p.IsNoise && p.ClusterId != -1){
                clusterPointsDict[p.ClusterId].Add(p);
			}else{
				//print ("is Noise");
			}
        }
		foreach(ClusterTouch ct in touchObjects){
			Color color = Color.black;
			if(!ct.dbscanPoint.IsNoise && ct.dbscanPoint.ClusterId != -1){
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
