using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DatabaseHandler
{
	[Table("file")]
	public class FileEntity
	{

		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		public string MessageId { get; set; }
		public MessageEntity Message { get; set; }
	}
}