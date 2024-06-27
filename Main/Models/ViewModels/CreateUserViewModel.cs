using System.ComponentModel.DataAnnotations;

namespace Main.Models.ViewModels
{
    public class CreateUserViewModel
    {
        [Required]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public bool IsAdmin { get; set; }

        public bool IsHeadAdmin { get; set; }
        public bool IsUser{ get; set; }
    }
}
