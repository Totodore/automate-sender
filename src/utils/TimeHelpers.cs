using System;

namespace AutomateSender
{
	public static class TimeHelpers
	{
		public static long CurrentTimeMillis()
		{
			return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
		}
		/// <summary>
		/// Convert a timestamp to a UTC DateTime
		/// </summary>
		/// <param name="unixTimeStamp">The timestamp to convert</param>
		/// <returns>The DateTime corresponding to the timestamp given</returns>
		public static DateTime TimestampToDateTime(double unixTimeStamp)
		{
			// Unix timestamp is seconds past epoch
			DateTime dtDateTime = new(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			return dtDateTime.AddMilliseconds(unixTimeStamp);
		}

		/// <summary>Get the current month</summary>
		/// <returns>The first day of the month in DateTime UTC</returns>
		public static DateTime CurrentMonth { get { return new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1); } }
	}
}