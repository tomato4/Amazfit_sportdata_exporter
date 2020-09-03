using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Data;
using System.Data.SQLite;
using SharpAdbClient;
using ICSharpCode.SharpZipLib.Tar;
using static Amazfit_data_exporter.Messenger;

namespace Amazfit_data_exporter {
	internal static class Program {
		private const string Version = "2.0";
		private static readonly string Directory = System.IO.Directory.GetCurrentDirectory() + @"\";
		
		// ReSharper disable once InconsistentNaming
		public static void Main() {
			sendMessage("Amazfit watch nonGPS sport data exporter by Tomato4444; Version: " + Version + "\n", SuccessMsg);
			sendMessage("For support or suggestions contact me on Reddit\nReddit: https://www.reddit.com/user/Tomato4444/\nReddit message: https://www.reddit.com/message/compose/?to=Tomato4444\n\n" +
			"Connect your Amazfit into you PC. Make sure your drivers for your watch are installed properly. After confirmation program starts with getting data from your watch. You will need to agree with notification popped up on your watch. This process could take some while (up to minute).\n"
			);
			
			//ask if user wants to continue
			ConsoleKey response;
			do {
				sendMessage("Do you want to continue? [y/n]", InfoMsg);
				response = Console.ReadKey(true).Key;
				if (response != ConsoleKey.Enter)
					Console.WriteLine();
			} while (response != ConsoleKey.Y && response != ConsoleKey.N);
			if (response == ConsoleKey.N)
				abort(false);
			
			//check folder structure
			checkFolders();
			//delete previous workouts
			var lastExportFolder = new DirectoryInfo(@".\Exported workouts\Last export\");
			foreach (var file in lastExportFolder.GetFiles()) {
				file.Delete();
			}
			
			//create timestamp for current export
			var timeStamp = DateTime.Now.ToString("yyyyMMddHHmmss");
			
			//extract data from Amazfit
			var extractor = new Extractor(Directory + "adb.exe");
			try { 
				extractor.extract(timeStamp);
			}
			catch (Exception e) {
				sendMessage("Error: " + e.Message, ErrorMsg);
				abort();
			}
			sendMessage("Data extraction from Amazfit completed.", SuccessMsg);
			sendNewLine();
			
			sendMessage("List of workouts in database:", InfoMsg);
			var db = new Database(@".\Data\temp\" + timeStamp + @"\apps\com.huami.watch.newsport\db\sport_data.db");
			Database.writeWorkouts(db.getAllWorkouts());
			
			abort();
		}

		private static void checkFolders() {
			System.IO.Directory.CreateDirectory(@".\Exported workouts\Last export\");
			System.IO.Directory.CreateDirectory(@".\Exported workouts\Ordered by date\");
			
			System.IO.Directory.CreateDirectory(@".\Data\backup\");
			System.IO.Directory.CreateDirectory(@".\Data\temp\");

			System.IO.Directory.CreateDirectory(@".\Exported workouts\Ordered by sport\Indoor cycling\");
			System.IO.Directory.CreateDirectory(@".\Exported workouts\Ordered by sport\Indoor swimming\");
			System.IO.Directory.CreateDirectory(@".\Exported workouts\Ordered by sport\Outdoor cycling\");
			System.IO.Directory.CreateDirectory(@".\Exported workouts\Ordered by sport\Outdoor swimming\");
			System.IO.Directory.CreateDirectory(@".\Exported workouts\Ordered by sport\Rope\");
			System.IO.Directory.CreateDirectory(@".\Exported workouts\Ordered by sport\Running\");
			System.IO.Directory.CreateDirectory(@".\Exported workouts\Ordered by sport\Tennis\");
			System.IO.Directory.CreateDirectory(@".\Exported workouts\Ordered by sport\Trail run\");
			System.IO.Directory.CreateDirectory(@".\Exported workouts\Ordered by sport\Treadmill\");
			System.IO.Directory.CreateDirectory(@".\Exported workouts\Ordered by sport\Walking\");
		}
		

		private static void abort(bool stop = true) {
			if (stop) {
				Console.WriteLine("\nPress enter to terminate job...");
				Console.ReadLine();
			}
			Environment.Exit(0);
		}
	}

	internal static class Messenger {
		public const ConsoleColor SuccessMsg = ConsoleColor.Green;
		public const ConsoleColor InfoMsg = ConsoleColor.Yellow;
		public const ConsoleColor LowInfoMsg = ConsoleColor.DarkYellow; //used for "skipping already exported"
		public const ConsoleColor WarningMsg = ConsoleColor.DarkMagenta;
		public const ConsoleColor ErrorMsg = ConsoleColor.Red;
		public const ConsoleColor LogMsg = ConsoleColor.DarkGray;
		public const ConsoleColor DefaultMsg = ConsoleColor.Gray;

		public static void sendMessage(string msg, ConsoleColor msgType = DefaultMsg, bool newLine = true) {
			Console.ForegroundColor = msgType;
			if (newLine)
				Console.WriteLine(msg);
			else 
				Console.Write(msg);
			Console.ResetColor();
		}

		public static void sendNewLine() {
			Console.WriteLine();
		}
	}

	//extracts data (database of records) from watches
	internal class Extractor {
		private readonly AdbServer _server = new AdbServer();
		private readonly AdbClient _adb = new AdbClient();
		private readonly MyAdbCommandLineClient _adbCmd;

		public Extractor(string adbPath) {
			_server.StartServer(adbPath,true);
			_adbCmd = new MyAdbCommandLineClient(adbPath);
		}

		public void extract(string timeStamp) {
			//check connected devices
			var devices = _adb.GetDevices();
			if (devices.Count == 0)
				throw new Exception("ADB error: no device detected. Please connect device and start program again.");
			if (devices.Count > 1) {
				sendMessage("ADB warning: more than 1 device detected. This could potentially cause problems. If export will be unsuccessful, please disconnect all devices but Amazfit.", WarningMsg);
				sendNewLine();
				sendMessage("List of connected devices:", WarningMsg);
				var count = devices.Count;
				for (var i = 0; i < count; i++) {
					sendMessage(i + ": " + devices[i].Name + " " + devices[i].Model + " " + devices[i].Product, WarningMsg);
				}
				sendNewLine();
			}
			
			//get backup of apk, that holds database
			sendMessage("Requesting backup on Amazfit...", LogMsg);
			_adbCmd.runAdbProcess(@"backup -noapk com.huami.watch.newsport -f .\Data\Backup\" + timeStamp + ".ab", null, null);
			sendMessage("Converting backup to .tar", LogMsg);
			Convertor.backupToTar(timeStamp);
			sendMessage("Extracting tar file", LogMsg);
			Convertor.extractTar(timeStamp);
		}
	}

	internal static class Convertor {
		public static int backupToTar(string timeStamp) {
			var psi = new ProcessStartInfo("java",
				@"-jar abe.jar unpack .\Data\backup\" + timeStamp + ".ab " + @".\Data\temp\" + timeStamp + ".tar") {
				CreateNoWindow = true,
				WindowStyle = ProcessWindowStyle.Hidden,
				UseShellExecute = false,
				RedirectStandardError = true,
				RedirectStandardOutput = true
			};

			var process = Process.Start(psi);

			// ReSharper disable once PossibleNullReferenceException
			if (!process.WaitForExit(5000)) {
				process.Kill();
			}

			return process.ExitCode;
		}

		public static void extractTar(string timeStamp) {
			extractTar(@".\Data\temp\" + timeStamp + ".tar", @".\Data\temp\" + timeStamp + @"\");
			
			//check existence of DB file
			var dbFilePath = @".\Data\temp\" + timeStamp + @"\apps\com.huami.watch.newsport\db\sport_data.db";
			if (!File.Exists(dbFilePath))
				throw new Exception("Database file not found. Probably error of data export. Try again...");
		}

		private static void extractTar(string sourcePath, string destPath) {
			if (!File.Exists(sourcePath))
				throw new Exception("Tar file not found. Probably error of data export. Try again...");
			
			Stream inStream = File.OpenRead(sourcePath);
			
			var tarArchive = TarArchive.CreateInputTarArchive(inStream);
			tarArchive.ExtractContents(destPath);
			
			tarArchive.Close();
			inStream.Close();
		}
	}

	internal class Database {
		public static readonly List<long> SportsWithoutGps = new List<long>(new long[] { 8, 10, 12, 14, 17, 21 });
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
			foreach (DataRow row in workouts.Rows) {
				writeWorkout(row);
			}
		}

		private static void writeWorkout(DataRow workout) {
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
		
		

		private static DateTime dateConvertor(long dateFromDb) {
			dateFromDb /= 1000; //to seconds
			var date = new DateTime(1970, 1, 1, 0, 0, 0);
			date = date.AddSeconds(dateFromDb);
			return date;
		}

		private static string sportName(long sportNumber) {
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

		private static bool sportWithoutGps(long sportNumber) {
			return SportsWithoutGps.Contains(sportNumber);
		}
	}
}

namespace SharpAdbClient {
	using Microsoft.Extensions.Logging;
	public class MyAdbCommandLineClient : AdbCommandLineClient {
		public MyAdbCommandLineClient(string adbPath, ILogger<AdbCommandLineClient> logger = null) : base(adbPath, logger) { }

		public void runAdbProcess(string command, List<string> errorOutput, List<string> standartOutput) {
			RunAdbProcess(command, errorOutput, standartOutput);
		}
	}
}