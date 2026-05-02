using Final_Project_Adv.Domain.DTO;
using Final_Project_Adv.Domain.Enums;
using Final_Project_Adv.Infrastructure.Data;
using Final_Project_Adv.Models;
using Microsoft.EntityFrameworkCore;

namespace Final_Project_Adv.Services
{
    public class TaskLimitExceededException : Exception
    {
        public List<UsersDto> AvailableUsers { get; }
        public TaskLimitExceededException(string message, List<UsersDto> availableUsers)
            : base(message) { AvailableUsers = availableUsers; }
    }

    public class ManagerServices : IManagerServices
    {
        private readonly AppDbContext context;
        private readonly AuditService auditService;
        private readonly PermissionService permissionService;

        public ManagerServices(AppDbContext context, AuditService auditService, PermissionService permissionService)
        {
            this.context = context;
            this.auditService = auditService;
            this.permissionService = permissionService;
        }

        // ─────────────────────────────────────────────────────────────────────
        // USER
        // ─────────────────────────────────────────────────────────────────────

        public async Task<UsersDto> CreateUserAsync(CreateUserDto dto)
        {
            if (!dto.DepartmentId.HasValue)
                throw new Exception("Please select a department.");

            var departmentExists = await context.Department.AnyAsync(d => d.Id == dto.DepartmentId.Value);
            if (!departmentExists)
                throw new Exception($"Department ID {dto.DepartmentId} does not exist.");

            var userExists = await context.Users.AnyAsync(u => u.Username == dto.Username);
            if (userExists)
                throw new Exception("Username is already taken.");

            var user = new Users
            {
                Username = dto.Username,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Email = dto.Email,
                Role = dto.Role,
                DepartmentId = dto.DepartmentId.Value,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            return new UsersDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                DepartmentId = user.DepartmentId
            };
        }

        public async Task<UsersDto> UpdateUserAsync(int id, UpdateUserDto dto)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user is null)
                throw new KeyNotFoundException($"User with ID {id} does not exist.");

            user.Username = dto.Username;
            user.Password = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            user.Email = dto.Email;
            user.Role = dto.Role;
            user.DepartmentId = dto.DepartmentId;
            user.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();

