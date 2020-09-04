using System;
using System.Diagnostics;
using System.IO;
using ICSharpCode.SharpZipLib.Tar;

namespace Amazfit_data_exporter.Classes {
	public static class Convertor {
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
}