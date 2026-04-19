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
public record UpdateUserDto(
    string Username,
    string Password,
    string Email,
    string Role,
    int DepartmentId
);

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
    int DepartmentId,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
public class CreateTaskItemDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int CreatedById { get; set; }
    public int DepartmentId { get; set; }
}

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
    int? AssignedToId,
    int CreatedById,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
public record CreateSubtaskDto(
    string Title,
    string Description,
    int TaskItemId,
    int CreatedById,
    int? AssignedToId  // optional, can be assigned later
);
public record UpdateTaskDTO(

    string Title,
    string Description,
    string Status,
    int DepartmentId,
    DateTime UpdatedAt

    );
public record UpdateSubTaskDTO(

    string Title,
    string Description,
    string Status,
    int AssignedId,
    DateTime UpdatedAt

    );
public class UserTaskStatusDto
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;

    public List<TaskStatusDto> Tasks { get; set; } = new();
}

public class TaskStatusDto
{
    public int TaskId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;

    public List<SubtaskStatusDto> Subtasks { get; set; } = new();
}

public class SubtaskStatusDto
{
    public int SubtaskId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
public class TaskCommentDto
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;

    public int AuthorId { get; set; }
    public string AuthorName { get; set; } = string.Empty;

    public int TaskItemId { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
public class CreateTaskCommentDto
{
    public string Content { get; set; } = string.Empty;

    public int AuthorId { get; set; }
    public int TaskItemId { get; set; }
}
public class SubtaskCommentDto
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;

    public int AuthorId { get; set; }
    public string AuthorName { get; set; } = string.Empty;

    public int SubtaskId { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
public class CreateSubtaskCommentDto
{
    public string Content { get; set; } = string.Empty;

    public int AuthorId { get; set; }
    public int SubtaskId { get; set; }
}
public record TaskWithSubtasksDto(
    int Id,
    string Title,
    string Description,
    TaskStatusEnum Status,
    int CreatedById,
    int DepartmentId,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<SubtaskDto> Subtasks
);