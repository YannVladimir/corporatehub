using System.ComponentModel.DataAnnotations;

namespace CorporateHub.Models
{
	public class ContactMessageViewModel
	{
		[Required]
		public string Name { get; set; } = string.Empty;

		[Required]
		[EmailAddress]
		public string Email { get; set; } = string.Empty;

		[Required]
		public string Message { get; set; } = string.Empty;
	}
}
