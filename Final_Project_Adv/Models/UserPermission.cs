using Final_Project_Adv.Domain.Enums;

namespace Final_Project_Adv.Models;

public class UserPermission
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public Users User { get; set; } = null!;

    public PermissionType Permission { get; set; }

    public int GrantedById { get; set; }
    public Users GrantedBy { get; set; } = null!;

    public DateTime GrantedAt { get; set; }
}