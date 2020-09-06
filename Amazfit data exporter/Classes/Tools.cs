using System;
using System.IO;
using System.Collections.Generic;
using System.Net;
using System.Net.Cache;
using static Amazfit_data_exporter.Classes.Paths;

namespace Amazfit_data_exporter.Classes {
	public static class Tools {
		private static readonly List<long> SportsWithoutGps = new List<long>(new long[] {8, 10, 12, 14, 17, 21});

		public static string checkVersion() {
			var cachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);
			var req = WebRequest.Create(VersionUrl);
			req.CachePolicy = cachePolicy;
			var version = new StreamReader(req.GetResponse().GetResponseStream()).ReadLine();
			return version;
		}
		
		public static DateTime dateConvertor(long dateFromDb) {
			dateFromDb /= 1000; //to seconds
			var date = new DateTime(1970, 1, 1, 0, 0, 0); //Unix timestamp of start (value 0)
			date = date.AddSeconds(dateFromDb);
			return date;
		}

		public static bool sportWithoutGps(long sportNumber) {
			return SportsWithoutGps.Contains(sportNumber);
		}

		public static string sportName(long sportNumber) {
			string sportName;

			switch (sportNumber) {
				case 1:
					sportName = "Running";
					break;
				case 6:
					sportName = "Walking";
					break;
				case 7:
					sportName = "Trail run";
					break;
				case 8:
					sportName = "Treadmill";
					break;
				case 9:
					sportName = "Outdoor cycling";
					break;
				case 10:
					sportName = "Indoor cycling";
					break;
				case 12:
					sportName = "Elliptical";
					break;
				case 13:
					sportName = "Climbing";
					break;
				case 14:
					sportName = "Indoor swimming";
					break;
				case 15:
					sportName = "Outdoor swimming";
					break;
				case 17:
					sportName = "Tennis";
					break;
				case 18:
					sportName = "Soccer";
					break;
				case 21:
					sportName = "Rope";
					break;
				case 2001:
					sportName = "Triathlon";
					break;
				case 2002:
					sportName = "Multisport";
					break;
				default:
					sportName = "";
					break;
			}

			return sportName;
		}

		public static void checkFolders() {
			Directory.CreateDirectory(LastExportFolder.cleanPath());
			Directory.CreateDirectory(WorkoutsByDateFolder.cleanPath());

			Directory.CreateDirectory(BackupFolder.cleanPath());
			Directory.CreateDirectory(TempFolder.cleanPath());
			
			Directory.CreateDirectory(workoutSportNameFolder("Indoor cycling").cleanPath());
			Directory.CreateDirectory(workoutSportNameFolder("Indoor swimming").cleanPath());
			Directory.CreateDirectory(workoutSportNameFolder("Outdoor cycling").cleanPath());
			Directory.CreateDirectory(workoutSportNameFolder("Outdoor swimming").cleanPath());
			Directory.CreateDirectory(workoutSportNameFolder("Rope").cleanPath());
			Directory.CreateDirectory(workoutSportNameFolder("Running").cleanPath());
			Directory.CreateDirectory(workoutSportNameFolder("Tennis").cleanPath());
			Directory.CreateDirectory(workoutSportNameFolder("Trail run").cleanPath());
			Directory.CreateDirectory(workoutSportNameFolder("Treadmill").cleanPath());
			Directory.CreateDirectory(workoutSportNameFolder("Walking").cleanPath());
			Directory.CreateDirectory(workoutSportNameFolder("Unknown").cleanPath());
		}
	}
}