using System;
using System.IO;
using Serilog;

namespace AutomateSender
{
	public class FileHandler {
		private readonly string baseRoute;
		public FileHandler() {
			baseRoute = Path.Combine(Environment.CurrentDirectory, Environment.GetEnvironmentVariable("IMAGE_ROUTE") ?? "./data");
			Directory.CreateDirectory(baseRoute);
			Log.Information("Base File Route: " + baseRoute);
		}

		public FileStream GetFileStream(string id) {
			return File.OpenRead(Path.Combine(baseRoute, id));
		}

		public void DeleteFile(string id) {
			File.Delete(Path.Combine(baseRoute, id));
		}
	}
}