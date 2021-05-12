using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DatabaseHandler
{
	[Table("file")]
	public class FileEntity : Entity
	{

		[Key]
		public string Id { get; set; }

		public string MessageId { get; set; }
		public MessageEntity Message { get; set; }
	}
}