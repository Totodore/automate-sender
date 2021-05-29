
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutomateSender.DatabaseHandler
{
	[Table("quota")]
	public class QuotaEntity : Entity
	{
		[Key]
		public int Id { get; set; }
		public DateTime Date { get; set; } = TimeHelpers.CurrentMonth;
		public int MonthlyQuota { get; set; }
		public string GuildId { get; set; }
		public GuildEntity Guild { get; set; }
	}
}