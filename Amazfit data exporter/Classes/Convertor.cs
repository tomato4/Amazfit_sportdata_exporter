using System;
using System.Diagnostics;
using System.IO;
using ICSharpCode.SharpZipLib.Tar;

namespace Amazfit_data_exporter.Classes {
	public class Convertor {
		private string _timeStamp;

		public Convertor(string timeStamp) {
			_timeStamp = timeStamp;
		}

		public void backupToTar() {
			var psi = new ProcessStartInfo("java",
										   @"-jar abe.jar unpack " + Paths.backupFilePath(_timeStamp).cleanPath(WrapStyle.DoubleQuotes) + " " +
										   Paths.tarFilePath(_timeStamp).cleanPath(WrapStyle.DoubleQuotes)) {
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

			if (process.ExitCode != 0)
				throw new Exception("error occurred when converting backup to .tar. Try again...");
		}

		public void extractTar() {
			extractTar(Paths.tarFilePath(_timeStamp).cleanPath(), Paths.extractedTarFolder(_timeStamp).cleanPath());

			//check existence of DB file
			var dbFilePath = Paths.databaseFilePath(_timeStamp).cleanPath();
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
}