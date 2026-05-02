using Final_Project_Adv.Domain.DTO;
using Final_Project_Adv.Domain.Enums;
using Final_Project_Adv.Infrastructure.Data;
using Final_Project_Adv.Models;
using Microsoft.EntityFrameworkCore;

namespace Final_Project_Adv.Services
{
    public class EmployeeServices : IEmployeeServices
    {
        private readonly AppDbContext context;
        private readonly AuditService auditService;
        private readonly PermissionService permissionService;

        public EmployeeServices(
            AppDbContext context,
            AuditService auditService,
            PermissionService permissionService)
        {
            this.context = context;
            this.auditService = auditService;
            this.permissionService = permissionService;
        }

        // ─────────────────────────────────────────────────────────────────────
        // READ
        // ─────────────────────────────────────────────────────────────────────

        public async Task<IEnumerable<TaskItemDto>> GetMyTasksAsync(int userId)
        {
            return await context.TaskAssignment
                .Where(a => a.UserId == userId)
                .Select(a => new TaskItemDto(
                    a.TaskItem.Id,
                    a.TaskItem.Title,
                    a.TaskItem.Description,
                    a.TaskItem.Status,
                    a.TaskItem.CreatedById,
                    a.TaskItem.DepartmentId,
                    a.TaskItem.CreatedAt,
                    a.TaskItem.UpdatedAt))
                .ToListAsync();
        }

        public async Task<IEnumerable<SubtaskDto>> GetMySubtasksAsync(int userId)
        {
            return await context.Subtask
                .Where(s => s.AssignedToId == userId)
                .Select(s => new SubtaskDto(
                    s.Id, s.Title, s.Description, s.Status,
                    s.TaskItemId, s.AssignedToId, s.CreatedById,
                    s.CreatedAt, s.UpdatedAt))
                .ToListAsync();
        }

        // ─────────────────────────────────────────────────────────────────────
        // ACCEPT  (Pending → InProgress)
        // ─────────────────────────────────────────────────────────────────────

        public async Task<bool> AcceptTaskAsync(int taskId)
        {
            var task = await context.TaskItem.FindAsync(taskId);
            if (task == null) return false;

            if (task.Status != Domain.Enums.TaskStatus.Pending)
                throw new InvalidOperationException("Only pending tasks can be accepted.");

            var old = new { task.Id, Status = task.Status.ToString() };

            task.Status = Domain.Enums.TaskStatus.InProgress;
            task.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();

            await auditService.LogAsync(
                "Accepted", "TaskItem", task.Id,
                old,
                new
                {
                    task.Id,
                    task.Title,
                    Status = task.Status.ToString(),
                    task.DepartmentId,
                    task.CreatedById
                },
                task.CreatedById);

            return true;
        }

        // ─────────────────────────────────────────────────────────────────────
        // SUBMIT TASK  (→ Completed)
        // ─────────────────────────────────────────────────────────────────────

        public async Task<bool> SubmitTaskAsync(int taskId)
        {
            var task = await context.TaskItem.FindAsync(taskId);
            if (task == null) return false;

            if (task.Status == Domain.Enums.TaskStatus.Completed)
                throw new InvalidOperationException("Task is already completed.");

            var old = new { task.Id, Status = task.Status.ToString() };

            task.Status = Domain.Enums.TaskStatus.Completed;
            task.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();

            await auditService.LogAsync(
                "Submitted", "TaskItem", task.Id,
                old,
                new
                {
                    task.Id,
                    task.Title,
                    Status = task.Status.ToString(),
                    task.DepartmentId,
                    task.CreatedById
                },
                task.CreatedById);

            return true;
        }

        // ─────────────────────────────────────────────────────────────────────
        // SUBMIT SUBTASK  (→ Completed)
        // ─────────────────────────────────────────────────────────────────────

        public async Task<bool> SubmitSubtaskAsync(int subtaskId)
        {
            var subtask = await context.Subtask.FindAsync(subtaskId);
            if (subtask == null) return false;

            if (subtask.Status == Domain.Enums.TaskStatus.Completed)
                throw new InvalidOperationException("Subtask is already completed.");

            var old = new { subtask.Id, Status = subtask.Status.ToString() };

            subtask.Status = Domain.Enums.TaskStatus.Completed;
            subtask.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();

            await auditService.LogAsync(
                "Submitted", "Subtask", subtask.Id,
                old,
                new
                {
                    subtask.Id,
                    subtask.Title,
                    Status = subtask.Status.ToString(),
                    subtask.TaskItemId,
                    subtask.AssignedToId,
                    subtask.CreatedById
                },
                subtask.CreatedById);

            return true;
        }

