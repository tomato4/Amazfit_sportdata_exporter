using System;

namespace Amazfit_data_exporter.Classes {
	public static class Messenger {
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
}