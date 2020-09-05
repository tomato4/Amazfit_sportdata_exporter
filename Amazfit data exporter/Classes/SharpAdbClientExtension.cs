using System.Collections.Generic;
using Microsoft.Extensions.Logging;

// ReSharper disable once CheckNamespace
namespace SharpAdbClient {
	public class MyAdbCommandLineClient : AdbCommandLineClient {
		public MyAdbCommandLineClient(string adbPath, ILogger<AdbCommandLineClient> logger = null) : base(
			adbPath, logger) { }

		public void runAdbProcess(string command, List<string> errorOutput, List<string> standartOutput) {
			RunAdbProcess(command, errorOutput, standartOutput);
		}
	}
}