using System.ComponentModel.DataAnnotations;

namespace CorporateHub.Models
{
	public class EditProfileViewModel
	{
		[Required]
		public string FullName { get; set; } = string.Empty;

		public string Address { get; set; } = string.Empty;
	}
}
