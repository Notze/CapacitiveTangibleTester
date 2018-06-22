using UnityEngine;
using UnityEngine.UI;

namespace CTR {
	public static class CTRUtils {
		
		public static GameObject InstantiateTangibleObject (TangiblePattern pattern, GameObject patternPrefab, GameObject footPrefab, RectTransform parent, bool setPosition = true)
		{
			GameObject patternObj = GameObject.Instantiate(patternPrefab);
			Tangible tangible = patternObj.GetComponent<Tangible> ();
			tangible.SetIDText(pattern.id.ToString());
			tangible.pattern = pattern;
			Vector2 center = MathHelper.ComputeCenter (pattern.points, Color.green);
			RectTransform patternRectTransform = patternObj.transform as RectTransform;
				

			patternRectTransform.sizeDelta = new Vector2(2 * pattern.radius, 2 * pattern.radius);


			for (int i = 0; i < pattern.points.Count; i++) {
				Vector2 point = pattern.points[i];

				GameObject footObj = GameObject.Instantiate(footPrefab);
				RectTransform footRectTransform = footObj.transform as RectTransform;
				Vector3 pos = patternObj.transform.position + new Vector3(point.x, point.y, 0);
				footRectTransform.SetParent (patternRectTransform);
				footRectTransform.localScale = Vector3.one;
				footRectTransform.position = pos;

				Image footImage = footObj.GetComponent<Image>();
				if (i == pattern.anchorPoint1) {
					footImage.color = Color.green;
					tangible.anchor1 = footObj.transform;
				} else if (i == pattern.anchorPoint2) {
					footImage.color = Color.yellow;
					tangible.anchor2 = footObj.transform;
				} else {
					footImage.color = Color.grey;
				}
			}

			patternRectTransform.SetParent (parent);
			patternRectTransform.localPosition = Vector3.zero;
			if (setPosition) {
				
			}
			return patternObj;
		}
	}

}

