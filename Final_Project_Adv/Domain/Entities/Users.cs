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

        // 🔥 Audit
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
    
}