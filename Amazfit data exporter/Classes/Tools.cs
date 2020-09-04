﻿using System;
using System.IO;
using System.Collections.Generic;

namespace Amazfit_data_exporter.Classes {
	public static class Tools {
		private static readonly List<long> SportsWithoutGps = new List<long>(new long[] { 8, 10, 12, 14, 17, 21 });

		public static DateTime dateConvertor(long dateFromDb) {
			dateFromDb /= 1000; //to seconds
			var date = new DateTime(1970, 1, 1, 0, 0, 0);
			date = date.AddSeconds(dateFromDb);
			return date;
		}
		
		public static bool sportWithoutGps(long sportNumber) {
			return SportsWithoutGps.Contains(sportNumber);
		}

		public static string sportName(long sportNumber) {
			string sportName;

			switch (sportNumber)
			{
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
			Directory.CreateDirectory(@".\Exported workouts\Last export\");
			Directory.CreateDirectory(@".\Exported workouts\Ordered by date\");

			Directory.CreateDirectory(@".\Data\backup\");
			Directory.CreateDirectory(@".\Data\temp\");

			Directory.CreateDirectory(@".\Exported workouts\Ordered by sport\Indoor cycling\");
			Directory.CreateDirectory(@".\Exported workouts\Ordered by sport\Indoor swimming\");
			Directory.CreateDirectory(@".\Exported workouts\Ordered by sport\Outdoor cycling\");
			Directory.CreateDirectory(@".\Exported workouts\Ordered by sport\Outdoor swimming\");
			Directory.CreateDirectory(@".\Exported workouts\Ordered by sport\Rope\");
			Directory.CreateDirectory(@".\Exported workouts\Ordered by sport\Running\");
			Directory.CreateDirectory(@".\Exported workouts\Ordered by sport\Tennis\");
			Directory.CreateDirectory(@".\Exported workouts\Ordered by sport\Trail run\");
			Directory.CreateDirectory(@".\Exported workouts\Ordered by sport\Treadmill\");
			Directory.CreateDirectory(@".\Exported workouts\Ordered by sport\Walking\");
		}
	}
}