using System.ComponentModel.DataAnnotations;

namespace Main.Models.ViewModels
{
    public class RegisterViewModel
    {
        public string Username { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; }

        public string Password { get; set; }
    }

}
