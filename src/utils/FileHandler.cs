using System;
using System.IO;

namespace AutomateSender
{
	public class FileHandler {
		private readonly string baseRoute;
		public FileHandler() {
			baseRoute = Path.Combine(Environment.CurrentDirectory, Environment.GetEnvironmentVariable("IMAGE_ROUTE") ?? "./data");
			Directory.CreateDirectory(baseRoute);
			Console.WriteLine("Base File Route: " + baseRoute);
		}

		public FileStream GetFileStream(string id) {
			return File.OpenRead(Path.Combine(baseRoute, id));
		} 
	}
}