using System.ComponentModel.DataAnnotations;

namespace TripEnjoy.Client.ViewModels
{
    public class VerifyOtpRequestVM
    {
        [Required]
        public string Email { get; set; }

        [Required]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "OTP must be 6 digits.")]
        public string Otp { get; set; }

        public bool RememberMe { get; set; }
    }
}
