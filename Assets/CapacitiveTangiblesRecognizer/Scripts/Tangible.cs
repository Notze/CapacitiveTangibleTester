using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CTR{
	public class Tangible : MonoBehaviour {

		public TangiblePattern pattern;
		public Text idText;
		public Vector3 lastKnownPosition;
		public Quaternion lastKnownRotation;
		public Transform anchor1;
		public Transform anchor2;
		public Transform info1;
		public Transform info2;
		public float positionProbability;
		public float fadeOutDuration = 10.0f;

		public RectTransform rectTransform;




		private void Start(){
			positionProbability = 0.01f;
			rectTransform = transform as RectTransform;
		}

		private void Update () {
			positionProbability -= Time.deltaTime/fadeOutDuration;
			positionProbability = Mathf.Clamp(positionProbability, 0.01f, 0.99f);
			//print (pattern.id + " " + positionToken);
		}


		public void SetIDText(string id)
		{
			idText.text = id;
		}

		public void UpdatePosition (Vector3 pos, Vector3 rot, float probability) {
			gameObject.transform.position = pos;
			(gameObject.transform as RectTransform).eulerAngles = rot;
			positionProbability = probability;
			SavePosition();
			CapacitiveTangiblesRecognizer.Instance.NotifyTangibleUpdate(pattern.id, transform.position, transform.rotation);
		}

		void SavePosition() {
			lastKnownPosition = gameObject.transform.position;
			lastKnownRotation = gameObject.transform.rotation;
		}

		public void ResetPosition() {
			gameObject.transform.position = lastKnownPosition;
			gameObject.transform.rotation = lastKnownRotation;
		}

		public void SetColor(Color color) {
			GetComponent<Image>().color = color;
		}

		public List<Vector2> GetFeetPoints(){
			Transform[] feet = GetComponentsInChildren<Transform>();
			List<Vector2> feetPoints = new List<Vector2>();
			foreach(Transform foot in feet) {
				if(foot.CompareTag("Foot")) {
					feetPoints.Add(foot.position);
				}
			}
			return feetPoints;
		}
	}
}


