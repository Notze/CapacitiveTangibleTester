using System.Collections.Generic;
using UnityEngine;

namespace CTR{
	public static class MathHelper {
		public static Vector2 ComputeCenter (List<Vector2> points) {
			Vector2 center = Vector2.zero;
			foreach (Vector2 p in points) {
				center += p;
			}
			center /= points.Count;
			return center;
		}

		public static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles) {
			return Quaternion.Euler(angles) * (point - pivot) + pivot;
		}

        // Rotates a Vector2.
        public static Vector2 RotateVector(Vector2 v, float degrees)
        {
            float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
            float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

            float tx = v.x;
            float ty = v.y;
            v.x = (cos * tx) - (sin * ty);
            v.y = (sin * tx) + (cos * ty);
            return v;
        }

        public static void DrawCircle (Vector2 center, float radius, int segments, Color color) {
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

		public static Rect RectTransformToScreenSpace (RectTransform transform) {
			Vector2 size = Vector2.Scale (transform.rect.size, transform.lossyScale);
			float x = transform.position.x + transform.anchoredPosition.x;
			float y = Screen.height - transform.position.y - transform.anchoredPosition.y;

			return new Rect (x - size.x, y + Screen.height - size.y, size.x, size.y);
		}

        // Takes a list of n Vectors and returns a list of the three Vectors 
        // that create the shortest path.
        // That List is ordered so that the first two entries habe the smallest 
        // distance between each other.
        public static List<Vector2> FindMindDistTriplet(List<Vector2> patternPoints, out float minDist)
        {
            minDist = -1f;
            List<Vector2> output = new List<Vector2>();

            // create list of all distances between two points
            List<Tuple<float, Tuple<int, int>>> distList = new List<Tuple<float, Tuple<int, int>>>();
            for (int i = 0; i < patternPoints.Count; i++)
                for (int j = i + 1; j < patternPoints.Count; j++)
                {
                    float dist = Vector2.Distance(patternPoints[i], patternPoints[j]);
                    distList.Add(new Tuple<float, Tuple<int, int>>(dist, new Tuple<int, int>(i,j)));
                }
            // create ordered list of all combinations of two elements of previously created list
            SortedList<float, Tuple<int, int>> distCombinationList = new SortedList<float, Tuple<int, int>>();
            for (int i=0; i<distList.Count; i++)
                for (int j=i+1; j<distList.Count; j++)
                {
                    float combinedDist = distList[i].first + distList[j].first;
                    distCombinationList.Add(combinedDist, new Tuple<int,int>(i,j));
                }
            // find fist entry of previously created list that consist of only 3 indiviual points
            foreach(KeyValuePair<float,Tuple<int,int>> distCombination in distCombinationList)
            {
                int patternPointIndexA1 = distList[distCombination.Value.first].second.first;
                int patternPointIndexA2 = distList[distCombination.Value.first].second.second;
                int patternPointIndexB1 = distList[distCombination.Value.second].second.first;
                int patternPointIndexB2 = distList[distCombination.Value.second].second.second;
                if (patternPointIndexA1 == patternPointIndexB1)
                {
                    if (distList[distCombination.Value.first].first < distList[distCombination.Value.second].first)
                    {
                        minDist = distList[distCombination.Value.first].first;
                        output.Add(patternPoints[patternPointIndexA1]);
                        output.Add(patternPoints[patternPointIndexA2]);
                        //output.Add(patternPoints[patternPointIndexB1]);
                        output.Add(patternPoints[patternPointIndexB2]);
                        break;
                    }
                    else
                    {
                        minDist = distList[distCombination.Value.second].first;
                        output.Add(patternPoints[patternPointIndexB1]);
                        output.Add(patternPoints[patternPointIndexB2]);
                        //output.Add(patternPoints[patternPointIndexA1]);
                        output.Add(patternPoints[patternPointIndexA2]);
                        break;
                    }
                }else if (patternPointIndexA1 == patternPointIndexB2)
                {
                    if (distList[distCombination.Value.first].first < distList[distCombination.Value.second].first)
                    {
                        minDist = distList[distCombination.Value.first].first;
                        output.Add(patternPoints[patternPointIndexA1]);
                        output.Add(patternPoints[patternPointIndexA2]);
                        output.Add(patternPoints[patternPointIndexB1]);
                        //output.Add(patternPoints[patternPointIndexB2]);
                        break;
                    }
                    else
                    {
                        minDist = distList[distCombination.Value.second].first;
                        output.Add(patternPoints[patternPointIndexB1]);
                        output.Add(patternPoints[patternPointIndexB2]);
                        //output.Add(patternPoints[patternPointIndexA1]);
                        output.Add(patternPoints[patternPointIndexA2]);
                        break;
                    }
                }else if (patternPointIndexA2 == patternPointIndexB1)
                {
                    if (distList[distCombination.Value.first].first < distList[distCombination.Value.second].first)
                    {
                        minDist = distList[distCombination.Value.first].first;
                        output.Add(patternPoints[patternPointIndexA1]);
                        output.Add(patternPoints[patternPointIndexA2]);
                        //output.Add(patternPoints[patternPointIndexB1]);
                        output.Add(patternPoints[patternPointIndexB2]);
                        break;
                    }
                    else
                    {
                        minDist = distList[distCombination.Value.second].first;
                        output.Add(patternPoints[patternPointIndexB1]);
                        output.Add(patternPoints[patternPointIndexB2]);
                        output.Add(patternPoints[patternPointIndexA1]);
                        //output.Add(patternPoints[patternPointIndexA2]);
                        break;
                    }
                }else if (patternPointIndexA2 == patternPointIndexB2)
                {
                    if (distList[distCombination.Value.first].first < distList[distCombination.Value.second].first)
                    {
                        minDist = distList[distCombination.Value.first].first;
                        output.Add(patternPoints[patternPointIndexA1]);
                        output.Add(patternPoints[patternPointIndexA2]);
                        output.Add(patternPoints[patternPointIndexB1]);
                        //output.Add(patternPoints[patternPointIndexB2]);
                        break;
                    }
                    else
                    {
                        minDist = distList[distCombination.Value.second].first;
                        output.Add(patternPoints[patternPointIndexB1]);
                        output.Add(patternPoints[patternPointIndexB2]);
                        output.Add(patternPoints[patternPointIndexA1]);
                        //output.Add(patternPoints[patternPointIndexA2]);
                        break;
                    }
                }
            }

            return output;
        }

        // Takes a list of 3 Vectors and returns it ordered so that the first 
        // two entries are the Vectors with the smallest distance between each 
        // other.
        public static List<Vector2> SortPatternPointsByDistance(List<Vector2> patternPoints, out float minDist) {

            minDist = float.MaxValue;
            if (patternPoints.Count != 3) return null;

            // find the minDistancePair
            int minDistPairIndex1 = -1;
            int minDistPairIndex2 = -1;
            for (int i = 0; i < patternPoints.Count; i++)
                for (int j = i + 1; j < patternPoints.Count; j++)
                {
                    float dist = Vector2.Distance(patternPoints[i], patternPoints[j]);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        minDistPairIndex1 = i;
                        minDistPairIndex2 = j;
                    }
                }

            // build the ordered output list
            List<Vector2> output = new List<Vector2>();
            output.Add(patternPoints[minDistPairIndex1]);
            output.Add(patternPoints[minDistPairIndex2]);
            for (int i=0; i<=3; i++)
                if(i!=minDistPairIndex1 && i != minDistPairIndex2)
                {
                    output.Add(patternPoints[i]);
                    break;
                }

            return output;
        }

