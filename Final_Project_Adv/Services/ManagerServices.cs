using Final_Project_Adv.Domain.DTO;
using Final_Project_Adv.Domain.Enums;
using Final_Project_Adv.Infrastructure.Data;
using Final_Project_Adv.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Final_Project_Adv.Services
{
    public class TaskLimitExceededException : Exception
    {
        public List<UsersDto> AvailableUsers { get; }

        public TaskLimitExceededException(string message, List<UsersDto> availableUsers)
            : base(message)
        {
            AvailableUsers = availableUsers;
        }
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
                DepartmentId = (int)dto.DepartmentId,
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

        public async Task<TaskItemDto> CreateTaskAsync(CreateTaskItemDto dto)
        {
            var task = new TaskItem
            {
                Title = dto.Title,
                Description = dto.Description,
                CreatedById = dto.CreatedById,
                DepartmentId = dto.DepartmentId,
                Status = Domain.Enums.TaskStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            context.TaskItem.Add(task);
            await context.SaveChangesAsync();

            // FIX: flat anonymous object — no navigation properties
            await auditService.LogAsync(
                "Created",
                "TaskItem",
                task.Id,
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
                dto.CreatedById
            );

            return new TaskItemDto(
                task.Id,
                task.Title,
                task.Description,
                task.Status,
                task.CreatedById,
                task.DepartmentId,
                task.CreatedAt,
                task.UpdatedAt
            );
        }

        public async Task<bool> DeleteTaskAsync(int id, int requestingUserId)
        {
            await permissionService.RequirePermissionAsync(requestingUserId, PermissionType.DeleteTask);

            await auditService.LogAsync(
                "Deleted",
                "TaskItem",
                id,
                null,
                null,
                requestingUserId
            );

            var deleted = await context.TaskItem
                .Where(t => t.Id == id)
                .ExecuteDeleteAsync();

            return deleted > 0;
        }

        public async Task<SubtaskDto> CreateSubtaskAsync(CreateSubtaskDto dto)
        {
            await permissionService.RequirePermissionAsync(dto.CreatedById, PermissionType.CreateSubtask);

            var subtask = new Subtask
            {
                Title = dto.Title,
                Description = dto.Description,
                Status = Domain.Enums.TaskStatus.Pending,
                TaskItemId = dto.TaskItemId,
                CreatedById = dto.CreatedById,
                AssignedToId = dto.AssignedToId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            context.Subtask.Add(subtask);
            await context.SaveChangesAsync();

            // FIX: flat anonymous object — no navigation properties
            await auditService.LogAsync(
                "Created",
                "Subtask",
                subtask.Id,
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
                dto.CreatedById
            );

            return new SubtaskDto(
                subtask.Id,
                subtask.Title,
                subtask.Description,
                subtask.Status,
                subtask.TaskItemId,
                subtask.AssignedToId,
                subtask.CreatedById,
                subtask.CreatedAt,
                subtask.UpdatedAt
            );
        }

        public async Task<bool> DeleteSubTaskAsync(int id, int requestingUserId)
        {
            await permissionService.RequirePermissionAsync(requestingUserId, PermissionType.DeleteSubtask);

            await auditService.LogAsync(
                "Deleted",
                "Subtask",
                id,
                null,
                null,
                requestingUserId
            );

            var deleted = await context.Subtask
                .Where(item => item.Id == id)
                .ExecuteDeleteAsync();

            return deleted > 0;
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
            return await context.Users.Select(user => new UsersDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                DepartmentId = user.DepartmentId
            }).ToListAsync();
        }

        public async Task<IEnumerable<TaskItemDto>> GetAllTasksAsync()
        {
            return await context.TaskItem.Select(taskItem => new TaskItemDto(
                taskItem.Id,
                taskItem.Title,
                taskItem.Description,
                taskItem.Status,
                taskItem.CreatedById,
                taskItem.DepartmentId,
                taskItem.CreatedAt,
                taskItem.UpdatedAt
            )).ToListAsync();
        }

        public async Task<IEnumerable<SubtaskDto>> GetAllSubTasksAsync()
        {
            return await context.Subtask.Select(subtask => new SubtaskDto(
                subtask.Id,
                subtask.Title,
                subtask.Description,
                subtask.Status,
                subtask.TaskItemId,
                subtask.AssignedToId,
                subtask.CreatedById,
                subtask.CreatedAt,
                subtask.UpdatedAt
            )).ToListAsync();
        }

        public async Task<TaskItemDto> UpdateTasksAsync(int id, UpdateTaskDTO dto)
        {
            var task = await context.TaskItem
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task is null)
                throw new KeyNotFoundException($"Task with ID {id} does not exist.");

            var oldTask = new
            {
                task.Title,
                task.Description,
                Status = task.Status.ToString(),
                task.DepartmentId
            };

            var trackedTask = await context.TaskItem.FirstOrDefaultAsync(t => t.Id == id);

            trackedTask!.Title = dto.Title;
            trackedTask.Description = dto.Description;
            trackedTask.DepartmentId = dto.DepartmentId;
            trackedTask.Status = Enum.Parse<Domain.Enums.TaskStatus>(dto.Status);
            trackedTask.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();

            // Already flat — no fix needed here
            var newTask = new
            {
                trackedTask.Id,
                trackedTask.Title,
                trackedTask.Description,
                Status = trackedTask.Status.ToString(),
                trackedTask.DepartmentId,
                trackedTask.UpdatedAt
            };

            await auditService.LogAsync(
                "Updated",
                "TaskItem",
                trackedTask.Id,
                oldTask,
                newTask,
                trackedTask.CreatedById
            );

            return new TaskItemDto(
                trackedTask.Id,
                trackedTask.Title,
                trackedTask.Description,
                trackedTask.Status,
                trackedTask.CreatedById,
                trackedTask.DepartmentId,
                trackedTask.CreatedAt,
                trackedTask.UpdatedAt
            );
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

            // FIX: flat anonymous object — no navigation properties
            var newSubtask = new
            {
                subtask.Id,
                subtask.Title,
                subtask.Description,
                Status = subtask.Status.ToString(),
                subtask.TaskItemId,
                subtask.AssignedToId,
                subtask.CreatedById,
                subtask.UpdatedAt
            };

            await auditService.LogAsync(
                "Updated",
                "Subtask",
                subtask.Id,
                oldSubtask,
                newSubtask,
                subtask.CreatedById
            );

            return new SubtaskDto(
                subtask.Id,
                subtask.Title,
                subtask.Description,
                subtask.Status,
                subtask.TaskItemId,
                subtask.AssignedToId,
                subtask.CreatedById,
                subtask.CreatedAt,
                subtask.UpdatedAt
            );
        }

        public async Task<TaskAssignmentDto> TaskAssignAsync(int userId, int taskId)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            var task = await context.TaskItem.FirstOrDefaultAsync(t => t.Id == taskId);

            if (user is null) throw new KeyNotFoundException($"User with ID {userId} does not exist.");
            if (task is null) throw new KeyNotFoundException($"Task with ID {taskId} does not exist.");

            var userTaskCount = await context.TaskAssignment.CountAsync(a => a.UserId == userId);

            if (userTaskCount >= 2)
            {
                var usersWithNoTasks = await context.Users
                    .Where(u => u.DepartmentId == task.DepartmentId &&
                                !context.TaskAssignment.Any(a => a.UserId == u.Id))
                    .Select(u => new UsersDto
                    {
                        Id = u.Id,
                        Username = u.Username,
                        Email = u.Email,
                        Role = u.Role,
                        DepartmentId = u.DepartmentId
                    }).ToListAsync();

                throw new TaskLimitExceededException(
                    "This user already has the maximum allowed number of tasks (2).",
                    usersWithNoTasks
                );
            }

            var existingAssignment = await context.TaskAssignment.FirstOrDefaultAsync(a => a.TaskItemId == taskId);
            if (existingAssignment != null) throw new Exception("Task is already assigned.");

            var lebanonTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Beirut");
            var assignment = new TaskAssignment
            {
                TaskItemId = taskId,
                UserId = userId,
                AssignedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, lebanonTimeZone),
            };

            context.TaskAssignment.Add(assignment);
            await context.SaveChangesAsync();

            // Already flat — no fix needed
            await auditService.LogAsync(
                "Assigned",
                "TaskAssignment",
                taskId,
                null,
                new { userId, taskId },
                userId
            );

            return new TaskAssignmentDto(
                task.Id,
                new TaskItemDto(task.Id, task.Title, task.Description, task.Status, task.CreatedById, task.DepartmentId, task.CreatedAt, task.UpdatedAt),
                user.Id,
                new UsersDto { Id = user.Id, Username = user.Username, Email = user.Email, Role = user.Role, DepartmentId = user.DepartmentId },
                DateTime.UtcNow
            );
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

            // Already flat — no fix needed
            await auditService.LogAsync(
                "Unassigned",
                "TaskAssignment",
                taskId,
                new { userId, taskId },
                null,
                userId
            );

            return new TaskAssignmentDto(
                assignment.TaskItemId,
                new TaskItemDto(task.Id, task.Title, task.Description, task.Status, task.CreatedById, task.DepartmentId, task.CreatedAt, task.UpdatedAt),
                assignment.UserId,
                new UsersDto { Id = user.Id, Username = user.Username, Email = user.Email, Role = user.Role, DepartmentId = user.DepartmentId },
                assignment.AssignedAt
            );
        }

        public async Task<UserTaskStatusDto> GetUserTaskStatus(int userId)
        {
            var user = await context.Users
                .Include(u => u.TaskAssignments)
                    .ThenInclude(ta => ta.TaskItem)
                        .ThenInclude(t => t.Subtasks)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) throw new KeyNotFoundException("User not found");

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

        public async Task<TaskCommentDto> TaskCommentAsync(CreateTaskCommentDto dto)
        {
            var lebanonTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Beirut");
            var taskComment = new TaskComment
            {
                Content = dto.Content,
                AuthorId = dto.AuthorId,
                TaskItemId = dto.TaskItemId,
                CreatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, lebanonTimeZone),
                UpdatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, lebanonTimeZone),
            };

            context.TaskComment.Add(taskComment);
            await context.SaveChangesAsync();

            // FIX: flat anonymous object — no navigation properties
            await auditService.LogAsync(
                "CommentAdded",
                "TaskComment",
                taskComment.Id,
                null,
                new
                {
                    taskComment.Id,
                    taskComment.Content,
                    taskComment.AuthorId,
                    taskComment.TaskItemId,
                    taskComment.CreatedAt
                },
                dto.AuthorId
            );

            return new TaskCommentDto
            {
                Id = taskComment.Id,
                Content = taskComment.Content,
                AuthorId = taskComment.AuthorId,
                TaskItemId = taskComment.TaskItemId,
                CreatedAt = taskComment.CreatedAt,
                UpdatedAt = taskComment.UpdatedAt
            };
        }

        public async Task<SubtaskCommentDto> SubTaskCommentAsync(CreateSubtaskCommentDto dto)
        {
            var lebanonTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Beirut");
            var subtaskComment = new SubtaskComment
            {
                Content = dto.Content,
                AuthorId = dto.AuthorId,
                SubtaskId = dto.SubtaskId,
                CreatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, lebanonTimeZone),
                UpdatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, lebanonTimeZone),
            };

            context.SubtaskComment.Add(subtaskComment);
            await context.SaveChangesAsync();

            // FIX: flat anonymous object — no navigation properties
            await auditService.LogAsync(
                "CommentAdded",
                "SubtaskComment",
                subtaskComment.Id,
                null,
                new
                {
                    subtaskComment.Id,
                    subtaskComment.Content,
                    subtaskComment.AuthorId,
                    subtaskComment.SubtaskId,
                    subtaskComment.CreatedAt
                },
                dto.AuthorId
            );

            return new SubtaskCommentDto
            {
                Id = subtaskComment.Id,
                Content = subtaskComment.Content,
                AuthorId = subtaskComment.AuthorId,
                SubtaskId = subtaskComment.SubtaskId,
                CreatedAt = subtaskComment.CreatedAt,
                UpdatedAt = subtaskComment.UpdatedAt
            };
        }

        public async Task<List<TaskWithSubtasksDto>> GetOldTasksAsync()
        {
            var lebanonTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Beirut");
            var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, lebanonTimeZone);
            var twoWeeksAgo = now.AddDays(-14);

            return await context.TaskItem
                .Where(t => t.CreatedAt <= twoWeeksAgo || t.UpdatedAt <= twoWeeksAgo)
                .Select(t => new TaskWithSubtasksDto(
                    t.Id, t.Title, t.Description, t.Status, t.CreatedById, t.DepartmentId, t.CreatedAt, t.UpdatedAt,
                    t.Subtasks
                        .Where(s => s.CreatedAt <= twoWeeksAgo || s.UpdatedAt <= twoWeeksAgo)
                        .Select(s => new SubtaskDto(
                            s.Id, s.Title, s.Description, s.Status, s.TaskItemId,
                            s.AssignedToId, s.CreatedById, s.CreatedAt, s.UpdatedAt))
                        .ToList()
                )).ToListAsync();
        }

        public async Task<Users?> GetUserByEmailAsync(string email)
        {
            return await context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public Task<bool> DeleteTaskAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<TaskDashboardItemDto>> GetDashboardTasksAsync()
        {
            return await context.TaskItem
                .Include(t => t.Department)
                .Include(t => t.TaskAssignments)
                    .ThenInclude(a => a.User)
                .Select(t => new TaskDashboardItemDto(
                    t.Id,
                    t.Title,
                    t.Description,
                    t.Status,
                    t.CreatedById,
                    t.DepartmentId,
                    t.Department.Name,
                    t.TaskAssignments.FirstOrDefault() != null
                        ? t.TaskAssignments.FirstOrDefault()!.User.Username
                        : null,
                    t.CreatedAt,
                    t.UpdatedAt
                ))
                .ToListAsync();
        }
    }
}