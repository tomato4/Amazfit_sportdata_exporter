using System;
using System.IO;

using Amazfit_data_exporter.Classes;
using static Amazfit_data_exporter.Classes.Messenger;
using static Amazfit_data_exporter.Classes.Tools;

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
			try {
				var extractor = new Extractor(Directory + "adb.exe");
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

		private static void abort(bool stop = true) {
			if (stop) {
				Console.WriteLine("\nPress enter to terminate job...");
				Console.ReadLine();
			}
			Environment.Exit(0);
		}
	}
}
