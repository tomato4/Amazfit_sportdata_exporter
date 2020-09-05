﻿using System.Data;
using System.Data.SQLite;
using System.IO;
using static Amazfit_data_exporter.Classes.Messenger;
using static Amazfit_data_exporter.Classes.Tools;

namespace Amazfit_data_exporter.Classes {
	public class Database {
		private SQLiteConnection _db;
		//private const string GetAllWorkoutsCmd = "SELECT * FROM sport_summary WHERE parent_trackid=-1 AND NOT current_status=7 ORDER BY track_id";
		
		public Database(string databasePath) {
			_db = new SQLiteConnection("Data Source=" + databasePath);
			_db.Open();
		}

		public static string queryBuilder(string tableName, string[] columnSelect = null, string[] conditions = null, string orderBy = null) {
			//TODO temporary query builder - bad way to create sql command
			var select = "SELECT ";
			var where = "";
			var order = "";

			if (columnSelect != null) {
				foreach (var column in columnSelect) {
					select += column + ",";
				}
				
				if (columnSelect.Length > 0) {
					select = select.Remove(select.Length - 1, 1);
				}
			}
			else {
				select += "*";
			}
			select += " FROM " + tableName;

			if (conditions != null && conditions.Length > 0) {
				where = " WHERE ";
				foreach (var cond in conditions) {
					where += cond + " AND ";
				}
				where = where.Remove(where.Length - 5, 5);
			}

			if (orderBy != null) {
				order = " ORDER BY " + orderBy;
			}

			return select + where + order;
		}

		public DataTable executeQuery(string query) {
			var cmd = new SQLiteCommand(query, _db);
			var result = new DataTable();
			using (var reader = cmd.ExecuteReader()) {
				result.Load(reader);
			}
			return result;
		}

		public DataTable getAllWorkouts() {
			return executeQuery(queryBuilder("sport_summary", null, new string[2] {"parent_trackid=-1", "NOT current_status=7"}, "track_id"));
		}

		public static void writeWorkouts(DataTable workouts) {
			//show list of all workouts
			foreach (DataRow row in workouts.Rows) {
				writeWorkout(row);
			}
		}

		private static void writeWorkout(DataRow workout) {
			//show given workout
			var startTime = dateConvertor((long)workout["start_time"]);
			var startTimeString = startTime.ToString("yyyy_MM_dd HH:mm");
			var sportNumber = (long)workout["type"];
			var name = sportName(sportNumber);
			
			sendMessage(startTimeString + " -" + name + "- ", DefaultMsg, false);
			if (name == "")
				sendMessage("[Warning: Unknown type of sport]", ErrorMsg);
			else if (File.Exists(@".\Exported workouts\Ordered by date\" + startTimeString + " " + name + ".tcx"))
				sendMessage("[Skipping: Already exported]", LowInfoMsg);
			else if (name == "Multisport" || name == "Triathlon")
				sendMessage("[Skipping: Triathlon/Multisport is not supported]");
			else if (!sportWithoutGps(sportNumber))
				sendMessage("[Skipping: contains GPS data]", InfoMsg);
			else
				sendMessage("[Will be exported]", SuccessMsg);
		}
	}
}