using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace AutomateSender.DatabaseHandler
{
	[DbConfigurationType(typeof(MySql.Data.Entity.MySqlEFConfiguration))]
	public class DatabaseContext : DbContext
	{
		public DatabaseContext() : base(Constants.connectionString)
		{
    	// Database.Log = Console.Write;
			Database.SetInitializer(new CreateDatabaseIfNotExists<DatabaseContext>());
		}

		public DbSet<FileEntity> Files { get; set; }
		public DbSet<GuildEntity> Guilds { get; set; }
		public DbSet<MessageEntity> Messages { get; set; }

		public static async Task<List<MessageEntity>> GetAllMessages(DateTime minDate, DateTime maxDate)
		{
			using var context = new DatabaseContext();
			return await context.Messages
			.Where(el => el.Activated && (el.TypeEnum == 1 || (el.TypeEnum == 0 && el.Date > minDate && el.Date < maxDate)))
			.Include(el => el.Guild)
			.Include(el => el.Files)
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