using System.ComponentModel.DataAnnotations;

namespace Main.Models.ViewModels
{
    public class EditUserProfileViewModel
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "Username is required.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string EmailAddress { get; set; }
    }
}
