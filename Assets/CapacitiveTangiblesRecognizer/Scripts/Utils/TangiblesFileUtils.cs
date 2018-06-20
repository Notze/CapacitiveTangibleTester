﻿using UnityEngine;
using System.IO;

namespace CTR{
	public static class TangiblesFileUtils {

		static string tangibledirectory = "tangibles";
		public static string PatternFilename (string patternID)
		{

			if (!Directory.Exists (Application.persistentDataPath + "/" + tangibledirectory)) {
				Directory.CreateDirectory (Application.persistentDataPath + "/" + tangibledirectory);
			}

			string filename = "tangible_" + patternID + ".json";
			string fullpath = Application.persistentDataPath + "/" + tangibledirectory + "/" + filename;
			return fullpath;
		}

		public static string [] LoadTangiblesJSON ()
		{
			if (!Directory.Exists (Application.persistentDataPath + "/" + tangibledirectory)) {
				return null;
			} else {
				return Directory.GetFiles (Application.persistentDataPath + "/" + tangibledirectory, "*.json");
			}
		}

		public static void DeleteTangibles ()
		{

			DirectoryInfo di = new DirectoryInfo (Application.persistentDataPath + "/" + tangibledirectory);
			foreach (FileInfo file in di.GetFiles ()) {
				file.Delete ();
			}
		}
	}
}


