using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Amazfit_data_exporter.Classes {
	public static class Paths {
		//url for version check
		public const string VersionUrl = "https://raw.githubusercontent.com/tomato4/Amazfit_sportdata_exporter/master/version";

		//basic paths
		public static readonly string MainFullPath = Directory.GetCurrentDirectory() + @"\";
		public static readonly string AdbPath = MainFullPath + "adb.exe";
		
		//data for export path - for app
		public static readonly string DataPath = MainFullPath + @"Data\";
		public static readonly string BackupFolder = DataPath + @"backup\";
		public static readonly string TempFolder = DataPath + @"temp\";

		//workouts path - data, that user wants
		public static readonly string ExportedWorkoutsFolder = MainFullPath + @"..\Exported workouts\";
		public static readonly string LastExportFolder = ExportedWorkoutsFolder + @"Last export\";
		public static readonly string WorkoutsByDateFolder = ExportedWorkoutsFolder + @"Ordered by date\";
		public static readonly string WorkoutsBySportFolder = ExportedWorkoutsFolder + @"Ordered by sport\";

		//get workout sport name specific folder
		public static string workoutSportNameFolder(string sportName) {
			return WorkoutsBySportFolder + sportName + @"\";
		}

		//get file path in date folder
		public static string workoutDateFolderFilePath(string sportName, string startTime) {
			if (sportName == "")
				sportName = "Unknown";
			return WorkoutsByDateFolder + startTime + " " + sportName + ".tcx";
		}

		//get file path of sport name specific folder
		public static string workoutSportNameFolderFilePath(string sportName, string startTime) {
			return workoutSportNameFolder(sportName) + startTime + ".tcx";
		}
		
		//get file path of last export folder
		public static string workoutLastExportFolderFilePath(string sportName, string startTime) {
			return LastExportFolder + startTime + " " + sportName + ".tcx";
		}

		public static string extractedTarFolder(string timeStamp) {
			return TempFolder + timeStamp + @"\";
		}
		
		//get database file path
		public static string databaseFilePath(string timeStamp) {
			return extractedTarFolder(timeStamp) + @"apps\com.huami.watch.newsport\db\sport_data.db";
		}

		//get backup file path
		public static string backupFilePath(string timeStamp) {
			return BackupFolder + timeStamp + ".ab";
		}

		//get tar file path
		public static string tarFilePath(string timeStamp) {
			return TempFolder + timeStamp + ".tar";
		}

		public static string cleanPath(this string path, WrapStyle wrap = WrapStyle.None) {
			switch (wrap) {
				case WrapStyle.None:
					return cleanPathFromBackFolder(path);
				case WrapStyle.SingleQuotes:
					return "'" + cleanPathFromBackFolder(path) + "'";
				case WrapStyle.DoubleQuotes:
					return "\"" + cleanPathFromBackFolder(path) + "\"";
				default:
					throw new ArgumentOutOfRangeException(nameof(wrap), wrap, "wrap style exception");
			}
		}

		private static string cleanPathFromBackFolder(string path) {
			return Regex.Replace(path, @"(\w| )+\\\.\.\\", "");
		}
	}
}