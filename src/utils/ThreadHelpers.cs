using System;

namespace AutomateSender
{
	public static class ThreadHelpers
	{
		/// <summary>
		/// Wait for threads in threadpool
		/// </summary>
		/// <param name="timeOutSeconds">The timeout in seconds for the waiting</param>
		/// <returns>True if it timed out, false if everything went ok</returns>
		public static bool WaitForThreads(int timeOutSeconds)
		{
			//Now wait until all threads from the Threadpool have returned
			while (timeOutSeconds > 0)
			{
				//figure out what the max worker thread count it
				System.Threading.ThreadPool.GetMaxThreads(out int maxThreads, out _);
				System.Threading.ThreadPool.GetAvailableThreads(out int availThreads, out _);

				if (availThreads == maxThreads) return false;
				// Sleep
				System.Threading.Thread.Sleep(TimeSpan.FromMilliseconds(1000));
				--timeOutSeconds;
			}
			return true;
		}
	}
}