//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.UI;

//namespace CTR{
//	public class Tangible : MonoBehaviour {

//        public TangiblePattern pattern;

//        public float orientation { get; set; }
//        public Vector2 position { get; set; }


//        //public Tuple<int, int> infoCoord; 
//        //public Text idText;
//        //public Vector3 lastKnownPosition;
//        //public Quaternion lastKnownRotation;
//        //public Transform anchor1;
//        //public Transform anchor2;
//        //public Transform info;
//        //public float positionProbability;
//        //public float fadeOutDuration = 10.0f;

//        //// should be obsolete after complete adaption of other classes
//        //public Transform info1;
//        //public Transform info2;

//        //// rectangle transform that represents a tangible on GUI
//        //public RectTransform rectTransform;



//        //      // initiates rectTransform and positionProbability
//        //private void Start(){
//        //	positionProbability = 0.01f;
//        //	rectTransform = transform as RectTransform;
//        //}

//        //      // updates positionPobability
//        //private void Update () {
//        //	positionProbability -= Time.deltaTime/fadeOutDuration;
//        //	positionProbability = Mathf.Clamp(positionProbability, 0.01f, 0.99f);
//        //	//print (pattern.id + " " + positionToken);
//        //}

//        //      // updates position of this tangible-instance and calls for a GUI-Update
//        //public void UpdatePosition (Vector3 pos, Vector3 rot, float probability) {
//        //	gameObject.transform.position = pos;
//        //	(gameObject.transform as RectTransform).eulerAngles = rot;
//        //	positionProbability = probability;
//        //	SavePosition();
//        //	CapacitiveTangiblesRecognizer.Instance.NotifyTangibleUpdate(pattern.id, transform.position, transform.rotation);
//        //}

//        //      // updates 'lastKnown' variables
//        //void SavePosition() {
//        //	lastKnownPosition = gameObject.transform.position;
//        //	lastKnownRotation = gameObject.transform.rotation;
//        //}

//        //      // reverts the position of this tangible-instance to the 'lastKnown' 
//        //public void ResetPosition() {
//        //	gameObject.transform.position = lastKnownPosition;
//        //	gameObject.transform.rotation = lastKnownRotation;
//        //}

//        //      #region Tangible getters&setters

//        //      // getter of infoCoord
//        //      public Tuple<int,int> getInfoCoord()
//        //      {
//        //          return infoCoord;
//        //      }

//        //      // setter of color
//        //      public void SetColor(Color color) {
//        //	GetComponent<Image>().color = color;
//        //}

//        //      // setter of IDText
//        //public void SetIDText(string id)
//        //{
//        //	idText.text = id;
//        //}

//        //      // getter of feetPoints
//        //      public List<Vector2> GetFeetPoints()
//        //      {
//        //          Transform[] feet = GetComponentsInChildren<Transform>();
//        //          List<Vector2> feetPoints = new List<Vector2>();
//        //          foreach (Transform foot in feet)
//        //          {
//        //              if (foot.CompareTag("Foot"))
//        //              {
//        //                  feetPoints.Add(foot.position);
//        //              }
//        //          }
//        //          return feetPoints;
//        //      }

//        //      #endregion
//    }
//}


