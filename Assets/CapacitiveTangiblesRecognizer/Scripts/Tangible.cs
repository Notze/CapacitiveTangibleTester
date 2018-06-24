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
		public float positionToken;
		public float fadeOutDuration = 10.0f;

		public void SetIDText (string id)
		{
			idText.text = id;
		}

		private void Update () {
			positionToken -= fadeOutDuration * Time.deltaTime;
			positionToken = Mathf.Clamp (positionToken, 0.01f, 0.99f);
		}


		public void UpdatePosition (Vector3 pos, Vector3 rot) {
			gameObject.transform.position = pos;
			(gameObject.transform as RectTransform).eulerAngles = rot;
			positionToken = 0.99f;
			SavePosition();
		}

		void SavePosition() {
			lastKnownPosition = gameObject.transform.position;
			lastKnownRotation = gameObject.transform.rotation;
		}

		public void ResetPosition() {
			gameObject.transform.position = lastKnownPosition;
			gameObject.transform.rotation = lastKnownRotation;
		}
	}
}


