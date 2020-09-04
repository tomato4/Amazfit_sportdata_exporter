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
			if (!File.Exists(adbPath))
				throw new Exception("adb.exe not found.");
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
			if(Convertor.backupToTar(timeStamp) != 0)
				throw new Exception("error occurred when converting backup to .tar. Try again...");
			sendMessage("Extracting tar file", LogMsg);
			Convertor.extractTar(timeStamp);
		}
	}
}