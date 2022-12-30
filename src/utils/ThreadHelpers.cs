using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AutomateSender
{
	public static class ThreadHelpers
	{
		/// <summary>
		/// Create a threadpool from a list of functions and with an optional timeout
		/// </summary>
		/// <param name="actions">The list of the function that returns a T arg</param>
		/// <param name="timeout">The timeout of the threadpool</param>
		/// <typeparam name="T">The return type of the lambda list</typeparam>
		/// <returns>The list of the lambda response</returns>
		public static List<T> SpawnAndWait<T>(List<Func<Task<T>>> actions, int timeout = -1)
		{
			var handles = new EventWaitHandle[actions.Count];
			var els = new List<T>();
			for (var i = 0; i < actions.Count; i++)
			{
				handles[i] = new ManualResetEvent(false);
				var currentAction = actions[i];
				var currentHandle = handles[i];
				ThreadPool.QueueUserWorkItem(async _ =>
				{
					try
					{
						els.Add(await currentAction());
					}
					finally
					{
						currentHandle.Set();
					}
				});
			}
			if (handles.Length > 0)
				WaitHandle.WaitAll(handles, timeout);
			return els;
		}
	}
}