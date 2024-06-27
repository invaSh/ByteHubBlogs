using System.ComponentModel.DataAnnotations;

namespace Main.Models.ViewModels
{
    public class AddTagRequest
    {
        [Required(ErrorMessage ="Name is required")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Display name is required")]
        public string DisplayName { get; set; }
    }
}
