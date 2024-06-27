using Main.Models.Domain;
using System.ComponentModel.DataAnnotations;

namespace Main.Models.ViewModels
{
    public class UserDetailsViewModel
    {
        public Guid Id { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        public string EmailAddress { get; set; }

        public string Role {  get; set; }
        public bool IsAdmin { get; set; }
        public bool IsHeadAdmin { get; set; }
        public bool IsUser { get; set; }

    }
}
