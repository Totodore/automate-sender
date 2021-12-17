
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutomateSender.DatabaseHandler
{
	[Table("webhook")]
	public class WebhookEntity : Entity
	{
		[Key]
		public string Id { get; set; }
		public GuildEntity Guild { get; set; }
		public string GuildId { get; set; }
		public string ChannelId { get; set; }
		public string Url { get; set; }
	}
}