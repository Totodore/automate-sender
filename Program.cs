using System;
using System.Threading.Tasks;
using AutomateSender.DatabaseHandler;
using dotenv.net;

namespace AutomateSender
{
	public static class Program
	{
		private static async Task Main()
		{
			DotEnv.Load();
			ConnectToDatabase();
			await new Bot().Init();
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
					Console.WriteLine("Successfully connected to database!");
			}
			catch (Exception e)
			{
				Console.WriteLine("Could not connect to the database with connectionString '" + Constants.connectionString + "', quitting...");
				Console.WriteLine(e);
				Environment.Exit(1);
			}
		}
	}
}
