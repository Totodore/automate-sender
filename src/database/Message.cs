using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;

namespace AutomateSender.DatabaseHandler
{
	[Table("message")]
	public class MessageEntity : Entity
	{
		[Key]
		public string Id { get; set; }
		public string ChannelId { get; set; }
		public string Cron { get; set; }
		public DateTime? Date { get; set; }
		public string ParsedMessage { get; set; }
		public string Message { get; set; }
		public string Description { get; set; }
		[NotMapped]
		public MessageType Type
		{
			get
			{
				return Enum.GetValues<MessageType>()[TypeEnum];
			}
			set
			{
				TypeEnum = Enum.GetValues<MessageType>().ToList().IndexOf(value);
			}
		}

		public int TypeEnum { get; set; }
		public string GuildId { get; set; }
		public GuildEntity Guild { get; set; }
		public string WebhookId { get; set; }
		public WebhookEntity Webhook { get; set; }
		public string CreatorId { get; set; }
		public ICollection<FileEntity> Files { get; set; }
		public bool Activated { get; set; }
		public DateTime UpdatedDate { get; set; }
	}
	public enum MessageType
	{
		PONCTUAL,
		FREQUENTIAL
	}
}