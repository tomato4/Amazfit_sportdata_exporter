﻿using System;
using System.IO;
using Amazfit_data_exporter.Classes;
using static Amazfit_data_exporter.Classes.Messenger;

namespace Amazfit_data_exporter {
	internal static class Program {
		private const string Version = "2.0";

		// ReSharper disable once InconsistentNaming
		public static void Main() {
			sendMessage("Amazfit watch nonGPS sport data exporter by Tomáš Glázr\nVersion: " + Version, SuccessMsg);
			sendNewLine();

			//check for updates
			var originVersion = "";
			try {
				originVersion = Tools.checkVersion();
			}
			catch (Exception e) {
				sendMessage("Unable to check updates for this app.", WarningMsg);
				sendMessage("Error: " + e.Message, LogMsg);
			}

			if (originVersion != "") {
				if (Version != originVersion)
					sendMessage(
						"New version available (" + originVersion + "). Consider downloading new version from github.",
						UpdateAvailable);
				else
					sendMessage("Your version is up to date.", LogMsg);
			}

			sendNewLine();

			sendMessage(
				"For support or suggestions write issue on Github repository of this project\nGithub: https://github.com/tomato4/Amazfit_sportdata_exporter/releases/");
			sendNewLine();
			sendMessage(
				"Connect your Amazfit into you PC. Make sure your drivers for your watch are installed properly. After confirmation program starts with getting data from your watch. You will need to agree with notification popped up on your watch. This process could take some while (up to minute).");
			sendNewLine();

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
			var lastExportFolder = new DirectoryInfo(Paths.LastExportFolder.cleanPath());
			foreach (var file in lastExportFolder.GetFiles()) {
				file.Delete();
			}

			//clear temp
			Directory.Delete(Paths.TempFolder.cleanPath(), true);
			Directory.CreateDirectory(Paths.TempFolder.cleanPath());

			//create timestamp for current export
			var timeStamp = DateTime.Now.ToString("yyyyMMddHHmmss");

			//extract data from Amazfit
			try {
				var extractor = new Extractor(Paths.AdbPath.cleanPath());
				extractor.extract(timeStamp);
			}
			catch (Exception e) {
				sendMessage("Error: " + e.Message, ErrorMsg);
				abort();
			}

			sendMessage("Data extraction from Amazfit completed.", SuccessMsg);
			sendNewLine();

			sendMessage("List of workouts in database:", InfoMsg);
			var db = new Database(Paths.databaseFilePath(timeStamp).cleanPath());
			var allWorkouts = db.getAllWorkouts();
			//show list of all workouts
			Database.writeWorkouts(allWorkouts);
			sendNewLine();

			//check if there is unknown workout, that wasn't exported and if so ask if wanna export
			var answer = false;
			if (db.isThereUnknownSport(allWorkouts)) {
				sendMessage("There is unknown workout, that wasn't exported. Do you want to export these workouts too?",
							InfoMsg);
				sendMessage("(note: this might potentially cause export fail)");
				sendNewLine();
				ConsoleKey response2;
				do {
					sendMessage("Do you want to export unknown file? [y/n]", InfoMsg);
					response2 = Console.ReadKey(true).Key;
					if (response2 != ConsoleKey.Enter)
						sendNewLine();
				} while (response2 != ConsoleKey.Y && response2 != ConsoleKey.N);

				answer = response2 == ConsoleKey.Y;
			}

			ConsoleKey response3;
			do {
				sendMessage("Do you want to continue to export? [y/n]", InfoMsg);
				response3 = Console.ReadKey(true).Key;
				if (response3 != ConsoleKey.Enter)
					sendNewLine();
			} while (response3 != ConsoleKey.Y && response3 != ConsoleKey.N);

			if (response3 == ConsoleKey.N) {
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

			Environment.Exit(0);
		}
	}
}