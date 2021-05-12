using System;

namespace AutomateSender
{
	public static class TimeHelpers
	{
		private static readonly DateTime Jan1st1970 = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		/// <returns>Returns the current timestamp</returns>
		public static long CurrentTimeMillis()
		{
			return (long)(DateTime.UtcNow - Jan1st1970).TotalMilliseconds;
		}
		/// <summary>
		/// Convert a timestamp to a DateTime
		/// </summary>
		/// <param name="unixTimeStamp">The timestamp to convert</param>
		/// <returns>The DateTime corresponding to the timestamp given</returns>
		public static DateTime TimestampToDateTime(double unixTimeStamp)
		{
			// Unix timestamp is seconds past epoch
			DateTime dtDateTime = new(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			dtDateTime = dtDateTime.AddMilliseconds(unixTimeStamp).ToLocalTime();
			return dtDateTime;
		}
	}
}