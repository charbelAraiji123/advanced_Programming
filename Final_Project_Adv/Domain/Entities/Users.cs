using TaskStatusEnum = Final_Project_Adv.Domain.Enums.TaskStatus;
namespace Final_Project_Adv.Domain.Entities
{
    
    public class Users
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;

        // FK → Department
        public int DepartmentId { get; set; }
        public Department Department { get; set; } = null!;

        // 🔥 Many-to-Many (assigned tasks)
        public List<TaskAssignment> TaskAssignments { get; set; } = new();

        // 🔥 One-to-Many (created tasks)
        public List<TaskItem> CreatedTasks { get; set; } = new();

        // 🔥 Subtasks assigned to this user
        public List<Subtask> AssignedSubtasks { get; set; } = new();
        public List<TaskComment> TaskComments { get; set; } = new ();
        public List<SubtaskComment> SubtaskComments { get; set; } = new();
        public List<AuditLog> AuditLogs { get; set; } = new();
        public List<Schedule> OrganizedSchedules { get; set; } = new();
        public List<ScheduleParticipant> ScheduleParticipants { get; set; } = new();
        public List<Subtask> CreatedSubtasks { get; set; } = new();

        // 🔥 Audit
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }


   
    public class Department
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public List<Users> Users { get; set; } = new();
        public List<TaskItem> Tasks { get; set; } = new();

        // 🔥 Audit
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }


    
    public class TaskItem   // ⚠️ renamed to avoid conflict
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public TaskStatusEnum Status { get; set; } = TaskStatusEnum.Pending;

        // Creator
        public int CreatedById { get; set; }
        public Users CreatedBy { get; set; } = null!;

        // Department
        public int DepartmentId { get; set; }
        public Department Department { get; set; } = null!;

        // Assignments (M:N)
        public List<TaskAssignment> TaskAssignments { get; set; } = new();

        // Subtasks
        public List<Subtask> Subtasks { get; set; } = new();

        public List<TaskComment> Comments { get; set; } = new();
        public List<Schedule> Schedules { get; set; } = new();

        // 🔥 Audit
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }


   
    public class TaskAssignment
    {
        public int TaskItemId { get; set; }
        public TaskItem TaskItem { get; set; } = null!;

        public int UserId { get; set; }
        public Users User { get; set; } = null!;

        // 🔥 Extra (VERY useful later)
        public DateTime AssignedAt { get; set; }
    }


    
    public class Subtask
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public TaskStatusEnum Status { get; set; } = TaskStatusEnum.Pending;

        // Parent Task
        public int TaskItemId { get; set; }
        public TaskItem TaskItem { get; set; } = null!;

        // Assigned user (optional)
        public int? AssignedToId { get; set; }
        public Users? AssignedTo { get; set; }

        // Creator
        public int CreatedById { get; set; }
        public Users CreatedBy { get; set; } = null!;

        public List<SubtaskComment> Comments { get; set; } = new();

        // 🔥 Audit
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
    // Add at the bottom of Final_Project_Adv.Domain.Entities namespace

    public class TaskComment
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;

        // Author
        public int AuthorId { get; set; }
        public Users Author { get; set; } = null!;

        // Required FK → TaskItem
        public int TaskItemId { get; set; }
        public TaskItem TaskItem { get; set; } = null!;

        // Audit
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class SubtaskComment
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;

        // Author
        public int AuthorId { get; set; }
        public Users Author { get; set; } = null!;

        // Required FK → Subtask
        public int SubtaskId { get; set; }
        public Subtask Subtask { get; set; } = null!;

        // Audit
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
    public class AuditLog
    {
        public int Id { get; set; }
        public string Action { get; set; } = string.Empty;       // e.g. "Created", "StatusChanged"
        public string EntityType { get; set; } = string.Empty;   // "TaskItem" or "Subtask"
        public int EntityId { get; set; }                        // ID of the affected row
        public string? OldValue { get; set; }                    // JSON snapshot before
        public string? NewValue { get; set; }                    // JSON snapshot after

        // Who triggered it
        public int PerformedById { get; set; }
        public Users PerformedBy { get; set; } = null!;

        public DateTime PerformedAt { get; set; }
    }

    public class Schedule
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        // Organizer
        public int OrganizerId { get; set; }
        public Users Organizer { get; set; } = null!;

        // Optional: link meeting to a task
        public int? TaskItemId { get; set; }
        public TaskItem? TaskItem { get; set; }

        // Participants (M:N)
        public List<ScheduleParticipant> Participants { get; set; } = new();

        // Audit
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class ScheduleParticipant
    {
        public int ScheduleId { get; set; }
        public Schedule Schedule { get; set; } = null!;

        public int UserId { get; set; }
        public Users User { get; set; } = null!;

        public DateTime JoinedAt { get; set; }
    }
}