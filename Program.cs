using System;
using System.Threading.Tasks;
using AutomateSender.DatabaseHandler;
using dotenv.net;
using Serilog;

namespace AutomateSender
{
	public static class Program
	{
		private static async Task Main()
		{
			DotEnv.Load();
			var loggerConf =  new LoggerConfiguration();
			if (Environment.GetEnvironmentVariable("LOG_LEVEL") == "Verbose")
				loggerConf.MinimumLevel.Verbose();
			Log.Logger = loggerConf.WriteTo.Console().CreateLogger();
			ConnectToDatabase();
			await new Bot().Init();
			Log.CloseAndFlush();
		}

		/// <summary>
		/// Connect to the Mysql Database
		/// - Build the database connection from the environment variables
		/// - Check the connection
		/// </summary>
		private static void ConnectToDatabase()
		{
			try
			{
				var env = Environment.GetEnvironmentVariables();
				Constants.connectionString = $"Server={env["DB_HOST"]};Port={env["DB_PORT"]};Database={env["DB_NAME"]};Uid={env["DB_USER"]};Pwd={env["DB_PASS"]};";
				if (!DatabaseContext.CheckConnection())
					throw new Exception();
				else
					Log.Information("Successfully connected to database!");
			}
			catch (Exception e)
			{
				Log.Fatal("Could not connect to the database with connectionString '" + Constants.connectionString + "', quitting...");
				Log.Fatal(e.ToString());
				Environment.Exit(1);
			}
		}
	}
}
