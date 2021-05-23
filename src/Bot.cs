using System;
using System.Threading.Tasks;
using System.Timers;
using Cronos;
using Discord;
using Discord.WebSocket;
using System.Linq;
using System.Threading;
using AutomateSender.DatabaseHandler;

namespace AutomateSender
{
	public class Bot
	{
		private System.Timers.Timer timer;

		private DateTime minDate;
		private DateTime maxDate;

		private readonly FileHandler fileHandler = new();

		private readonly DiscordSocketClient client = new();

		/// <summary>
		/// - Wait for a new minute before starting (it computes the number of remaining seconds before the new minutes and the wait this time)
		/// - Start a new timer of 1 minute with the handler OnTimer
		/// - Even if the previous iteration of the timer takes more than 1 minutes the timer will execute the next one so there is no offset issues
		/// - Block the init method so program can wait infinitely
		/// </summary>
		public async Task Init()
		{
			Console.WriteLine("Connecting to AutomateBot...");
			await client.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("TOKEN_BOT"));
			await client.StartAsync();
			Console.WriteLine("Discord client successfully connected to AutomateBot!");
			Console.WriteLine("Awaiting new minute before starting...");
			await Task.Delay((int)(60_000 - (TimeHelpers.CurrentTimeMillis() % 60_000)));
			timer = new System.Timers.Timer(60_000);
			timer.Elapsed += OnTimer;
			timer.Enabled = true;
			Console.WriteLine("New minute detected, thread started!");
			await Task.Delay(-1);
		}

		/// <summary>
		/// - Get the date with the minute above this one
		/// - Get the date with the current minute but without any seconds
		/// - Get all the messages to send (all freq and current ponctual)
		/// - foreach message it checks if it has to be sended and then add it to the thread pool
		/// </summary>
		private async void OnTimer(object source, ElapsedEventArgs e)
		{
			maxDate = TimeHelpers.TimestampToDateTime(60_000 - (TimeHelpers.CurrentTimeMillis() % 60_000) + TimeHelpers.CurrentTimeMillis());
			minDate = TimeHelpers.TimestampToDateTime(TimeHelpers.CurrentTimeMillis() - (TimeHelpers.CurrentTimeMillis() % 60_000));
			var data = await DatabaseContext.GetAllMessages(minDate, maxDate);
			var i = 0;
			foreach (var msg in data)
			{
				if (msg.Type == DatabaseHandler.MessageType.FREQUENTIAL && CheckFreqMessage(msg))
				{
					ThreadPool.QueueUserWorkItem((object _) => SendMessage(msg));
					i++;
				}
				else if (msg.Type == DatabaseHandler.MessageType.PONCTUAL)
				{
					ThreadPool.QueueUserWorkItem((object _) => SendMessage(msg));
					i++;
				}
			}
			if (ThreadHelpers.WaitForThreads(120))
				Console.WriteLine("Threadpool timed out!");
			else
				Console.WriteLine("Threadpool ended with " + i + " messages sent");
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
				return next > minDate && next <= maxDate;
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

		/// <summary>
		/// Send a message to a specific channel
		/// - Get the text channel
		/// - Foreach files we read them and we send them asynchronously
		/// - - If there is an error while sending a file we send the next one
		/// - We then send the message
		/// - In case of error the message is completely logged
		/// </summary>
		/// <param name="msg">The message object to send</param>
		public async void SendMessage(MessageEntity msg)
		{
			try
			{
				var channel = client.GetChannel(ulong.Parse(msg.ChannelId)) as IMessageChannel;
				var files = msg.Files.ToList();
				for (int i = 0; i < (msg.Files?.Count ?? 0); i++)
				{
					try
					{
						await channel.SendFileAsync(fileHandler.GetFileStream(files[i].Id), $"attachment-{i}");
					}
					catch (Exception e)
					{
						Console.WriteLine("Crash during file sending for message: " + msg);
						Console.WriteLine("Error: " + e);
						continue;
					}
				}
				if (channel == null) {
					throw new Exception("No Channel found");
				}
				await channel?.SendMessageAsync(msg.ParsedMessage);
			}
			catch (Exception e)
			{
				Console.WriteLine("Crash during msg sending: " + msg);
				Console.WriteLine("Error: " + e);
				return;
			}
		}
	}
}