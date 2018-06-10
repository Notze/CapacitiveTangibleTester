using System.Collections;
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
	 
    void OnEnable()
    {

    }

    void Start()
    {

        LoadTangiblesPatterns();
    }
    
    void HandleMouseInput(ref List<Vector2> touchPoints){
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 pos = Input.mousePosition;
            HandleInput(ref touchPoints, pos);
        }
    }

    void HandleTouchInput(ref List<Vector2> touchPoints){
        //if(Input.touchCount >= 3){
        foreach (Touch touch in Input.touches)
        {
			if(touch.phase == TouchPhase.Began){
				HandleInput (ref touchPoints, touch.position, false);	
			}
        }

        RecognizeTangiblesPattern(patterns[0], touchPoints);
        
    }

    void HandleInput(ref List<Vector2> touchPoints, Vector2 screenPoint, bool recognize = true){
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
            if(recognize){
                RecognizeTangiblesPattern(patterns[0], touchPoints);    
            }
        }
    }

	int rotationPoint = 0;

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
		if(Input.GetKeyDown(KeyCode.Y)){
			RecognizeTangiblesPattern (patterns [0], touchPoints);
			rotationPoint++;
			rotationPoint %= touchPoints.Count;
		}
    }

    public void LoadTangiblesPatterns()
    {
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
			touchObjects.Add (clusterTouch);
        }

        DoClustering(pattern.radius);


        foreach(int clusterId in clusterPointsDict.Keys){
			RecognizeClusterPattern (pattern, touchPoints, clusterId);
        }
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

		// rotate tangible:
		Transform [] feet = tangibleObj.GetComponentsInChildren<Transform> ();
		List<Vector2> feetPoints = new List<Vector2> ();
		foreach (Transform foot in feet) {
			feetPoints.Add (foot.position);
		}
		float minDistanceSum = float.MaxValue;
		int minDistI = 0;
		Quaternion rotation = tangibleObj.transform.rotation;
		RotateTangible (tangibleObj, touchPoints [rotationPoint]);
		float distanceSum = 0;
		foreach (Vector2 feetPoint in feetPoints) {
			foreach (Vector2 touchPoint in touchPoints) {
				distanceSum += Vector2.Distance (feetPoint, touchPoint);
			}
		}

		distanceSum /= feetPoints.Count;
		if (distanceSum < minDistanceSum) {
			minDistanceSum = distanceSum;
		}
		print (minDistanceSum);


		return probability;
	}

	void RotateTangible(GameObject tangibleObj, Vector2 rotateTo){
		Vector2 a = tangibleObj.transform.position - tangibleObj.transform.right;
		Vector2 b = tangibleObj.transform.position - new Vector3 (rotateTo.x, rotateTo.y, 0);
		float angle = Vector2.Angle (a, b);

		tangibleObj.transform.RotateAround (transform.position, Vector3.forward, angle);
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
				print ("noise");
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
