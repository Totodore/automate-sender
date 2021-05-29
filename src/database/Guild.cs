using System.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace AutomateSender.DatabaseHandler
{
	[Table("guild")]
	public class GuildEntity : Entity
	{
		[Key]
		public string Id { get; set; }
		public string Timezone { get; set; }
		public bool Scope { get; set; }
		public int DailyQuota { get; set; }
		public ICollection<MessageEntity> Messages { get; set; }
		public ICollection<QuotaEntity> Quotas { get; set; }

		[NotMapped]
		public QuotaEntity CurrentQuota
		{
			get
			{
				return Quotas.SingleOrDefault(el => el.Date == TimeHelpers.CurrentMonth);
			}
		}
	}
}