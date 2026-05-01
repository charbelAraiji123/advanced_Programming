using Final_Project_Adv.Domain.Enums;

namespace Final_Project_Adv.Models
{
    public class UserPermissionsViewModel
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public List<PermissionType> Permissions { get; set; } = new();
    }

    public class SetAllPermissionsDto
    {
        public int UserId { get; set; }
        public List<PermissionType> Permissions { get; set; } = new();
    }
}