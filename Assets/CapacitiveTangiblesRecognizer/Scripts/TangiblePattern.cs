using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


namespace CTR
{
    [Serializable]
    public struct TangiblePattern
    {
        [NonSerialized]
        public const int numOfPoints = 3;
        public int id;
		public int trainingSamples; 
		public float radius; // TODO 

		// TODO should be obsolete after adaption of monitoring
		public List<float> standardDeviations;
        public List<float> meanDistances;
        public float anchorDistance;
        public int anchorPoint1;
        public int anchorPoint2;
        public int infoPoint1;
        public int infoPoint2;
		public List<Vector2> points; 


		public Tuple<int, int> infoCoord;

        public override string ToString()
        {
            string str = string.Empty;
            str += "id: " + id + "\n";
			str += "training samples: " + trainingSamples + "\n";
			str += "infoCoord: " + infoCoord.ToString() + "\n";
            return str;
        }
    }

}