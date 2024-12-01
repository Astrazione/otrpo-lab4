namespace lab4.Services
{
	public enum LoggingLevel
	{
		None,
		ErrorsOnly,
		Warnings,
		Full
	}

	public class LoggerNotInitializedException : Exception
	{
		public LoggerNotInitializedException(string message) : base(message) { }
	}

	internal static class Logger
	{
		private static Mutex Mutex = new Mutex();

		public static LoggingLevel LoggingLevel { get; set; } = LoggingLevel.Full;

		private static async Task Log(string type, string message)
		{
			Mutex.WaitOne();
				Console.WriteLine($"{DateTime.Now.TimeOfDay} [{type}]:\t{message}");
			Mutex.ReleaseMutex();
		}

		//Info
		public static async Task I(string message)
		{
			if (LoggingLevel == LoggingLevel.Full)
				await Log("Info", message);
		}

		//Warning
		public static async Task W(string message)
		{
			if (LoggingLevel >= LoggingLevel.Warnings)
				await Log("Warning", message);
		}

		//Error
		public static async Task E(string message)
		{
			if (LoggingLevel >= LoggingLevel.ErrorsOnly)
				await Log("Error", message);
		}
	}
}