            return new UsersDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                DepartmentId = user.DepartmentId
            };
        }

        public async Task<IEnumerable<UsersDto>> GetAllUsersAsync()
        {
            return await context.Users.Select(u => new UsersDto
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                Role = u.Role,
                DepartmentId = u.DepartmentId
            }).ToListAsync();
        }

        public async Task<Users?> GetUserByEmailAsync(string email)
            => await context.Users.FirstOrDefaultAsync(u => u.Email == email);

        // ─────────────────────────────────────────────────────────────────────
        // TASK
        // ─────────────────────────────────────────────────────────────────────

        public async Task<TaskItemDto> CreateTaskAsync(CreateTaskItemDto dto)
        {
            // Permission check — only users with CreateTask permission (or admin/manager by role)
            await permissionService.RequirePermissionAsync(dto.CreatedById, PermissionType.CreateTask);

            var deptExists = await context.Department.AnyAsync(d => d.Id == dto.DepartmentId);
            if (!deptExists)
                throw new KeyNotFoundException($"Department with ID {dto.DepartmentId} does not exist.");

            var task = new TaskItem
            {
                Title = dto.Title,
                Description = dto.Description,
                CreatedById = dto.CreatedById,
                DepartmentId = dto.DepartmentId,
                Status = Domain.Enums.TaskStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            context.TaskItem.Add(task);
            await context.SaveChangesAsync();

            await auditService.LogAsync(
                "Created", "TaskItem", task.Id,
                null,
                new
                {
                    task.Id,
                    task.Title,
                    task.Description,
                    Status = task.Status.ToString(),
                    task.DepartmentId,
                    task.CreatedById
                },
                dto.CreatedById);

            return new TaskItemDto(task.Id, task.Title, task.Description,
                task.Status, task.CreatedById, task.DepartmentId,
                task.CreatedAt, task.UpdatedAt);
        }

        public async Task<bool> DeleteTaskAsync(int id, int requestingUserId)
        {
            await permissionService.RequirePermissionAsync(requestingUserId, PermissionType.DeleteTask);

            var task = await context.TaskItem.FindAsync(id);
            if (task == null) return false;

            await auditService.LogAsync(
                "Deleted", "TaskItem", id,
                new { task.Id, task.Title, Status = task.Status.ToString(), task.DepartmentId },
                null,
                requestingUserId);

            var deleted = await context.TaskItem.Where(t => t.Id == id).ExecuteDeleteAsync();
            return deleted > 0;
        }

        // ✅ Remove the old stub — this was throwing NotImplementedException
        public Task<bool> DeleteTaskAsync(int id)
            => throw new NotSupportedException("Use DeleteTaskAsync(int id, int requestingUserId) instead.");

        public async Task<TaskItemDto> UpdateTasksAsync(int id, UpdateTaskDTO dto)
        {
            var task = await context.TaskItem.FirstOrDefaultAsync(t => t.Id == id);
            if (task is null)
                throw new KeyNotFoundException($"Task with ID {id} does not exist.");

            var oldTask = new
            {
                task.Title,
                task.Description,
                Status = task.Status.ToString(),
                task.DepartmentId
            };

            task.Title = dto.Title;
            task.Description = dto.Description;
            task.DepartmentId = dto.DepartmentId;
            task.Status = Enum.Parse<Domain.Enums.TaskStatus>(dto.Status);
            task.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();

            await auditService.LogAsync(
                "Updated", "TaskItem", task.Id,
                oldTask,
                new
                {
                    task.Id,
                    task.Title,
                    task.Description,
                    Status = task.Status.ToString(),
                    task.DepartmentId,
                    task.UpdatedAt
                },
                task.CreatedById);

            return new TaskItemDto(task.Id, task.Title, task.Description,
                task.Status, task.CreatedById, task.DepartmentId,
                task.CreatedAt, task.UpdatedAt);
        }

        public async Task<IEnumerable<TaskItemDto>> GetAllTasksAsync()
        {
            return await context.TaskItem.Select(t => new TaskItemDto(
                t.Id, t.Title, t.Description, t.Status,
                t.CreatedById, t.DepartmentId, t.CreatedAt, t.UpdatedAt
            )).ToListAsync();
        }

        // ─────────────────────────────────────────────────────────────────────
        // TASK ASSIGNMENT
        // ─────────────────────────────────────────────────────────────────────

        public async Task<TaskAssignmentDto> TaskAssignAsync(int userId, int taskId)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            var task = await context.TaskItem.FirstOrDefaultAsync(t => t.Id == taskId);

            if (user is null) throw new KeyNotFoundException($"User with ID {userId} does not exist.");
            if (task is null) throw new KeyNotFoundException($"Task with ID {taskId} does not exist.");

            // Enforce 2-task limit
            var userTaskCount = await context.TaskAssignment.CountAsync(a => a.UserId == userId);
            if (userTaskCount >= 2)
            {
                var available = await context.Users
                    .Where(u => u.DepartmentId == task.DepartmentId
                             && !context.TaskAssignment.Any(a => a.UserId == u.Id))
                    .Select(u => new UsersDto
                    {
                        Id = u.Id,
                        Username = u.Username,
                        Email = u.Email,
                        Role = u.Role,
                        DepartmentId = u.DepartmentId
                    }).ToListAsync();

                throw new TaskLimitExceededException(
                    "This user already has the maximum allowed number of tasks (2).", available);
            }

            var alreadyAssigned = await context.TaskAssignment
                .AnyAsync(a => a.TaskItemId == taskId);
            if (alreadyAssigned)
                throw new Exception("Task is already assigned to someone.");

            var lb = TimeZoneInfo.FindSystemTimeZoneById("Asia/Beirut");
            var assignment = new TaskAssignment
            {
                TaskItemId = taskId,
                UserId = userId,
                AssignedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, lb)
            };

            context.TaskAssignment.Add(assignment);
            await context.SaveChangesAsync();

            await auditService.LogAsync(
                "Assigned", "TaskAssignment", taskId,
                null, new { userId, taskId }, userId);

            return new TaskAssignmentDto(
                task.Id,
                new TaskItemDto(task.Id, task.Title, task.Description,
                    task.Status, task.CreatedById, task.DepartmentId,
                    task.CreatedAt, task.UpdatedAt),
                user.Id,
                new UsersDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    Role = user.Role,
                    DepartmentId = user.DepartmentId
                },
                assignment.AssignedAt);
        }

        public async Task<TaskAssignmentDto> UnassignTaskAsync(int userId, int taskId)
        {
            var assignment = await context.TaskAssignment
                .FirstOrDefaultAsync(a => a.UserId == userId && a.TaskItemId == taskId);
            if (assignment is null)
                throw new KeyNotFoundException("This task is not assigned to this user.");

            var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            var task = await context.TaskItem.FirstOrDefaultAsync(t => t.Id == taskId);

            context.TaskAssignment.Remove(assignment);
            await context.SaveChangesAsync();

            await auditService.LogAsync(
                "Unassigned", "TaskAssignment", taskId,
                new { userId, taskId }, null, userId);

            return new TaskAssignmentDto(
                assignment.TaskItemId,
                new TaskItemDto(task!.Id, task.Title, task.Description,
                    task.Status, task.CreatedById, task.DepartmentId,
                    task.CreatedAt, task.UpdatedAt),
                assignment.UserId,
                new UsersDto
                {
                    Id = user!.Id,
                    Username = user.Username,
                    Email = user.Email,
                    Role = user.Role,
                    DepartmentId = user.DepartmentId
                },
                assignment.AssignedAt);
        }

        // ─────────────────────────────────────────────────────────────────────
        // SUBTASK
        // ─────────────────────────────────────────────────────────────────────

        public async Task<SubtaskDto> CreateSubtaskAsync(CreateSubtaskDto dto)
        {
            await permissionService.RequirePermissionAsync(dto.CreatedById, PermissionType.CreateSubtask);

            // ✅ Verify the parent task exists
            var parentTask = await context.TaskItem.FindAsync(dto.TaskItemId);
            if (parentTask is null)
                throw new KeyNotFoundException($"Parent task with ID {dto.TaskItemId} does not exist.");

            // ✅ Verify the assigned employee exists (if provided)
            if (dto.AssignedToId.HasValue)
            {
                var assignee = await context.Users.FindAsync(dto.AssignedToId.Value);
                if (assignee is null)
                    throw new KeyNotFoundException($"User with ID {dto.AssignedToId} does not exist.");
            }

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

            await auditService.LogAsync(
                "Created", "Subtask", subtask.Id,
                null,
                new
                {
                    subtask.Id,
                    subtask.Title,
                    subtask.Description,
                    Status = subtask.Status.ToString(),
                    subtask.TaskItemId,
                    subtask.AssignedToId,
                    subtask.CreatedById
                },
                dto.CreatedById);

            return new SubtaskDto(subtask.Id, subtask.Title, subtask.Description,
                subtask.Status, subtask.TaskItemId, subtask.AssignedToId,
                subtask.CreatedById, subtask.CreatedAt, subtask.UpdatedAt);
        }

        public async Task<bool> DeleteSubTaskAsync(int id, int requestingUserId)
        {
            await permissionService.RequirePermissionAsync(requestingUserId, PermissionType.DeleteSubtask);

            var subtask = await context.Subtask.FindAsync(id);
            if (subtask == null) return false;

            await auditService.LogAsync(
                "Deleted", "Subtask", id,
                new
                {
                    subtask.Id,
                    subtask.Title,
                    Status = subtask.Status.ToString(),
                    subtask.TaskItemId,
                    subtask.AssignedToId
                },
                null, requestingUserId);

            var deleted = await context.Subtask.Where(s => s.Id == id).ExecuteDeleteAsync();
            return deleted > 0;
        }

        public async Task<SubtaskDto> UpdateSubTasksAsync(int id, UpdateSubTaskDTO dto)
        {
            var subtask = await context.Subtask.FirstOrDefaultAsync(t => t.Id == id);
            if (subtask is null)
                throw new KeyNotFoundException($"SubTask with ID {id} does not exist.");

            var oldSubtask = new
            {
                subtask.Title,
                subtask.Description,
                Status = subtask.Status.ToString(),
                subtask.AssignedToId
            };

            subtask.Title = dto.Title;
            subtask.Description = dto.Description;
            subtask.Status = Enum.Parse<Domain.Enums.TaskStatus>(dto.Status);
            subtask.AssignedToId = dto.AssignedId;
            subtask.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();

            await auditService.LogAsync(
                "Updated", "Subtask", subtask.Id,
                oldSubtask,
                new
                {
                    subtask.Id,
                    subtask.Title,
                    subtask.Description,
                    Status = subtask.Status.ToString(),
                    subtask.TaskItemId,
                    subtask.AssignedToId,
                    subtask.UpdatedAt
                },
                subtask.CreatedById);

            return new SubtaskDto(subtask.Id, subtask.Title, subtask.Description,
                subtask.Status, subtask.TaskItemId, subtask.AssignedToId,
                subtask.CreatedById, subtask.CreatedAt, subtask.UpdatedAt);
        }

        public async Task<IEnumerable<SubtaskDto>> GetAllSubTasksAsync()
        {
            return await context.Subtask.Select(s => new SubtaskDto(
                s.Id, s.Title, s.Description, s.Status,
                s.TaskItemId, s.AssignedToId, s.CreatedById,
                s.CreatedAt, s.UpdatedAt
            )).ToListAsync();
        }

        // ─────────────────────────────────────────────────────────────────────
        // COMMENTS
        // ─────────────────────────────────────────────────────────────────────

        public async Task<TaskCommentDto> TaskCommentAsync(CreateTaskCommentDto dto)
        {
            await permissionService.RequirePermissionAsync(dto.AuthorId, PermissionType.AddComment);

            var taskExists = await context.TaskItem.AnyAsync(t => t.Id == dto.TaskItemId);
            if (!taskExists)
                throw new KeyNotFoundException($"Task with ID {dto.TaskItemId} does not exist.");

            var lb = TimeZoneInfo.FindSystemTimeZoneById("Asia/Beirut");
            var comment = new TaskComment
            {
                Content = dto.Content,
                AuthorId = dto.AuthorId,
                TaskItemId = dto.TaskItemId,
                CreatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, lb),
                UpdatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, lb)
            };

            context.TaskComment.Add(comment);
            await context.SaveChangesAsync();

            await auditService.LogAsync(
                "CommentAdded", "TaskComment", comment.Id,
                null,
                new
                {
                    comment.Id,
                    comment.Content,
                    comment.AuthorId,
                    comment.TaskItemId,
                    comment.CreatedAt
                },
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

        public async Task<SubtaskCommentDto> SubTaskCommentAsync(CreateSubtaskCommentDto dto)
        {
            await permissionService.RequirePermissionAsync(dto.AuthorId, PermissionType.AddComment);

            var subtaskExists = await context.Subtask.AnyAsync(s => s.Id == dto.SubtaskId);
            if (!subtaskExists)
                throw new KeyNotFoundException($"Subtask with ID {dto.SubtaskId} does not exist.");

            var lb = TimeZoneInfo.FindSystemTimeZoneById("Asia/Beirut");
            var comment = new SubtaskComment
            {
                Content = dto.Content,
                AuthorId = dto.AuthorId,
                SubtaskId = dto.SubtaskId,
                CreatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, lb),
                UpdatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, lb)
            };

            context.SubtaskComment.Add(comment);
            await context.SaveChangesAsync();

            await auditService.LogAsync(
                "CommentAdded", "SubtaskComment", comment.Id,
                null,
                new
                {
                    comment.Id,
                    comment.Content,
                    comment.AuthorId,
                    comment.SubtaskId,
                    comment.CreatedAt
                },
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

        // ─────────────────────────────────────────────────────────────────────
        // MISC
        // ─────────────────────────────────────────────────────────────────────

        public async Task<UserTaskStatusDto> GetUserTaskStatus(int userId)
        {
            var user = await context.Users
                .Include(u => u.TaskAssignments)
                    .ThenInclude(ta => ta.TaskItem)
                        .ThenInclude(t => t.Subtasks)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) throw new KeyNotFoundException("User not found.");

            return new UserTaskStatusDto
            {
                UserId = user.Id,
                Username = user.Username,
                Tasks = user.TaskAssignments.Select(ta => new TaskStatusDto
                {
                    TaskId = ta.TaskItem.Id,
                    Title = ta.TaskItem.Title,
                    Status = ta.TaskItem.Status.ToString(),
                    Subtasks = ta.TaskItem.Subtasks.Select(st => new SubtaskStatusDto
                    {
                        SubtaskId = st.Id,
                        Title = st.Title,
                        Status = st.Status.ToString()
                    }).ToList()
                }).ToList()
            };
        }

        public async Task<List<TaskWithSubtasksDto>> GetOldTasksAsync()
        {
            var lb = TimeZoneInfo.FindSystemTimeZoneById("Asia/Beirut");
            var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, lb);
            var twoWeeksAgo = now.AddDays(-14);

            return await context.TaskItem
                .Where(t => t.CreatedAt <= twoWeeksAgo || t.UpdatedAt <= twoWeeksAgo)
                .Select(t => new TaskWithSubtasksDto(
                    t.Id, t.Title, t.Description, t.Status,
                    t.CreatedById, t.DepartmentId, t.CreatedAt, t.UpdatedAt,
                    t.Subtasks
                        .Where(s => s.CreatedAt <= twoWeeksAgo || s.UpdatedAt <= twoWeeksAgo)
                        .Select(s => new SubtaskDto(s.Id, s.Title, s.Description, s.Status,
                            s.TaskItemId, s.AssignedToId, s.CreatedById,
                            s.CreatedAt, s.UpdatedAt))
                        .ToList()
                )).ToListAsync();
        }

        public async Task<IEnumerable<TaskDashboardItemDto>> GetDashboardTasksAsync()
        {
            return await context.TaskItem
                .Include(t => t.Department)
                .Include(t => t.TaskAssignments).ThenInclude(a => a.User)
                .Select(t => new TaskDashboardItemDto(
                    t.Id, t.Title, t.Description, t.Status,
                    t.CreatedById, t.DepartmentId, t.Department.Name,
                    t.TaskAssignments.FirstOrDefault() != null
                        ? t.TaskAssignments.FirstOrDefault()!.User.Username
                        : null,
                    t.CreatedAt, t.UpdatedAt
                )).ToListAsync();
        }
    }
}