using Final_Project_Adv.Domain.DTO;
using Final_Project_Adv.Domain.Enums;
using Final_Project_Adv.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Final_Project_Adv.Services
{
    public class ProgressService
    {
        private readonly AppDbContext _context;

        public ProgressService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Returns progress data for every user who has at least one task or subtask assigned.
        /// </summary>
        public async Task<List<UserProgressDto>> GetAllUsersProgressAsync()
        {
            // Load all users with their assigned tasks and subtasks in one query
            var users = await _context.Users
                .Include(u => u.Department)
                .Include(u => u.TaskAssignments)
                    .ThenInclude(ta => ta.TaskItem)
                        .ThenInclude(t => t.Subtasks)
                .Include(u => u.AssignedSubtasks)
                .Where(u => u.Role == "Employee" || u.Role == "Manager")
                .OrderBy(u => u.Department.Name)
                .ThenBy(u => u.Username)
                .ToListAsync();

            var result = new List<UserProgressDto>();

            foreach (var user in users)
            {
                var tasks = user.TaskAssignments.Select(ta => ta.TaskItem).ToList();
                var subtasks = user.AssignedSubtasks.ToList();

                var dto = new UserProgressDto
                {
                    UserId = user.Id,
                    Username = user.Username,
                    Role = user.Role,
                    DepartmentName = user.Department?.Name ?? "—",

                    TotalTasks = tasks.Count,
                    PendingTasks = tasks.Count(t => t.Status == Domain.Enums.TaskStatus.Pending),
                    InProgressTasks = tasks.Count(t => t.Status == Domain.Enums.TaskStatus.InProgress),
                    CompletedTasks = tasks.Count(t => t.Status == Domain.Enums.TaskStatus.Completed),

                    TotalSubtasks = subtasks.Count,
                    PendingSubtasks = subtasks.Count(s => s.Status == Domain.Enums.TaskStatus.Pending),
                    InProgressSubtasks = subtasks.Count(s => s.Status == Domain.Enums.TaskStatus.InProgress),
                    CompletedSubtasks = subtasks.Count(s => s.Status == Domain.Enums.TaskStatus.Completed),

                    Tasks = tasks.Select(t => new TaskProgressDto
                    {
                        TaskId = t.Id,
                        Title = t.Title,
                        Status = t.Status.ToString(),

                        TotalSubtasks = t.Subtasks.Count,
                        CompletedSubtasks = t.Subtasks.Count(s => s.Status == Domain.Enums.TaskStatus.Completed),

                        Subtasks = t.Subtasks.Select(s => new SubtaskProgressDto
                        {
                            SubtaskId = s.Id,
                            Title = s.Title,
                            Status = s.Status.ToString()
                        }).ToList()
                    }).ToList()
                };

                result.Add(dto);
            }

            return result;
        }

        /// <summary>
        /// Returns progress data for a single user.
        /// </summary>
        public async Task<UserProgressDto?> GetUserProgressAsync(int userId)
        {
            var all = await GetAllUsersProgressAsync();
            return all.FirstOrDefault(u => u.UserId == userId);
        }
    }
}