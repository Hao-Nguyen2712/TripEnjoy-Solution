using System.ComponentModel.DataAnnotations;

namespace TripEnjoy.Client.ViewModels
{
    public class LoginRequestVM
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; }

    }
}
