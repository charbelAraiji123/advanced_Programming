using System.ComponentModel.DataAnnotations;

namespace Final_Project_Adv.Models
{
    public class CreateUserViewModel
    {
        [Required(ErrorMessage = "Username is required")]
        [MinLength(3, ErrorMessage = "Username must be at least 3 characters")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Please select a role")]
        public string Role { get; set; }

        [Required(ErrorMessage = "Department is required")]
        public string DepartmentName { get; set; }
    }
}