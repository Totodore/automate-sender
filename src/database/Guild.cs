using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DatabaseHandler
{
	[Table("guild")]
	public class GuildEntity : Entity
	{
		[Key]
		public string Id { get; set; }
		public string Timezone { get; set; }
		public bool Scope { get; set; }
		public ICollection<MessageEntity> Messages { get; set; }
	}
}