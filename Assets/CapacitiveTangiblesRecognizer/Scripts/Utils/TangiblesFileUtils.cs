using UnityEngine;
using System;
using System.IO;

namespace CTR{
	public static class TangiblesFileUtils {

		static string tangibledirectory = "tangibles";
        static string testingDirectory = "testing";


        // Builds a filename for the  log file.
        public static string TestingFilename()
        {
            if (!Directory.Exists(Application.persistentDataPath + "/" + testingDirectory))
                Directory.CreateDirectory(Application.persistentDataPath + "/" + testingDirectory);
            return Application.persistentDataPath + "/" + testingDirectory + "/" + "testrun_" + SystemInfo.deviceName + "_" + DateString() + ".csv";
        }

        // Builds the filename for a given pattern.
        public static string PatternFilename (string patternID) {
			if (!Directory.Exists (Application.persistentDataPath + "/" + tangibledirectory))
				Directory.CreateDirectory (Application.persistentDataPath + "/" + tangibledirectory);

			string filename = "tangible_" + patternID + ".json";
			return Application.persistentDataPath + "/" + tangibledirectory + "/" + filename;
		}

        // Returns a list of absolute filenames of persistent pattern data.
		public static string [] LoadTangiblesJSON () {
			if (!Directory.Exists (Application.persistentDataPath + "/" + tangibledirectory))
				return null;
			else
				return Directory.GetFiles (Application.persistentDataPath + "/" + tangibledirectory, "*.json");
		}

        // Builds a nicely formatted timestamp.
		public static string DateString() {
			DateTime dateTime = DateTime.UtcNow;
			return string.Format("{0}_{1}_{2}_{3}_{4}_{5}",dateTime.Year,dateTime.Month,dateTime.Day,dateTime.Hour,dateTime.Minute,dateTime.Second);
		}

        // Deletes all persistent tangible pattern data.
		public static void DeleteTangibles () {
			DirectoryInfo di = new DirectoryInfo (Application.persistentDataPath + "/" + tangibledirectory);
			foreach (FileInfo file in di.GetFiles ())
				file.Delete ();
		}
	}
}


