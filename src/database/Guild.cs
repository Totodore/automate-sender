using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DatabaseHandler
{
	[Table("guild")]
	public class GuildEntity {
		[Key]	
	  public string Id { get; set; }
	  public string Token { get; set; }
	  public int TokenExpires { get; set; }
	  public string RefreshToken { get; set; }
  	public string Timezone { get; set; }
	  public bool Scope { get; set; }
		public ICollection<MessageEntity> Messages { get; set; }
	}
}