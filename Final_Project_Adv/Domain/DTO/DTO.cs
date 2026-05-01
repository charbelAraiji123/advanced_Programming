using Final_Project_Adv.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using TaskStatusEnum = Final_Project_Adv.Domain.Enums.TaskStatus;

namespace Final_Project_Adv.Domain.DTO;

// ═══════════════════════════════════════════════════════════════
//  USER
// ═══════════════════════════════════════════════════════════════

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
    [Required(ErrorMessage = "Username is required")]
    [MinLength(3, ErrorMessage = "Username must be at least 3 characters")]
    public string Username { get; set; }

    [Required(ErrorMessage = "Password is required")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
    public string Password { get; set; }

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Please select a role")]
    public string Role { get; set; }

    [Required(ErrorMessage = "Please select a department")]
    public int? DepartmentId { get; set; }
}

public record UpdateUserDto(
    string Username,
    string Password,
    string Email,
    string Role,
    int DepartmentId
);

// ═══════════════════════════════════════════════════════════════
//  AUDIT
// ═══════════════════════════════════════════════════════════════

public class AuditLogDto
{
    public int Id { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public int PerformedById { get; set; }
    public string PerformedBy { get; set; } = string.Empty;
    public DateTime PerformedAt { get; set; }
}

// ═══════════════════════════════════════════════════════════════
//  DEPARTMENT
// ═══════════════════════════════════════════════════════════════

public record DepartmentDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<UsersDto> Users { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public record CreateDepartmentDto
{
    public string Name { get; set; }
}

// ═══════════════════════════════════════════════════════════════
//  TASK
// ═══════════════════════════════════════════════════════════════

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

public record UpdateTaskDTO(
    string Title,
    string Description,
    string Status,
    int DepartmentId,
    DateTime UpdatedAt
);

public record TaskAssignmentDto(
    int TaskItemId,
    TaskItemDto TaskItem,
    int UserId,
    UsersDto User,
    DateTime AssignedAt
);

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

public record TaskDashboardItemDto(
    int Id,
    string Title,
    string Description,
    TaskStatusEnum Status,
    int CreatedById,
    int DepartmentId,
    string DepartmentName,
    string? AssignedUsername,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

// ═══════════════════════════════════════════════════════════════
//  SUBTASK
// ═══════════════════════════════════════════════════════════════

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
    int? AssignedToId
);

public record UpdateSubTaskDTO(
    string Title,
    string Description,
    string Status,
    int AssignedId,
    DateTime UpdatedAt
);

// ═══════════════════════════════════════════════════════════════
//  TASK STATUS (user view)
// ═══════════════════════════════════════════════════════════════

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

// ═══════════════════════════════════════════════════════════════
//  COMMENTS
// ═══════════════════════════════════════════════════════════════

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

// ═══════════════════════════════════════════════════════════════
//  PERMISSIONS
// ═══════════════════════════════════════════════════════════════

public class UserPermissionDto
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public List<string> Permissions { get; set; } = new();
}

public class GrantPermissionDto
{
    public int UserId { get; set; }
    public int GrantedById { get; set; }
    public PermissionType Permission { get; set; }
}

public class RevokePermissionDto
{
    public int UserId { get; set; }
    public PermissionType Permission { get; set; }
}

// ═══════════════════════════════════════════════════════════════
//  PROGRESS TRACKING  (NEW)
// ═══════════════════════════════════════════════════════════════

public class UserProgressDto
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;

    // Task counts
    public int TotalTasks { get; set; }
    public int PendingTasks { get; set; }
    public int InProgressTasks { get; set; }
    public int CompletedTasks { get; set; }
    public double TaskCompletionPercent =>
        TotalTasks == 0 ? 0 : Math.Round((double)CompletedTasks / TotalTasks * 100, 1);

    // Subtask counts
    public int TotalSubtasks { get; set; }
    public int PendingSubtasks { get; set; }
    public int InProgressSubtasks { get; set; }
    public int CompletedSubtasks { get; set; }
    public double SubtaskCompletionPercent =>
        TotalSubtasks == 0 ? 0 : Math.Round((double)CompletedSubtasks / TotalSubtasks * 100, 1);

    // Combined overall
    public int TotalItems => TotalTasks + TotalSubtasks;
    public int CompletedItems => CompletedTasks + CompletedSubtasks;
    public double OverallCompletionPercent =>
        TotalItems == 0 ? 0 : Math.Round((double)CompletedItems / TotalItems * 100, 1);

    // Task detail breakdown
    public List<TaskProgressDto> Tasks { get; set; } = new();
}

public class TaskProgressDto
{
    public int TaskId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;

    public int TotalSubtasks { get; set; }
    public int CompletedSubtasks { get; set; }
    public double SubtaskPercent =>
        TotalSubtasks == 0 ? 0 : Math.Round((double)CompletedSubtasks / TotalSubtasks * 100, 1);

    public List<SubtaskProgressDto> Subtasks { get; set; } = new();
}

public class SubtaskProgressDto
{
    public int SubtaskId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}