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

        //  Many-to-Many (assigned tasks)
        public List<TaskAssignment> TaskAssignments { get; set; } = new();

        //  One-to-Many (created tasks)
        public List<TaskItem> CreatedTasks { get; set; } = new();

        //  Subtasks assigned to this user
        public List<Subtask> AssignedSubtasks { get; set; } = new();
        public List<TaskComment> TaskComments { get; set; } = new ();
        public List<SubtaskComment> SubtaskComments { get; set; } = new();
        public List<AuditLog> AuditLogs { get; set; } = new();
        public List<Schedule> OrganizedSchedules { get; set; } = new();
        public List<ScheduleParticipant> ScheduleParticipants { get; set; } = new();
        public List<Subtask> CreatedSubtasks { get; set; } = new();

        //  Audit
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }


    
 
  
}