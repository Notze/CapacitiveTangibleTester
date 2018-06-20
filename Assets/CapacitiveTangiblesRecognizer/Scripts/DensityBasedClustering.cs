using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace CTR{
	[System.Serializable]
	public class DbscanPoint {
		public long pointID;
		public Vector2 point;

		[System.NonSerialized]
		public ClusterTouch clusterTouch;
		/// <summary>
		/// Gets or sets the X component of the point.
		/// </summary>
		public float X { get { return point.x; } set { point.x = value; } }

		/// <summary>
		/// Gets or sets the Y component of the point.
		/// </summary>
		public float Y { get { return point.y; } set { point.y = value; } }

		/// <summary>
		/// Gets or sets a value indicating whether the point is noise.
		/// </summary>
		public bool IsNoise;

		/// <summary>
		/// Gets or sets a value indicating whether the point was visited.
		/// </summary>
		public bool IsVisited;

		/// <summary>
		/// Gets or sets a value indicating the cluster id.
		/// </summary>
		public int ClusterId = -1;

		/// <summary>
		/// Initializes a new instance of the <see cref="DbscanPoint"/> class.
		/// </summary>
		public DbscanPoint (Vector2 p, long id)
		{
			point = p;
			pointID = id;
		}


		public void Reset ()
		{
			IsNoise = false;
			IsVisited = false;
			ClusterId = -1;
		}


		/// <summary>
		/// Determines whether the specified point neighbors the current instance using the specified value.
		/// </summary>
		/// <param name="point">The point to test as a neighor.</param>
		/// <param name="eps">The value to use to find neighoring points.</param>
		/// <returns>True if the point is a neighbor; otherwise, false.</returns>
		public bool IsNeighbor (DbscanPoint point, float eps)
		{
			return (Mathf.Pow (point.X - this.X, 2) + Mathf.Pow (point.Y - this.Y, 2) < Mathf.Pow (eps, 2));
		}
	}

	public static class DensityBasedClustering {


		/// <summary>
		/// Clusters the specified points using the specified value and minimum points to form a cluster.
		/// </summary>
		/// <param name="points">The points to cluster.</param>
		/// <param name="eps">The value to use to find neighoring points.</param>
		/// <param name="minimumClusterCount">The minimum number of points to form a cluster.</param>
		/// <returns>The number of clusters created from the collection.</returns>
		public static int DBScan (List<DbscanPoint> points, float eps, int minimumClusterCount)
		{
			int cid = 0;

			foreach (DbscanPoint p in points) {
				if (!p.IsVisited) {
					p.IsVisited = true;

					DbscanPoint [] neighbors = GetNeighors (points, p, eps);

					if (neighbors.Length < minimumClusterCount)
						p.IsNoise = true;
					else {
						cid++;
						ExpandCluster (points, p, neighbors, cid, eps, minimumClusterCount);
					}
				}
			}

			return cid;
		}

		private static void ExpandCluster (List<DbscanPoint> points, DbscanPoint p, DbscanPoint [] neighbors, int cid, float eps, int minimumClusterCount)
		{
			p.ClusterId = cid;

			Queue<DbscanPoint> q = new Queue<DbscanPoint> (neighbors);

			while (q.Count > 0) {
				DbscanPoint n = q.Dequeue ();
				if (!n.IsVisited) {
					n.IsVisited = true;

					DbscanPoint [] ns = GetNeighors (points, n, eps);
					if (ns.Length >= minimumClusterCount) {
						foreach (DbscanPoint item in ns) {
							q.Enqueue (item);
						}
					}
				} else if (n.ClusterId == -1) {
					n.ClusterId = cid;
				}

			}
		}

		private static DbscanPoint [] GetNeighors (List<DbscanPoint> points, DbscanPoint point, float eps)
		{
			List<DbscanPoint> neighbors = new List<DbscanPoint> ();
			neighbors.Add (point);

			foreach (DbscanPoint p in points)
				if (point.IsNeighbor (p, eps))
					neighbors.Add (p);

			return neighbors.ToArray ();
		}
	}
}

