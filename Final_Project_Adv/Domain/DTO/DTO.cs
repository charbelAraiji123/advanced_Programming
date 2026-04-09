using TaskStatusEnum = Final_Project_Adv.Domain.Enums.TaskStatus;

namespace Final_Project_Adv.Domain.DTO;

public record UsersDto
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }

    public int DepartmentId { get; set; }
}
public class CreateUserDto
{
    public string Username { get; set; }
    public string Password { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }

    public int DepartmentId { get; set; } // 🔥 important
}

public record DepartmentDto
{
    public int Id { get; set; }
    public string Name { get; set; }

    public List<UsersDto> Users { get; set; } = new();

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
public class CreateDepartmentDto
{
    public string Name { get; set; }
}
public record TaskItemDto(
    int Id,
    string Title,
    string Description,
    TaskStatusEnum Status,
    int CreatedById,
    UsersDto CreatedBy,
    int DepartmentId,
    DepartmentDto Department,
    List<TaskAssignmentDto> TaskAssignments,
    List<SubtaskDto> Subtasks,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record TaskAssignmentDto(
    int TaskItemId,
    TaskItemDto TaskItem,
    int UserId,
    UsersDto User,
    DateTime AssignedAt
);

public record SubtaskDto(
    int Id,
    string Title,
    string Description,
    TaskStatusEnum Status,
    int TaskItemId,
    TaskItemDto TaskItem,
    int? AssignedToId,
    UsersDto? AssignedTo,
    int CreatedById,
    UsersDto CreatedBy,
    DateTime CreatedAt,
    DateTime UpdatedAt
);