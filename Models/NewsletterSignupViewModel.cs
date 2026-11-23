using System.ComponentModel.DataAnnotations;

namespace CorporateHub.Models
{
	public class NewsletterSignupViewModel
	{
		[Required]
		[EmailAddress]
		public string Email { get; set; } = string.Empty;
	}
}
