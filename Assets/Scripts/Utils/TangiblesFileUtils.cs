using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TangiblesFileUtils{

    public static string PatternFilename(string patternID){
        string filename = "tangible_" + patternID + ".json";
        string fullpath = Application.persistentDataPath + "/" + filename;
        return fullpath;
    }
}
