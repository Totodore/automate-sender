using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using AutomateSender;

namespace DatabaseHandler
{
	[DbConfigurationType(typeof(MySql.Data.Entity.MySqlEFConfiguration))]
	public class DatabaseContext : DbContext
	{
		public DatabaseContext() : base(Constants.connectionString)
		{
			Database.SetInitializer(new CreateDatabaseIfNotExists<DatabaseContext>());
		}

		public DbSet<FileEntity> Files { get; set; }
		public DbSet<GuildEntity> Guilds { get; set; }
		public DbSet<MessageEntity> Messages { get; set; }


		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			// configures one-to-many relationship
			modelBuilder.Entity<MessageEntity>()
				.HasMany(s => s.Files)
				.WithRequired(g => g.Message)
				.HasForeignKey(s => s.MessageId);
		}

		public static async Task<List<MessageEntity>> GetAllMessages(DateTime minDate, DateTime maxDate)
		{ 
			using var context = new DatabaseContext();
			return await context.Messages
			.Where(el => (el.Activated && el.Type == MessageType.FREQUENTIAL) || (el.Type == MessageType.PONCTUAL && el.Date > minDate && el.Date < maxDate))
			.ToListAsync() ?? new List<MessageEntity>();
		}

		public static bool CheckConnection()
		{
			Console.WriteLine("Connection String : " + Constants.connectionString);
			using var context = new DatabaseContext();
			return context.Database.Exists();
		}
	}
}