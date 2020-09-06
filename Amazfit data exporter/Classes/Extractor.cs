using System;
using System.IO;
using SharpAdbClient;
using static Amazfit_data_exporter.Classes.Messenger;

namespace Amazfit_data_exporter.Classes {
	//extracts data (database of records) from watches
	public class Extractor {
		private readonly AdbServer _server = new AdbServer();
		private readonly AdbClient _adb = new AdbClient();
		private readonly MyAdbCommandLineClient _adbCmd;

		public Extractor(string adbPath) {
			//if couldn't resolve adb path throw error
			if (!File.Exists(adbPath))
				throw new Exception("adb.exe not found.");
			//start adb daemon
			_server.StartServer(adbPath, true);
			//command line client for adb
			_adbCmd = new MyAdbCommandLineClient(adbPath);
		}

		public void extract(string timeStamp) {
			//check connected devices
			var devices = _adb.GetDevices();
			if (devices.Count == 0)
				throw new Exception("ADB error: no device detected. Please connect device and start program again.");
			if (devices.Count > 1) {
				//show all connected devices
				sendMessage(
					"ADB warning: more than 1 device detected. This could potentially cause problems. If export will be unsuccessful, please disconnect all devices but Amazfit.",
					WarningMsg);
				sendNewLine();
				sendMessage("List of connected devices:", WarningMsg);
				var count = devices.Count;
				for (var i = 0; i < count; i++) {
					sendMessage(i + ": " + devices[i].Name + " " + devices[i].Model + " " + devices[i].Product,
								WarningMsg);
				}

				sendNewLine();
			}

			//get backup of apk, that holds database
			sendMessage("Sending request for backup on Amazfit...", LogMsg);
			_adbCmd.runAdbProcess(@"backup -noapk com.huami.watch.newsport -f " + "\"" + Paths.backupFilePath(timeStamp).cleanPath() + "\"", null,
								  null);
			//convert backup to .tar
			sendMessage("Converting backup to .tar", LogMsg);
			var conv = new Convertor(timeStamp);
			conv.backupToTar();
			//extract tar to folder
			sendMessage("Extracting tar file", LogMsg);
			conv.extractTar();
		}
	}
}