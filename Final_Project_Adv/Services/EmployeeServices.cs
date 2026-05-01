using Final_Project_Adv.Domain.DTO;
using Final_Project_Adv.Infrastructure.Data;
using Final_Project_Adv.Models;
using Microsoft.EntityFrameworkCore;

namespace Final_Project_Adv.Services
{
    public class EmployeeServices : IEmployeeServices
    {
        private readonly AppDbContext context;
        private readonly AuditService auditService;

        public EmployeeServices(AppDbContext context, AuditService auditService)
        {
            this.context = context;
            this.auditService = auditService;
        }

        // ================= TASKS =================

        public async Task<IEnumerable<TaskItemDto>> GetMyTasksAsync(int userId)
        {
            return await context.TaskAssignment
                .Where(a => a.UserId == userId)
                .Select(a => a.TaskItem)
                .Select(t => new TaskItemDto(
                    t.Id,
                    t.Title,
                    t.Description,
                    t.Status,
                    t.CreatedById,
                    t.DepartmentId,
                    t.CreatedAt,
                    t.UpdatedAt
                ))
                .ToListAsync();
        }

        public async Task<IEnumerable<SubtaskDto>> GetMySubtasksAsync(int userId)
        {
            return await context.Subtask
                .Where(s => s.AssignedToId == userId)
                .Select(s => new SubtaskDto(
                    s.Id,
                    s.Title,
                    s.Description,
                    s.Status,
                    s.TaskItemId,
                    s.AssignedToId,
                    s.CreatedById,
                    s.CreatedAt,
                    s.UpdatedAt
                ))
                .ToListAsync();
        }

        // ================= ACCEPT (Pending → InProgress) =================

        public async Task<bool> AcceptTaskAsync(int taskId)
        {
            var task = await context.TaskItem.FindAsync(taskId);
            if (task == null) return false;

            task.Status = Domain.Enums.TaskStatus.InProgress;
            task.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();

            // FIX: flat anonymous object — no navigation properties
            await auditService.LogAsync(
                "Accepted",
                "TaskItem",
                task.Id,
                null,
                new
                {
                    task.Id,
                    task.Title,
                    Status = task.Status.ToString(),
                    task.DepartmentId,
                    task.CreatedById
                },
                task.CreatedById
            );

            return true;
        }

        // ================= SUBMIT (→ Completed) =================

        public async Task<bool> SubmitTaskAsync(int taskId)
        {
            var task = await context.TaskItem.FindAsync(taskId);
            if (task == null) return false;

            task.Status = Domain.Enums.TaskStatus.Completed;
            task.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();

            // FIX: flat anonymous object — no navigation properties
            await auditService.LogAsync(
                "Submitted",
                "TaskItem",
                task.Id,
                null,
                new
                {
                    task.Id,
                    task.Title,
                    Status = task.Status.ToString(),
                    task.DepartmentId,
                    task.CreatedById
                },
                task.CreatedById
            );

            return true;
        }

        public async Task<bool> SubmitSubtaskAsync(int subtaskId)
        {
            var subtask = await context.Subtask.FindAsync(subtaskId);
            if (subtask == null) return false;

            subtask.Status = Domain.Enums.TaskStatus.Completed;
            subtask.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();

            // FIX: flat anonymous object — no navigation properties
            await auditService.LogAsync(
                "Submitted",
                "Subtask",
                subtask.Id,
                null,
                new
                {
                    subtask.Id,
                    subtask.Title,
                    Status = subtask.Status.ToString(),
                    subtask.TaskItemId,
                    subtask.AssignedToId,
                    subtask.CreatedById
                },
                subtask.CreatedById
            );

            return true;
        }

        // ================= REQUEST TASK =================

        public async Task<bool> RequestTaskAssignmentAsync(int userId, int taskId)
        {
            var exists = await context.TaskAssignment
                .AnyAsync(x => x.UserId == userId && x.TaskItemId == taskId);

            if (exists) return false;

            context.TaskAssignment.Add(new TaskAssignment
            {
                UserId = userId,
                TaskItemId = taskId,
                AssignedAt = DateTime.UtcNow
            });

            await context.SaveChangesAsync();
            return true;
        }

        // ================= COMMENTS =================

        public async Task<TaskCommentDto> AddTaskCommentAsync(CreateTaskCommentDto dto)
        {
            var comment = new TaskComment
            {
                Content = dto.Content,
                AuthorId = dto.AuthorId,
                TaskItemId = dto.TaskItemId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            context.TaskComment.Add(comment);
            await context.SaveChangesAsync();

            return new TaskCommentDto
            {
                Id = comment.Id,
                Content = comment.Content,
                AuthorId = comment.AuthorId,
                TaskItemId = comment.TaskItemId,
                CreatedAt = comment.CreatedAt,
                UpdatedAt = comment.UpdatedAt
            };
        }

        public async Task<SubtaskCommentDto> AddSubtaskCommentAsync(CreateSubtaskCommentDto dto)
        {
            var comment = new SubtaskComment
            {
                Content = dto.Content,
                AuthorId = dto.AuthorId,
                SubtaskId = dto.SubtaskId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            context.SubtaskComment.Add(comment);
            await context.SaveChangesAsync();

            return new SubtaskCommentDto
            {
                Id = comment.Id,
                Content = comment.Content,
                AuthorId = comment.AuthorId,
                SubtaskId = comment.SubtaskId,
                CreatedAt = comment.CreatedAt,
                UpdatedAt = comment.UpdatedAt
            };
        }
    }
}