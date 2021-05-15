using System;

namespace AutomateSender
{
	public static class TimeHelpers
	{
		public static long CurrentTimeMillis()
		{
			return DateTimeOffset.Now.ToUnixTimeMilliseconds();
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
			return dtDateTime.AddMilliseconds(unixTimeStamp).ToLocalTime();
		}
	}
}