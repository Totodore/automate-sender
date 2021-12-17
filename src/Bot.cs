using System;
using System.Threading.Tasks;
using System.Timers;
using Cronos;
using Discord;
using Discord.WebSocket;
using Discord.Webhook;
using System.Linq;
using AutomateSender.DatabaseHandler;
using System.Runtime.InteropServices;
using TimeZoneConverter;
using Serilog;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace AutomateSender
{
	public class Bot
	{
		private System.Timers.Timer timer;

		private DateTime minDate;
		private DateTime maxDate;

		private List<MessageEntity> cronErroredMessages = new();

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
			Log.Information("Connecting to AutomateBot...");
			await client.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("TOKEN_BOT"));
			await client.StartAsync();
			Log.Information("Discord client successfully connected to AutomateBot!");
			Log.Information("Awaiting new minute before starting...");
			await Task.Delay((int)(60_000 - (TimeHelpers.CurrentTimeMillis() % 60_000) + 1000));
			timer = new Timer(60_000);
			timer.Elapsed += OnTimer;
			timer.Enabled = true;
			Log.Information("New minute detected, thread started!");
			OnTimer(null, null);
			await Task.Delay(-1);
		}

		/// <summary>
		/// - Get the date with the minute above this one
		/// - Get the date with the current minute but without any seconds
		/// - Get all the messages to send (all freq and current ponctual)
		/// - foreach message it checks if it has to be sended and then add it to the thread pool
		/// - Once the messages sent we build a map with the messages sent through channels for each guild
		/// </summary>
		private async void OnTimer(object source, ElapsedEventArgs e)
		{
			maxDate = TimeHelpers.TimestampToDateTime(60_000 - (TimeHelpers.CurrentTimeMillis() % 60_000) + TimeHelpers.CurrentTimeMillis());
			minDate = TimeHelpers.TimestampToDateTime(TimeHelpers.CurrentTimeMillis() - (TimeHelpers.CurrentTimeMillis() % 60_000));
			var data = DatabaseContext.GetAllMessages();
			var actions = new List<Func<Task<MessageEntity>>>();
			int messagesTobeSent = 0;
			int messagesSkipped = 0;
			foreach (var msg in data)
			{
				if (msg.Type == DatabaseHandler.MessageType.FREQUENTIAL ? CheckFreqMessage(msg) : CheckPonctualMessage(msg))
				{
					if ((msg.Guild.CurrentQuota?.MonthlyQuota ?? 0) < msg.Guild.MonthlyQuota || msg.Webhook != null) {
						actions.Add(() => SendMessage(msg));
						messagesTobeSent++;
					} else {
						Log.Verbose("Skiping this guild (daily quota exceeded)");
						messagesSkipped++;
					}
				}
			}
			var successfulMessages = ThreadHelpers.SpawnAndWait(actions, 60_000).Where(el => el != null).ToList();
			var guildMsg = new Dictionary<GuildEntity, int>();
			foreach(MessageEntity msg in successfulMessages) {
				if (guildMsg.ContainsKey(msg.Guild) && msg.Webhook == null)
					guildMsg[msg.Guild]++;
				else if (msg.Webhook == null)
					guildMsg.Add(msg.Guild, 1);
			}
			await DatabaseContext.IncrementQuota(guildMsg);
			await DatabaseContext.DisabledOneTimeMessage(successfulMessages.Where(el => el.TypeEnum == 0).ToList(), fileHandler);
			await DatabaseContext.DisableErroredMessages(cronErroredMessages);
			Log.Information($"[{minDate}] Threadpool ended with {successfulMessages.Count}/{messagesTobeSent} ({messagesSkipped} messages out of quota) messages sent");
			cronErroredMessages = new List<MessageEntity>();
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
				var tz = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? TZConvert.IanaToWindows(msg.Guild.Timezone) : msg.Guild.Timezone;
				var timezone = TimeZoneInfo.FindSystemTimeZoneById(tz);
				var next = cron.GetNextOccurrence(minDate, timezone, true);
				Log.Verbose($"next: {next}, min: {minDate}, max: {maxDate}");
				return next >= minDate && next < maxDate;
			}
			catch (CronFormatException)
			{
				Log.Verbose("Bad Cron format error, skipping and disabling message...");
				cronErroredMessages.Add(msg);
				return false;
			}
			catch (Exception error)
			{
				Log.Error("Cron parsing error (not known), error : " + error);
				return false;
			}
		}

		public bool CheckPonctualMessage(MessageEntity msg) {
			var tz = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? TZConvert.IanaToWindows(msg.Guild.Timezone) : msg.Guild.Timezone;
			try {
				var timezone = TimeZoneInfo.FindSystemTimeZoneById(tz);
				var utcDate = TimeZoneInfo.ConvertTimeToUtc((DateTime)msg.Date, timezone);
				Log.Verbose($"curr: {utcDate}, min: {minDate}, max: {maxDate}");
				return utcDate >= minDate && utcDate < maxDate;
			} catch (Exception error) {
				Log.Error("Date parsing error :" + error);
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
		public async Task<MessageEntity> SendMessage(MessageEntity msg)
		{
			try
			{
				using var webhook = msg.Webhook != null ? new DiscordWebhookClient(msg.Webhook.Url) : null;
				var channel = client.GetChannel(ulong.Parse(msg.ChannelId)) as IMessageChannel;
				var files = msg.Files.ToList();
				for (int i = 0; i < (msg.Files?.Count ?? 0); i++)
				{
					try
					{
						if (webhook != null)
							await webhook.SendFileAsync(fileHandler.GetFileStream(files[i].Id), $"attachment-{i}", "");
						else
							await channel.SendFileAsync(fileHandler.GetFileStream(files[i].Id), $"attachment-{i}");
					}
					catch (Exception e)
					{
						Log.Warning("Crash during file sending for message: " + msg);
						Log.Error("Error: " + e);
					}
				}
				if (channel == null && webhook == null) {
					throw new Exception("No Channel or webhook found");
				}
				if (webhook != null)
					await webhook.SendMessageAsync(msg.ParsedMessage);
				else
					await channel?.SendMessageAsync(msg.ParsedMessage);
				return msg;
			}
			catch (Exception e)
			{
				Log.Warning("Crash during msg sending: " + msg);
				Log.Error("Error: " + e);
				cronErroredMessages.Add(msg);
				return null;
			}
		}
	}
}
