using Final_Project_Adv.Domain.DTO;
using Final_Project_Adv.Models;

namespace Final_Project_Adv.Services
{
    public interface IManagerServices
    {
        Task<UsersDto> CreateUserAsync(CreateUserDto createUser);
        Task<TaskItemDto> CreateTaskAsync(CreateTaskItemDto dto);

        Task<SubtaskDto> CreateSubtaskAsync(CreateSubtaskDto dto);
        Task<UsersDto> UpdateUserAsync(int id, UpdateUserDto dto);

        Task<IEnumerable<UsersDto>> GetAllUsersAsync();
        Task<IEnumerable<TaskItemDto>> GetAllTasksAsync();

        Task<IEnumerable<SubtaskDto>> GetAllSubTasksAsync();

        Task<TaskItemDto> UpdateTasksAsync(int id, UpdateTaskDTO dto);

        Task<SubtaskDto> UpdateSubTasksAsync(int id, UpdateSubTaskDTO dto);

        Task<TaskAssignmentDto> TaskAssignAsync(int userId, int taskId);

        Task<TaskAssignmentDto> UnassignTaskAsync(int userId, int taskId);

        Task<UserTaskStatusDto> GetUserTaskStatus(int userId);

        Task<TaskCommentDto> TaskCommentAsync(CreateTaskCommentDto dto);
        Task<SubtaskCommentDto> SubTaskCommentAsync(CreateSubtaskCommentDto dto);

        Task<List<TaskWithSubtasksDto>> GetOldTasksAsync();
        Task<bool> DeleteTaskAsync(int id, int requestingUserId);
        Task<bool> DeleteSubTaskAsync(int id, int requestingUserId);

        Task<Users?> GetUserByEmailAsync(string email);

        Task<IEnumerable<TaskDashboardItemDto>> GetDashboardTasksAsync();
    }
}
