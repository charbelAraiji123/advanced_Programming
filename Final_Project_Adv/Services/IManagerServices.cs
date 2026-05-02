using Final_Project_Adv.Domain.DTO;
using Final_Project_Adv.Models;

namespace Final_Project_Adv.Services
{
    public interface IManagerServices
    {
        // User
        Task<UsersDto> CreateUserAsync(CreateUserDto dto);
        Task<UsersDto> UpdateUserAsync(int id, UpdateUserDto dto);
        Task<IEnumerable<UsersDto>> GetAllUsersAsync();
        Task<Users?> GetUserByEmailAsync(string email);

        // Task
        Task<TaskItemDto> CreateTaskAsync(CreateTaskItemDto dto);
        Task<bool> DeleteTaskAsync(int id, int requestingUserId);
        Task<TaskItemDto> UpdateTasksAsync(int id, UpdateTaskDTO dto);
        Task<IEnumerable<TaskItemDto>> GetAllTasksAsync();

        // Subtask
        Task<SubtaskDto> CreateSubtaskAsync(CreateSubtaskDto dto);
        Task<bool> DeleteSubTaskAsync(int id, int requestingUserId);
        Task<SubtaskDto> UpdateSubTasksAsync(int id, UpdateSubTaskDTO dto);
        Task<IEnumerable<SubtaskDto>> GetAllSubTasksAsync();

        // Assignment
        Task<TaskAssignmentDto> TaskAssignAsync(int userId, int taskId);
        Task<TaskAssignmentDto> UnassignTaskAsync(int userId, int taskId);

        // Status & progress
        Task<UserTaskStatusDto> GetUserTaskStatus(int userId);
        Task<List<TaskWithSubtasksDto>> GetOldTasksAsync();

        // Comments
        Task<TaskCommentDto> TaskCommentAsync(CreateTaskCommentDto dto);
        Task<SubtaskCommentDto> SubTaskCommentAsync(CreateSubtaskCommentDto dto);

        // Dashboard
        Task<IEnumerable<TaskDashboardItemDto>> GetDashboardTasksAsync();
    }
}