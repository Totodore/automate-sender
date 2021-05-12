using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DatabaseHandler
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
		public MessageType Type { get; set; }

		public string GuildId { get; set; }
		public GuildEntity Guild { get; set; }
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