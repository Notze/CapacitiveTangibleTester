using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace CTR{
	public class TouchMonitor : MonoBehaviour {
		[Range (1, 100)]
		public int numTouches;
		public GameObject touchPrefab;
		List<GameObject> touches;
		// Use this for initialization
		void Start ()
		{
			touches = new List<GameObject> (numTouches);
			for (int i = 0; i < numTouches; i++) {
				GameObject touch = GameObject.Instantiate (touchPrefab, this.transform);
				touches.Add (touch);
				touch.SetActive (false);
			}
		}

		// Update is called once per frame
		void Update ()
		{
			foreach (GameObject go in touches) {
				go.SetActive (false);
			}

			for (int i = 0; i < Input.touchCount; i++) {
				Touch t = Input.GetTouch (i);
				this.touches [i].transform.position = t.position;
				Image img = touches [i].GetComponent<Image> ();
				switch (t.phase) {
				case TouchPhase.Began:
					img.color = Color.green;
					break;
				case TouchPhase.Canceled:
					img.color = Color.yellow;
					break;
				case TouchPhase.Ended:
					img.color = Color.red;
					break;
				case TouchPhase.Moved:
					img.color = Color.blue;
					break;
				case TouchPhase.Stationary:
					//img.color = Color.cyan;
					break;
				}
				this.touches [i].SetActive (true);
			}
		}
	}
}

