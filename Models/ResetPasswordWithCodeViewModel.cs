using System.ComponentModel.DataAnnotations;

namespace GuidanceTracker.Models
{
    public class ResetPasswordWithCodeViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }  // Make sure this exists only once

        [Required]
        public string ResetCode { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
