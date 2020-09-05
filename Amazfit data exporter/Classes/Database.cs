using System.Data;
using System.Data.SQLite;
using System.IO;
using static Amazfit_data_exporter.Classes.Messenger;
using static Amazfit_data_exporter.Classes.Tools;

namespace Amazfit_data_exporter.Classes {
	public class Database {
		private SQLiteConnection _db;
		private const string GetAllWorkoutsCmd = "SELECT * FROM sport_summary WHERE parent_trackid=-1 AND NOT current_status=7 ORDER BY track_id";
		
		public Database(string databasePath) {
			_db = new SQLiteConnection("Data Source=" + databasePath);
			_db.Open();
		}

		public DataTable getAllWorkouts() {
			var getAllWorkoutsSqlCmd = new SQLiteCommand(GetAllWorkoutsCmd, _db);
			var workouts = new DataTable();
			using (var reader = getAllWorkoutsSqlCmd.ExecuteReader()) {
				workouts.Load(reader);
			}

			return workouts;
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