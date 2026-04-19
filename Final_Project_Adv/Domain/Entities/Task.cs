using System;
using System.Collections.Generic;
using TaskStatusEnum = Final_Project_Adv.Domain.Enums.TaskStatus;
namespace Final_Project_Adv.Domain.Entities 
{
    public class TaskItem   //  renamed to avoid conflict
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

        //  Audit
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }



    public class TaskAssignment
    {
        public int TaskItemId { get; set; }
        public TaskItem TaskItem { get; set; } = null!;

        public int UserId { get; set; }
        public Users User { get; set; } = null!;

        //  Extra (VERY useful later)
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

        //  Audit
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
}