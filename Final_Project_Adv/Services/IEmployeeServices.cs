using Final_Project_Adv.Domain.DTO;

namespace Final_Project_Adv.Services
{
    public interface IEmployeeServices
    {
        Task<IEnumerable<TaskItemDto>> GetMyTasksAsync(int userId);
        Task<IEnumerable<SubtaskDto>> GetMySubtasksAsync(int userId);
        Task<bool> AcceptTaskAsync(int taskId);
        Task<bool> SubmitTaskAsync(int taskId);
        Task<bool> SubmitSubtaskAsync(int subtaskId);
        Task<bool> RequestTaskAssignmentAsync(int userId, int taskId);
        Task<TaskCommentDto> AddTaskCommentAsync(CreateTaskCommentDto dto);
        Task<SubtaskCommentDto> AddSubtaskCommentAsync(CreateSubtaskCommentDto dto);
    }
}