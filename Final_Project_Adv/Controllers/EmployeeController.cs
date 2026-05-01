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

        [HttpGet("ViewTask")]
        public async Task<IActionResult> ViewTask()
        {
            int userId = GetCurrentUserId();
            var tasks = await _employeeServices.GetMyTasksAsync(userId);
            return View(tasks);
        }

        [HttpPost("AcceptTask")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptTask()
        {
            var taskIdStr = Request.Form["TaskId"].ToString();

            if (!int.TryParse(taskIdStr, out int taskId) || taskId <= 0)
            {
                TempData["Error"] = "Invalid task ID.";
                return RedirectToAction("ViewTask");
            }

            var success = await _employeeServices.AcceptTaskAsync(taskId);

            if (success)
                TempData["Success"] = "Task accepted and marked as In Progress.";
            else
                TempData["Error"] = "Task not found or could not be updated.";

            return RedirectToAction("ViewTask");
        }
    }
}