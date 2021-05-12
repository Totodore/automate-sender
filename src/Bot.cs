using System;
using System.Threading.Tasks;
using System.Timers;
using Cronos;
using DatabaseHandler;

namespace AutomateSender
{
	public class Bot
	{
		public Timer timer;

		public DateTime minDate;
		public DateTime maxDate;

		/// <summary>
		/// - Wait for a new minute before starting (it computes the number of remaining seconds before the new minutes and the wait this time)
		/// - Start a new timer of 1 minute with the handler OnTimer
		/// - Even if the previous iteration of the timer takes more than 1 minutes the timer will execute the next one so there is no offset issues
		/// - Block the init method so program can wait infinitely
		/// </summary>
		public async Task Init()
		{
			Console.WriteLine("Awaiting new minute before starting...");
			await Task.Delay((int)(60_000 - TimeHelpers.CurrentTimeMillis() % 60_000));
			timer = new Timer(3000);
			timer.Elapsed += new ElapsedEventHandler(OnTimer);
			timer.Enabled = true;
			Console.WriteLine("New minute detected, thread started!");
			while (true) { await Task.Delay(60_000); }	//Blocking Bot
		}

		/// <summary>
		/// - Get the date with the minute above this one
		/// - Get the date with the current minute but without any seconds
		/// - Get all the messages to send (all freq and current ponctual)
		/// - foreach message it checks if it has to be sended and then add it to the thread pool 
		/// </summary>
		private async void OnTimer(object source, ElapsedEventArgs e)
		{
			maxDate = TimeHelpers.TimestampToDateTime(60_000 - TimeHelpers.CurrentTimeMillis() % 60_000 + TimeHelpers.CurrentTimeMillis());
			minDate = TimeHelpers.TimestampToDateTime(TimeHelpers.CurrentTimeMillis() - (TimeHelpers.CurrentTimeMillis() % 60_000));
			var data = await DatabaseContext.GetAllMessages(minDate, maxDate);
			foreach (var msg in data)
			{
				if (msg.Type == MessageType.FREQUENTIAL && CheckFreqMessage(msg))
				{
					//TODO: Handle thread pool
				} else if (msg.Type == MessageType.PONCTUAL) {
					//TODO: Handle thread pool
				}
			}
		}

		/// <summary>
		/// Checks with the lib cronos if the next cron occurrence is now
		/// It uses the cron expression in the message and the guild timezone
		/// In case of error it returs false 
		/// </summary>
		/// <param name="msg">The current freq message</param>
		/// <returns>If the freq message should be sended at this current method</returns>
		public bool CheckFreqMessage(MessageEntity msg) 
		{
			try
			{
				var cron = CronExpression.Parse(msg.Cron);
				var timezone = TimeZoneInfo.FindSystemTimeZoneById(msg.Guild.Timezone);
				var next = cron.GetNextOccurrence(DateTimeOffset.UtcNow, timezone);
				return next > minDate && next < maxDate;
			}
			catch (CronFormatException)
			{
				Console.WriteLine("Bad Cron format error, skipping message...");
				return false;
			}
			catch (Exception error)
			{
				Console.WriteLine("Cron parsing error (not known), error : " + error);
				return false;
			}
		}
	}
}