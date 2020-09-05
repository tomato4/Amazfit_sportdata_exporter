using System;
using System.IO;
using Amazfit_data_exporter.Classes;
using static Amazfit_data_exporter.Classes.Messenger;

namespace Amazfit_data_exporter {
	internal static class Program {
		private const string Version = "2.0";
		private static readonly string Directory = System.IO.Directory.GetCurrentDirectory() + @"\";

		// ReSharper disable once InconsistentNaming
		public static void Main() {
			sendMessage("Amazfit watch nonGPS sport data exporter by Tomato4444; Version: " + Version + "\n",
						SuccessMsg);
			sendMessage(
				"For support or suggestions contact me on Reddit\nReddit: https://www.reddit.com/user/Tomato4444/\nReddit message: https://www.reddit.com/message/compose/?to=Tomato4444\n\n" +
				"Connect your Amazfit into you PC. Make sure your drivers for your watch are installed properly. After confirmation program starts with getting data from your watch. You will need to agree with notification popped up on your watch. This process could take some while (up to minute).\n"
			);
			//TODO change contact method
			//TODO update check

			//ask if user wants to continue
			ConsoleKey response;
			do {
				sendMessage("Do you want to continue? [y/n]", InfoMsg);
				response = Console.ReadKey(true).Key;
				if (response != ConsoleKey.Enter)
					sendNewLine();
			} while (response != ConsoleKey.Y && response != ConsoleKey.N);

			if (response == ConsoleKey.N)
				abort(false);

			//check folder structure
			Tools.checkFolders();
			//delete previous workouts
			var lastExportFolder = new DirectoryInfo(@".\Exported workouts\Last export\");
			foreach (var file in lastExportFolder.GetFiles()) {
				file.Delete();
			}

			//create timestamp for current export
			// var timeStamp = DateTime.Now.ToString("yyyyMMddHHmmss");
			var timeStamp = "20200905210756"; //TODO remove - testing

			//extract data from Amazfit
			try {
				// todo remove - testing
				// var extractor = new Extractor(Directory + "adb.exe");
				// extractor.extract(timeStamp);
			}
			catch (Exception e) {
				sendMessage("Error: " + e.Message, ErrorMsg);
				abort();
			}

			sendMessage("Data extraction from Amazfit completed.", SuccessMsg);
			sendNewLine();

			sendMessage("List of workouts in database:", InfoMsg);
			var db = new Database(@".\Data\temp\" + timeStamp + @"\apps\com.huami.watch.newsport\db\sport_data.db");
			var allWorkouts = db.getAllWorkouts();
			//show list of all workouts
			Database.writeWorkouts(allWorkouts);
			sendNewLine();

			//check if there is unknown workout, that wasn't exported and if so ask if wanna export
			var answer = false;
			if (db.isThereUnknownSport(allWorkouts)) {
				sendMessage("There is unknown workout, that wasn't exported. Do you want to export these workouts too? (note: this might potentially cause export fail)");
				ConsoleKey response2;
				do {
					sendMessage("Do you want to export unknown file? [y/n]", InfoMsg);
					response2 = Console.ReadKey(true).Key;
					if (response2 != ConsoleKey.Enter)
						sendNewLine();
				} while (response2 != ConsoleKey.Y && response2 != ConsoleKey.N);

				answer = response2 == ConsoleKey.Y;
			}
			else {
				ConsoleKey response2;
				do {
					sendMessage("Do you want to continue to export? [y/n]", InfoMsg);
					response2 = Console.ReadKey(true).Key;
					if (response2 != ConsoleKey.Enter)
						sendNewLine();
				} while (response2 != ConsoleKey.Y && response2 != ConsoleKey.N);

				if (response2 == ConsoleKey.N)
					abort(false);
			}

			//get all workouts IDs and export them
			var workoutsToExport = db.getListOfExportableWorkouts(answer);
			foreach (var workoutId in workoutsToExport) {
				var details = db.getWorkoutDetails(workoutId);
				var xmlFactory = new XmlFactory();
				xmlFactory.createXmlFile(db.getSummaryInfoByTrackId(workoutId), details);
			}
			
			abort();
		}

		//exit program
		private static void abort(bool stop = true) {
			if (stop) {
				Console.WriteLine("\nPress enter to terminate job...");
				Console.ReadLine();
			}
			var temp = new DirectoryInfo(@".\Data\temp\");
			foreach (var file in temp.GetFiles()) {
				file.Delete();
			}
			Environment.Exit(0);
		}
	}
}