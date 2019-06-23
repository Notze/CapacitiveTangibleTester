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
        // serialized
        public Type type;
        public string _id;
        public int infoX, infoY;
        public float a1X, a1Y, a2X, a2Y, iX, iY; // anchor and id point coordinates
        public float gridSize;
        
        // non-serialized
        public enum Type { PLAIN, UDP };
        public Vector2 Position { get { return anchorPoint2; } }
        public Vector2 anchorPoint1 { get { return new Vector2(a1X, a1Y); } set { a1X = value.x; a1Y = value.y; } }
        public Vector2 anchorPoint2 { get { return new Vector2(a2X, a2Y); } set { a2X = value.x; a2Y = value.y; } }
        public Vector2 idPoint { get { return new Vector2(iX, iY); } set { iX = value.x; iY = value.y; } }
        public List<Vector2> OrderedTouchPoints { get {
                return new List<Vector2>(new Vector2[] { anchorPoint1, anchorPoint2, idPoint }); } }
        public string id {
            get
            {
                switch (type)
                {
                    case Type.PLAIN:
                        return infoCoord.ToString();
                    case Type.UDP:
                        return _id;
                }
                return null;
            }
        }
        public Tuple<int, int> infoCoord {
            get { return new Tuple<int,int>(infoX, infoY); }
            set { infoX = value.first; infoY = value.second; }
        }
        [NonSerialized] public float orientation;



        public override string ToString()
        {
            return ToString(false);
        }

        public string ToString(bool verbose)
        {
            if (verbose)
            {
                string str = string.Empty;
                str += "id: " + id + "\n";
                str += "type: " + type.ToString() + "\n";
                str += "infoCoord: " + infoCoord.ToString() + "\n";
                str += "gridSize: " + gridSize.ToString() +"\n";
                str += "position: " + Position.ToString() +"\n";
                str += "orientation: " + orientation.ToString() +"\n";
                return str;
            }
            else
            {
                string str = string.Empty;
                str += "id: " + id + "\n";
                str += "type: " + type.ToString() + "\n";
                return str;
            }
            
        }
    }

}