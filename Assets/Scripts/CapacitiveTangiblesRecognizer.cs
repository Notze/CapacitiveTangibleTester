using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;


public class CapacitiveTangiblesRecognizer : MonoBehaviour
{

    public List<TangiblePattern> patterns;
    public List<DbscanPoint> dbscanPoints = new List<DbscanPoint>();
    public int clusterCount = 0;
    public List<Color> clusterColors = new List<Color>();
    void OnEnable()
    {

    }

    void Start()
    {

        LoadTangiblesPatterns();
    }
    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.C))
        {
            clusterCount = DensityBasedClustering.DBScan(dbscanPoints, 1, 3);
            clusterColors = ClusterColors(clusterCount);
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            dbscanPoints.Clear();
        }

        List<Vector2> touchPoints = new List<Vector2>();

        if (Input.GetMouseButtonDown(0))
        {
            touchPoints.Add(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            RecognizeTangiblesPattern(patterns[0], touchPoints);
        }

        //if(Input.touchCount >= 3){
        //    foreach(Touch touch in Input.touches){
        //        touchPoints.Add(Camera.main.ScreenToWorldPoint(touch.position));
        //    }

        //    RecognizeTangiblesPattern(patterns[0], touchPoints);
        //}
    }

    public void LoadTangiblesPatterns()
    {
        patterns = new List<TangiblePattern>();
        string[] filenames = TangiblesFileUtils.LoadTangiblesJSON();
        foreach (string filename in filenames)
        {
            string json = System.IO.File.ReadAllText(filename);
            TangiblePattern pattern = JsonUtility.FromJson<TangiblePattern>(json);
            patterns.Add(pattern);
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
        //recognizerTouches = new List<RecognizerTouch>();
        float probability = 0;

        foreach (Vector2 tp in touchPoints)
        {
            DbscanPoint dbscanPoint = new DbscanPoint(tp);
            dbscanPoints.Add(dbscanPoint);
        }
        return probability;
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