        // unused TODO delete
		public static Tuple<int, int> FindMinDistancePair(List<Vector2> patternPoints, Vector2 patternCenter, out float minDist) {
			Tuple<int, int> minDistPair = new Tuple<int, int> ();
			minDistPair.first = -1;
			minDistPair.second = -1;
			minDist = float.MaxValue;
			if (patternPoints.Count > 2) {	
				for (int i = 0; i < patternPoints.Count; i++) {
					for (int j = 0; j < patternPoints.Count; j++) {
						if (i != j) {
							float dist = Vector2.Distance (patternPoints [i], patternPoints [j]);
							if (dist < minDist) {
								minDist = dist;
								minDistPair.first = i;
								minDistPair.second = j;
							}
						}
					}
				}
				float firstDistFromCenter = Vector2.Distance (patternPoints [minDistPair.first], patternCenter);
				float secondDistFromCenter = Vector2.Distance (patternPoints [minDistPair.second], patternCenter);

				//if (secondDistFromCenter > firstDistFromCenter && GlobalSettings.Instance.flipRotation) {
				//	int tmp = minDistPair.first;
				//	minDistPair.first = minDistPair.second;
				//	minDistPair.second = tmp;
				//}
			}
			return minDistPair;
		}

		public static float NormalDistribution(float x, float m, float s){
			float pi = Mathf.PI;
			return Mathf.Exp (-Mathf.Pow (x - m, 2) / (2 * s*s)) / (s*Mathf.Sqrt(2 * pi));
		}


		public static Vector2 FindClosestPoint(Vector2 point, List<Vector2> points){
			Vector2 closestPoint = Vector2.zero;
			float minDist = float.MaxValue;
			foreach(Vector2 pt in points){
				float dist = Vector2.Distance(point,pt);
				if(dist < minDist){
					minDist = dist;
					closestPoint = pt;
				}
			}
			return closestPoint;
		}

		//public static float ProbabilityOfValue(float zmin, float zmax){
		//	float p = 0;
		//	float z = zmin;
		//	while(z <= zmax){
		//		p += NormalDistribution(z, 0, 1);
		//		z += 0.01f;
		//	}
		//	return p;
		//}
	}
}


