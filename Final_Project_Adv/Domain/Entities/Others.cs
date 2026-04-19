using System;
using System.Collections.Generic;
using TaskStatusEnum = Final_Project_Adv.Domain.Enums.TaskStatus;
namespace Final_Project_Adv.Domain.Entities 
{
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
