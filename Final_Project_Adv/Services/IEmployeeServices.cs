using Final_Project_Adv.Domain.DTO;

namespace Final_Project_Adv.Services
{
    public interface IEmployeeServices
    {
        // ── Read ──────────────────────────────────────────────────────────────
        Task<IEnumerable<TaskItemDto>> GetMyTasksAsync(int userId);
        Task<IEnumerable<SubtaskDto>> GetMySubtasksAsync(int userId);
        Task<IEnumerable<SubtaskDto>> GetMySubtasksForTaskAsync(int userId, int taskItemId);
        Task<IEnumerable<SubtaskDto>> GetUnassignedSubtasksForTaskAsync(int taskItemId);

        // ── Task lifecycle ────────────────────────────────────────────────────
        Task<bool> AcceptTaskAsync(int taskId);
        Task<bool> SubmitTaskAsync(int taskId);

        // ── Subtask lifecycle ─────────────────────────────────────────────────
        Task<bool> SubmitSubtaskAsync(int subtaskId);
        Task<SubtaskDto> CreateSubtaskAsync(CreateSubtaskDto dto);
        Task<bool> AcceptSubtaskAsync(int subtaskId, int userId);

        // ── Requests ──────────────────────────────────────────────────────────
        Task<bool> RequestTaskAssignmentAsync(int userId, int taskId);

        // ── Comments ──────────────────────────────────────────────────────────
        Task<TaskCommentDto> AddTaskCommentAsync(CreateTaskCommentDto dto);
        Task<SubtaskCommentDto> AddSubtaskCommentAsync(CreateSubtaskCommentDto dto);
    }
}