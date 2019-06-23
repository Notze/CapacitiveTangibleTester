//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.UI;

//namespace CTR{
//	public class MainMenu : MonoBehaviour {

//		public Dropdown modalityDropdown;
//		public Slider clusterRadiusSlider;
//		public Slider anchorToleranceSlider;
//		public Slider minDistanceSlider;
//		public Slider fitnessThresholdSlider;
//		public Slider anchorWeightSlider;
//		public Slider positionWeightSlider;
//		public Button flipRotationButton;
//		bool isOpen;
//		public List<RectTransform> menuPanels;

//		void Start () {
//			clusterRadiusSlider.value = GlobalSettings.Instance.clusterRadiusScaler;
//			anchorToleranceSlider.value = GlobalSettings.Instance.anchorTolerance;
//			minDistanceSlider.value = GlobalSettings.Instance.minDistanceBetweenTouchPoints;
//			fitnessThresholdSlider.value = GlobalSettings.Instance.patternFitThreshold;
//			anchorWeightSlider.value = GlobalSettings.Instance.anchorWeight;
//			positionWeightSlider.value = GlobalSettings.Instance.positionWeight;
//		}



//		public void ToggleMenu (){
//			if (isOpen) {
//				CloseMenu ();
//			} else {
//				OpenMenu ();
//			}
//			isOpen = !isOpen;

//		}

//		void OpenMenu () {
//			foreach (RectTransform rt in menuPanels) {
//				rt.gameObject.SetActive (true);
//			}
//		}

//		void CloseMenu () {
//			foreach (RectTransform rt in menuPanels) {
//				rt.gameObject.SetActive (false);
//			}
//		}

//		public void SetModality () {
//			GlobalSettings.Instance.SetModality (modalityDropdown.value);
//		}
//		public void SetClusterRadiusScalar () {
//			GlobalSettings.Instance.SetClusterRadiusScaler (clusterRadiusSlider.value);
//		}
//		public void SetAnchorTolerance () {
//			GlobalSettings.Instance.SetAnchorTolerance (anchorToleranceSlider.value);
//		}

//		public void SetMinDistanceTolerance () {
//			GlobalSettings.Instance.SetMinDistanceBetweenTouchPoints (minDistanceSlider.value);
//		}

//		public void SetFitnessThreshold () {
//			GlobalSettings.Instance.SetPatternFitThreshold (fitnessThresholdSlider.value);
//		}
//		public void SetAnchorWeight ()
//		{
//			GlobalSettings.Instance.SetAnchorWeight (anchorWeightSlider.value);
//		}
//		public void SetPositionWeight ()
//		{
//			GlobalSettings.Instance.SetPositionWeight (positionWeightSlider.value);
//		}
//		public void ToggleFlip(){
//			GlobalSettings.Instance.ToggleFlipRotation();
//			flipRotationButton.GetComponent<Image>().color = GlobalSettings.Instance.flipRotation ? Color.yellow : Color.white;
//		}
//	}
//}


