using Final_Project_Adv.Domain.DTO;
using Final_Project_Adv.Services;
using Microsoft.AspNetCore.Mvc;

namespace Final_Project_Adv.Controllers
{
    [Route("Employee")]
    public class EmployeeController : Controller
    {
        private readonly IEmployeeServices _employeeServices;

        public EmployeeController(IEmployeeServices employeeServices)
        {
            _employeeServices = employeeServices;
        }

        private int GetCurrentUserId()
        {
            var idStr = HttpContext.Session.GetString("UserId");
            return int.TryParse(idStr, out int id) ? id : 0;
        }

        private string? GetCurrentUserRole() =>
            HttpContext.Session.GetString("UserRole");

        [HttpGet("ViewTask")]
        public async Task<IActionResult> ViewTask()
        {
            var role = GetCurrentUserRole();
            if (role is not ("Employee" or "Manager"))
                return RedirectToAction("Login", "Account");

            var tasks = await _employeeServices.GetMyTasksAsync(GetCurrentUserId());
            return View(tasks);
        }

        [HttpPost("AcceptTask")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptTask(int taskId)
        {
            try
            {
                var result = await _employeeServices.AcceptTaskAsync(taskId);
                if (!result) TempData["Error"] = "Task not found.";
                else TempData["Success"] = "Task accepted — it is now In Progress.";
            }
            catch (InvalidOperationException ex) { TempData["Error"] = ex.Message; }
            return RedirectToAction("ViewTask");
        }

        [HttpGet("GetSubtasksForTask")]
        public async Task<IActionResult> GetSubtasksForTask(int taskItemId)
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();

            var mine = await _employeeServices.GetMySubtasksForTaskAsync(userId, taskItemId);
            var unassigned = await _employeeServices.GetUnassignedSubtasksForTaskAsync(taskItemId);

            return Json(new
            {
                mine = mine.Select(s => new {
                    s.Id,
                    s.Title,
                    s.Description,
                    Status = s.Status.ToString(),
                    s.TaskItemId,
                    s.AssignedToId
                }),
                unassigned = unassigned.Select(s => new {
                    s.Id,
                    s.Title,
                    s.Description,
                    Status = s.Status.ToString(),
                    s.TaskItemId,
                    s.AssignedToId
                })
            });
        }

        [HttpPost("AcceptSubtask")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptSubtask(int subtaskId)
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return RedirectToAction("Login", "Account");
            try
            {
                var result = await _employeeServices.AcceptSubtaskAsync(subtaskId, userId);
                if (!result) TempData["Error"] = "Subtask not found.";
                else TempData["Success"] = "Subtask accepted — it is now In Progress.";
            }
            catch (InvalidOperationException ex) { TempData["Error"] = ex.Message; }
            return RedirectToAction("ViewTask");
        }

        [HttpPost("CreateSubtask")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSubtask(
            string title, string? description, int taskItemId, int? assignedToId)
        {
            var role = GetCurrentUserRole();
            if (role is not ("Employee" or "Manager"))
                return RedirectToAction("Login", "Account");

            if (string.IsNullOrWhiteSpace(title))
            { TempData["Error"] = "Subtask title is required."; return RedirectToAction("ViewTask"); }

            if (taskItemId <= 0)
            { TempData["Error"] = "Invalid task reference."; return RedirectToAction("ViewTask"); }

            try
            {
                var dto = new CreateSubtaskDto(
                    title.Trim(),
                    description?.Trim() ?? string.Empty,
                    taskItemId,
                    GetCurrentUserId(),
                    assignedToId > 0 ? assignedToId : null);

                await _employeeServices.CreateSubtaskAsync(dto);
                TempData["Success"] = $"Subtask \"{title.Trim()}\" created successfully.";
            }
            catch (KeyNotFoundException ex) { TempData["Error"] = ex.Message; }
            catch (InvalidOperationException ex) { TempData["Error"] = ex.Message; }
            catch (Exception ex) { TempData["Error"] = $"Unexpected error: {ex.Message}"; }

            return RedirectToAction("ViewTask");
        }

        // ── POST /Employee/PostTaskComment ────────────────────────────────────

        [HttpPost("PostTaskComment")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PostTaskComment(int taskItemId, string content)
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return RedirectToAction("Login", "Account");

            if (string.IsNullOrWhiteSpace(content))
            { TempData["Error"] = "Comment cannot be empty."; return RedirectToAction("ViewTask"); }

            try
            {
                await _employeeServices.AddTaskCommentAsync(new CreateTaskCommentDto
                {
                    Content = content.Trim(),
                    AuthorId = userId,
                    TaskItemId = taskItemId
                });
                TempData["Success"] = "Comment posted.";
            }
            catch (UnauthorizedAccessException) { TempData["Error"] = "You do not have permission to add comments."; }
            catch (Exception ex) { TempData["Error"] = $"Error: {ex.Message}"; }

            return RedirectToAction("ViewTask");
        }

        // ── POST /Employee/PostSubtaskComment ─────────────────────────────────

        [HttpPost("PostSubtaskComment")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PostSubtaskComment(int subtaskId, string content)
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return RedirectToAction("Login", "Account");

            if (string.IsNullOrWhiteSpace(content))
            { TempData["Error"] = "Comment cannot be empty."; return RedirectToAction("ViewTask"); }

            try
            {
                await _employeeServices.AddSubtaskCommentAsync(new CreateSubtaskCommentDto
                {
                    Content = content.Trim(),
                    AuthorId = userId,
                    SubtaskId = subtaskId
                });
                TempData["Success"] = "Comment posted.";
            }
            catch (UnauthorizedAccessException) { TempData["Error"] = "You do not have permission to add comments."; }
            catch (Exception ex) { TempData["Error"] = $"Error: {ex.Message}"; }

            return RedirectToAction("ViewTask");
        }
    }
}