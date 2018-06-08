using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

using System.IO;


public class CapacitiveTangiblesRecognizer : MonoBehaviour{

    public GameObject patternPrefab;
    public GameObject patternFootPrefab;

    public List<TangiblePattern> patterns;
    public List<DbscanPoint> dbscanPoints = new List<DbscanPoint>();
    public int clusterCount = 0;
    public List<Color> clusterColors = new List<Color>();

    Dictionary<int, List<DbscanPoint>> clusterPointsDict = new Dictionary<int, List<DbscanPoint>>();
    public List<GameObject> patternObjects;

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
            HandleInput(ref touchPoints, touch.position, false);
        }

        RecognizeTangiblesPattern(patterns[0], touchPoints);
        //}
    }

    void HandleInput(ref List<Vector2> touchPoints, Vector2 screenPoint, bool recognize = true){
        bool inAvoidArea = false;
        foreach (RectTransform rt in avoidRecognitionAreas) {
            if (RectTransformUtility.RectangleContainsScreenPoint(rt, screenPoint))
            {
                inAvoidArea = true;
            }
        }
        if (!inAvoidArea) {
            touchPoints.Add(Camera.main.ScreenToWorldPoint(screenPoint));
            if(recognize){
                RecognizeTangiblesPattern(patterns[0], touchPoints);    
            }
        }
    }

    void Update() {

        if (Input.GetKeyDown(KeyCode.C))
        {
            DoClustering();
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

        List<Vector2> touchPoints = new List<Vector2>();
        switch(GlobalSettings.Instance.modality){
            case InputModality.Mouse:
                HandleMouseInput(ref touchPoints);
                break;
            case InputModality.Touch:
                HandleTouchInput(ref touchPoints);
                break;
        }
    }

    public void LoadTangiblesPatterns()
    {
        patterns = new List<TangiblePattern>();
        patternObjects = new List<GameObject>();
        string[] filenames = TangiblesFileUtils.LoadTangiblesJSON();
        foreach (string filename in filenames)
        {
            string json = System.IO.File.ReadAllText(filename);
            TangiblePattern pattern = JsonUtility.FromJson<TangiblePattern>(json);
            patterns.Add(pattern);
            GameObject patternObj = Instantiate(patternPrefab);
            Vector2 center = MathHelper.ComputeCenter(pattern.points, Color.green);
            patternObj.transform.localScale = new Vector3(pattern.radius, pattern.radius, 1);
            foreach(Vector2 point in pattern.points){
                GameObject footObj = Instantiate(patternFootPrefab);
                Vector3 pos = patternObj.transform.position - new Vector3(point.x, point.y, 0);
                footObj.transform.position = pos;
                //footObj.transform.SetParent(patternObj.transform);
            }

            patternObj.transform.position = Vector3.zero;
            patternObjects.Add(patternObj);
        }
    }

    private void OnDrawGizmos()
    {
        
        foreach (DbscanPoint point in dbscanPoints)
        {
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
            DbscanPoint dbscanPoint = new DbscanPoint(tp);
            dbscanPoints.Add(dbscanPoint);
        }

        DoClustering();


        foreach(int clusterId in clusterPointsDict.Keys){
            List<Vector2> clusterPoints = new List<Vector2>();
            List<DbscanPoint> clsPts = clusterPointsDict[clusterId];
            clusterPoints.AddRange(clsPts.Select(item => item.point).ToList<Vector2>());
            Vector2 clusterCenter = MathHelper.ComputeCenter(clusterPoints, clusterColors[clusterId-1]);
        }

        return probability;
    }


    public void DoClustering(){
        clusterCount = DensityBasedClustering.DBScan(dbscanPoints, 1, 3);
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
    }

    public void ResetClusters(){
        foreach (DbscanPoint dsp in dbscanPoints)
        {
            dsp.Reset();
        }
    }

    public void ClearClusters(){
        dbscanPoints.Clear();
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
