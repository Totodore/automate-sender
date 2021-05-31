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
		/// <param name="guilds">The list of the guilds in which to increment or create the quota</param>
		/// <returns>A async task</returns>
		public static async Task IncrementQuota(List<GuildEntity> guilds) {
			using var context = new DatabaseContext();
			List<int> quotaIds = new();
			List<QuotaEntity> newQuotas = new();
			foreach (var guild in guilds)
			{
				if (guild.CurrentQuota?.Id != null)
					quotaIds.Add(guild.CurrentQuota.Id);
				else
					newQuotas.Add(new QuotaEntity { GuildId = guild.Id, MonthlyQuota = 1 });
			}
			if (newQuotas.Count > 0) {
				await context.BulkInsertAsync(newQuotas);
			}
			await context.Quotas.AsQueryable()
			.Where(el => quotaIds.Contains(el.Id))
			.UpdateFromQueryAsync(el => new QuotaEntity { MonthlyQuota = el.MonthlyQuota + 1 });
		}

		public static async Task DisabledOneTimeMessage(List<MessageEntity> messages) {
			using var context = new DatabaseContext();
			List<string> messagesIds = messages.ConvertAll(el => el.Id);
			await context.Messages.AsQueryable()
			.Where(el => el.TypeEnum == 0 && messagesIds.Contains(el.Id) && !el.Guild.RemoveOneTimeMessage && el.Guild.DeletedDate == null)
			.UpdateFromQueryAsync(_ => new MessageEntity { Activated = false });
			await context.BulkDeleteAsync(messages.Where(el => el.Guild.RemoveOneTimeMessage));
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