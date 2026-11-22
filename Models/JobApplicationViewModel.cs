using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace CorporateHub.Models
{
	public class JobApplicationViewModel
	{
		[Required]
		public string Name { get; set; } = string.Empty;

		[Required]
		[EmailAddress]
		public string Email { get; set; } = string.Empty;

		public string CoverLetter { get; set; } = string.Empty;

		[Required(ErrorMessage = "Please attach your resume.")]
		public IFormFile? Resume { get; set; }
	}
}