        // ─────────────────────────────────────────────────────────────────────
        // CREATE SUBTASK  (NEW)
        // ─────────────────────────────────────────────────────────────────────

        public async Task<SubtaskDto> CreateSubtaskAsync(CreateSubtaskDto dto)
        {
            // 1. Parent task must exist and be InProgress
            var parentTask = await context.TaskItem.FindAsync(dto.TaskItemId);

            if (parentTask == null)
                throw new KeyNotFoundException(
                    $"Task with ID {dto.TaskItemId} does not exist.");

            if (parentTask.Status != Domain.Enums.TaskStatus.InProgress)
                throw new InvalidOperationException(
                    "Subtasks can only be added to tasks that are In Progress.");

            // 2. Optional assignee must belong to the same department
            if (dto.AssignedToId.HasValue)
            {
                var assignee = await context.Users
                    .FirstOrDefaultAsync(u => u.Id == dto.AssignedToId.Value);

                if (assignee == null)
                    throw new KeyNotFoundException(
                        $"User with ID {dto.AssignedToId} does not exist.");

                if (assignee.DepartmentId != parentTask.DepartmentId)
                    throw new InvalidOperationException(
                        "The assignee must belong to the same department as the task.");
            }

            // 3. Persist
            var subtask = new Subtask
            {
                Title = dto.Title,
                Description = dto.Description,
                Status = Domain.Enums.TaskStatus.Pending,
                TaskItemId = dto.TaskItemId,
                CreatedById = dto.CreatedById,
                AssignedToId = dto.AssignedToId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            context.Subtask.Add(subtask);
            await context.SaveChangesAsync();

            // 4. Audit log
            await auditService.LogAsync(
                action: "SubtaskCreated",
                entityType: "Subtask",
                entityId: subtask.Id,
                oldValue: null,
                newValue: new
                {
                    subtask.Id,
                    subtask.Title,
                    subtask.Description,
                    Status = subtask.Status.ToString(),
                    subtask.TaskItemId,
                    subtask.CreatedById,
                    AssignedToId = subtask.AssignedToId
                },
                performedById: dto.CreatedById);

            return new SubtaskDto(
                subtask.Id,
                subtask.Title,
                subtask.Description,
                subtask.Status,
                subtask.TaskItemId,
                subtask.AssignedToId,
                subtask.CreatedById,
                subtask.CreatedAt,
                subtask.UpdatedAt);
        }

        // ─────────────────────────────────────────────────────────────────────
        // REQUEST ASSIGNMENT
        // ─────────────────────────────────────────────────────────────────────

        public async Task<bool> RequestTaskAssignmentAsync(int userId, int taskId)
        {
            var taskExists = await context.TaskItem.AnyAsync(t => t.Id == taskId);
            if (!taskExists)
                throw new KeyNotFoundException($"Task with ID {taskId} does not exist.");

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

        // ─────────────────────────────────────────────────────────────────────
        // COMMENTS
        // ─────────────────────────────────────────────────────────────────────

        public async Task<TaskCommentDto> AddTaskCommentAsync(CreateTaskCommentDto dto)
        {
            await permissionService.RequirePermissionAsync(dto.AuthorId, PermissionType.AddComment);

            var taskExists = await context.TaskItem.AnyAsync(t => t.Id == dto.TaskItemId);
            if (!taskExists)
                throw new KeyNotFoundException($"Task with ID {dto.TaskItemId} does not exist.");

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

            await auditService.LogAsync(
                "CommentAdded", "TaskComment", comment.Id,
                null,
                new { comment.Id, comment.Content, comment.AuthorId, comment.TaskItemId },
                dto.AuthorId);

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
            await permissionService.RequirePermissionAsync(dto.AuthorId, PermissionType.AddComment);

            var subtaskExists = await context.Subtask.AnyAsync(s => s.Id == dto.SubtaskId);
            if (!subtaskExists)
                throw new KeyNotFoundException($"Subtask with ID {dto.SubtaskId} does not exist.");

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

            await auditService.LogAsync(
                "CommentAdded", "SubtaskComment", comment.Id,
                null,
                new { comment.Id, comment.Content, comment.AuthorId, comment.SubtaskId },
                dto.AuthorId);

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