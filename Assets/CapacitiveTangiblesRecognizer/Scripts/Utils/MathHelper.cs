using System.Collections.Generic;
using UnityEngine;

namespace CTR{
	public static class MathHelper {

		public static Vector2 ComputeCenter (List<Vector2> points, Color color)
		{
			Vector2 center = Vector2.zero;
			foreach (Vector2 p in points) {
				center += p;
			}
			center /= points.Count;

			foreach (Vector2 p in points) {
				Debug.DrawLine (center, p, color, 30);
			}

			return center;
		}

		public static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles) {
			return Quaternion.Euler(angles) * (point - pivot) + pivot;
		}


		public static void DrawCircle (Vector2 center, float radius, int segments, Color color)
		{
			float angle = 360 / segments;
			List<Vector3> points = new List<Vector3> ();
			for (int i = 0; i < (segments + 1); i++) {
				float x = center.x + Mathf.Sin (Mathf.Deg2Rad * angle) * radius;
				float y = center.y + Mathf.Cos (Mathf.Deg2Rad * angle) * radius;
				points.Add (new Vector3 (x, y, 0));

				angle += (360f / segments);
			}
			for (int i = 1; i < points.Count; i++) {
				Debug.DrawLine (points [i - 1], points [i], color, 30);
			}
		}

		public static Rect RectTransformToScreenSpace (RectTransform transform)
		{
			Vector2 size = Vector2.Scale (transform.rect.size, transform.lossyScale);
			float x = transform.position.x + transform.anchoredPosition.x;
			float y = Screen.height - transform.position.y - transform.anchoredPosition.y;

			return new Rect (x - size.x, y + Screen.height - size.y, size.x, size.y);
		}
	}
}


