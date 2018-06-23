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
		public void SetIDText (string id)
		{
			idText.text = id;
		}



		public void UpdatePosition (Vector3 pos, Vector3 rot)
		{
			gameObject.transform.position = pos;
			(gameObject.transform as RectTransform).eulerAngles = rot;

		}

		public void SavePosition ()
		{
			lastKnownPosition = gameObject.transform.position;
			lastKnownRotation = gameObject.transform.rotation;
		}

		public void ResetPosition ()
		{
			//gameObject.transform.position = lastKnownPosition;
			//gameObject.transform.rotation = lastKnownRotation;
		}
	}
}


