// Instances of this class hold all relevant information about a specific
// tangible pattern and can be used to persistently store patterns.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


namespace CTR
{
    [Serializable]
    public struct TangiblePattern
    {
        [NonSerialized] public const int numOfPoints = 3;
        public enum Type { PLAIN, UDP };
        public Type type;
        public int id;
        public Tuple<int, int> infoCoord { get; set; }
        public float radius; // radius for clustering
        public float gridSize;
        public float orientation { get; set; }
        public Vector2 position { get; set; }

        //public int trainingSamples; 

        // TODO should be obsolete after adaption of monitoring
        public List<float> standardDeviations;
        public List<float> meanDistances;
        public float anchorDistance;
        public int anchorPoint1;
        public int anchorPoint2;
        public int infoPoint1;
        public int infoPoint2;
        public List<Vector2> points;


        public override string ToString()
        {
            string str = string.Empty;
            str += "id: " + id + "\n";
			str += "type: " + type.ToString() + "\n";
			str += "infoCoord: " + infoCoord.ToString() + "\n";
            return str;
        }
    }

}