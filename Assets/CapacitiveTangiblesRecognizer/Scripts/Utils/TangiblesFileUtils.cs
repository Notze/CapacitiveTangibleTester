using UnityEngine;
using System;
using System.IO;

namespace CTR{
	public static class TangiblesFileUtils {

		static string tangibledirectory = "tangibles";
		static string statisticsDirectory = "statistics";
        static string testingDirectory = "testing";


        public static string TestingFilename()
        {
            if (!Directory.Exists(Application.persistentDataPath + "/" + testingDirectory))
            {
                Directory.CreateDirectory(Application.persistentDataPath + "/" + testingDirectory);
            }
            string fullpath = Application.persistentDataPath + "/" + testingDirectory + "/" + "stats_" + SystemInfo.deviceName + "_" + DateString() + ".csv";
            return fullpath;
        }

        public static string PatternFilename (string patternID) {
			if (!Directory.Exists (Application.persistentDataPath + "/" + tangibledirectory)) {
				Directory.CreateDirectory (Application.persistentDataPath + "/" + tangibledirectory);
			}

			string filename = "tangible_" + patternID + ".json";
			string fullpath = Application.persistentDataPath + "/" + tangibledirectory + "/" + filename;
			return fullpath;
		}

		public static string StatisticsFilename() {
			if(!Directory.Exists(Application.persistentDataPath + "/" + statisticsDirectory)) {
				Directory.CreateDirectory(Application.persistentDataPath + "/" + statisticsDirectory);
			}
			string fullpath = Application.persistentDataPath + "/" + statisticsDirectory + "/" + "stats_" + SystemInfo.deviceName + "_" + DateString() +  ".csv";
			return fullpath;
		}

		public static string [] LoadTangiblesJSON () {
			if (!Directory.Exists (Application.persistentDataPath + "/" + tangibledirectory)) {
				return null;
			} else {
				return Directory.GetFiles (Application.persistentDataPath + "/" + tangibledirectory, "*.json");
			}
		}

		public static string DateString() {
			DateTime dateTime = DateTime.UtcNow;
			string str = string.Format("{0}_{1}_{2}_{3}_{4}_{5}",dateTime.Year,dateTime.Month,dateTime.Day,dateTime.Hour,dateTime.Minute,dateTime.Second);
			return str;
		}

		public static void DeleteTangibles () {
			DirectoryInfo di = new DirectoryInfo (Application.persistentDataPath + "/" + tangibledirectory);
			foreach (FileInfo file in di.GetFiles ()) {
				file.Delete ();
			}
		}
	}
}


