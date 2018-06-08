using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;


public class CapacitiveTangiblesMonitor : MonoBehaviour {

    public Text patternID;
    public GameObject TouchPrefab;

    RectTransform rectTransform;
	
	// Update is called once per frame
	void Update () {
        this.rectTransform = this.transform as RectTransform;
	}

    public void LoadPattern(){


        for (int i = 0; i < rectTransform.childCount; i++){
            Transform child = rectTransform.GetChild(i);
            if (child.CompareTag("Touch")){
                Destroy(child.gameObject);
            }
        }

        string fullfilepath = TangiblesFileUtils.PatternFilename(patternID.text);
        string json = File.ReadAllText(fullfilepath);
        TangiblePattern pattern = JsonUtility.FromJson<TangiblePattern>(json);

        List<Vector2> touchPositions = new List<Vector2>();
        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);
        foreach(Vector2 point in pattern.points){
            

            //Vector2 sPoint = Camera.main.WorldToScreenPoint(new Vector3(point.x, point.y, 0));
            GameObject touchGO = Instantiate(TouchPrefab);
            touchGO.transform.position = transform.position + corners[3]/2 + new Vector3 (point.x, point.y, 0);
            touchGO.transform.SetParent(this.transform);
            //Vector2 thisPos = new Vector2(rectTransform.position.x - rectTransform.sizeDelta.x/2,
            //                              rectTransform.position.y + rectTransform.sizeDelta.y/2);
            //(touchGO.transform as RectTransform).position = thisPos + sPoint;
            touchPositions.Add(touchGO.transform.position);
        }

        Vector2 center = MathHelper.ComputeCenter(touchPositions, Color.red);



        for (int i = 1; i < touchPositions.Count; i++)
        {
            Debug.DrawLine(touchPositions[i-1],
                           center,
                          Color.cyan, 30);
        }


        MathHelper.DrawCircle(center, pattern.radius, 50);
    }
}
