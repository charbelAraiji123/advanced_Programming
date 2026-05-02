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

        // ── Helpers ───────────────────────────────────────────────────────────

        private int GetCurrentUserId()
        {
            var idStr = HttpContext.Session.GetString("UserId");
            return int.TryParse(idStr, out int id) ? id : 0;
        }

        private string? GetCurrentUserRole() =>
            HttpContext.Session.GetString("UserRole");

        // ── GET /Employee/ViewTask ────────────────────────────────────────────

        [HttpGet("ViewTask")]
        public async Task<IActionResult> ViewTask()
        {
            var role = GetCurrentUserRole();
            if (role is not ("Employee" or "Manager"))
                return RedirectToAction("Login", "Account");

            var tasks = await _employeeServices.GetMyTasksAsync(GetCurrentUserId());
            return View(tasks);
        }

        // ── POST /Employee/AcceptTask ─────────────────────────────────────────

        [HttpPost("AcceptTask")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptTask(int taskId)
        {
            try
            {
                var result = await _employeeServices.AcceptTaskAsync(taskId);
                if (!result)
                    TempData["Error"] = "Task not found.";
                else
                    TempData["Success"] = "Task accepted — it is now In Progress.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction("ViewTask");
        }

        // ── POST /Employee/CreateSubtask ──────────────────────────────────────

        [HttpPost("CreateSubtask")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSubtask(
            string title,
            string? description,
            int taskItemId,
            int? assignedToId)
        {
            var role = GetCurrentUserRole();
            if (role is not ("Employee" or "Manager"))
                return RedirectToAction("Login", "Account");

            if (string.IsNullOrWhiteSpace(title))
            {
                TempData["Error"] = "Subtask title is required.";
                return RedirectToAction("ViewTask");
            }

            if (taskItemId <= 0)
            {
                TempData["Error"] = "Invalid task reference.";
                return RedirectToAction("ViewTask");
            }

            try
            {
                var dto = new CreateSubtaskDto(
                    title.Trim(),
                    description?.Trim() ?? string.Empty,
                    taskItemId,
                    GetCurrentUserId(),
                    assignedToId > 0 ? assignedToId : null
                );

                await _employeeServices.CreateSubtaskAsync(dto);

                TempData["Success"] = $"Subtask \"{title.Trim()}\" created successfully.";
            }
            catch (KeyNotFoundException ex)
            {
                TempData["Error"] = ex.Message;
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Unexpected error: {ex.Message}";
            }

            return RedirectToAction("ViewTask");
        }
    }
}