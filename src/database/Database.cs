using System.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace AutomateSender.DatabaseHandler
{
	public class DatabaseContext : DbContext
	{
		public DbSet<FileEntity> Files { get; set; }
		public DbSet<GuildEntity> Guilds { get; set; }
		public DbSet<MessageEntity> Messages { get; set; }
		public DbSet<QuotaEntity> Quotas { get; set; }

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseMySql(Constants.connectionString, ServerVersion.AutoDetect(Constants.connectionString));
		}

		/// <summary>
		/// Get all the messages with linked guild, current quota and files
		/// filter if it is a date and that it is not in the datespan given in argument
		/// </summary>
		/// <returns>The list of the queried messages</returns>
		public static List<MessageEntity> GetAllMessages()
		{
			using var context = new DatabaseContext();
			return context.Messages.AsQueryable()
			.Where(el => el.Activated && el.Guild.Timezone != null && el.Guild.DeletedDate == null)
			.Include(el => el.Guild)
			.Include(el => el.Guild.Quotas.Where(quota => quota.Date == TimeHelpers.CurrentMonth))
			.Include(el => el.Files)
			.ToList() ?? new List<MessageEntity>();
		}

		/// <summary>
		/// Increment or create all the quota per month for the guilds
		/// </summary>
		/// <param name="sentMessages">A dictionnary container the number of sent messages per guilds</param>
		/// <returns>A async task</returns>
		public static async Task IncrementQuota(Dictionary<GuildEntity, int> sentMessages) {
			using var context = new DatabaseContext();
			Dictionary<int, int> quotaIds = new();
			List<QuotaEntity> newQuotas = new();
			foreach (var guild in sentMessages.Keys)
			{
				if (guild.CurrentQuota?.Id != null)
					quotaIds.Add(guild.CurrentQuota.Id, sentMessages[guild]);
				else
					newQuotas.Add(new QuotaEntity { GuildId = guild.Id, MonthlyQuota = sentMessages[guild] });
			}
			if (newQuotas.Count > 0) {
				await context.BulkInsertAsync(newQuotas);
			}
			var quotas = await context.Quotas.AsQueryable().Where(el => quotaIds.Keys.Contains(el.Id)).ToListAsync();
			foreach (var quota in quotas) {
				quota.MonthlyQuota += quotaIds.First(el => el.Key == quota.Id).Value;
			}
			await context.BulkUpdateAsync(quotas);
		}

		public static async Task DisabledOneTimeMessage(List<MessageEntity> messages, FileHandler fileHandler) {
			using var context = new DatabaseContext();
			List<string> messagesIds = messages.ConvertAll(el => el.Id);
			await context.Messages.AsQueryable()
			.Where(el => messagesIds.Contains(el.Id) && !el.Guild.RemoveOneTimeMessage && el.Guild.DeletedDate == null)
			.UpdateFromQueryAsync(_ => new MessageEntity { Activated = false });
			var msgToDelete = messages.Where(el => el.Guild.RemoveOneTimeMessage && el.Guild.DeletedDate == null);
			var filesToDelete = msgToDelete.SelectMany(el => el.Files ?? new List<FileEntity>()) ?? new List<FileEntity>();
			await context.BulkDeleteAsync(filesToDelete);
			await context.BulkDeleteAsync(msgToDelete);
			foreach (FileEntity file in filesToDelete) {
				try {
					fileHandler.DeleteFile(file.Id);
				} catch(Exception err) {
					Log.Error($"Could not delete file {file.Id} err: {err}");
				}
			}
		}

		/// <summary>
		/// Check the mysql connection
		/// </summary>
		/// <returns>Returns true if the connection is good, false otherwise</returns>
		public static async Task<bool> CheckConnection()
		{
			Log.Information("Connection String : " + Constants.connectionString);
			try {
				using var context = new DatabaseContext();
				await context.Database.EnsureCreatedAsync();
				return true;
			} catch(Exception e) {
				Log.Fatal(e.ToString());
				return false;
			}
		}
	}
}