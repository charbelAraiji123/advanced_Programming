using Final_Project_Adv.Services;
using Microsoft.AspNetCore.Mvc;

namespace Final_Project_Adv.Controllers
{
    [Route("Manager/Progress")]
    public class ProgressController : Controller
    {
        private readonly ProgressService _progressService;

        public ProgressController(ProgressService progressService)
        {
            _progressService = progressService;
        }

        private string? GetCurrentUserRole() =>
            HttpContext.Session.GetString("UserRole");

        // ── GET /Manager/Progress ─────────────────────────────────────────────
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            if (GetCurrentUserRole() != "Manager")
                return RedirectToAction("Login", "Account");

            var progress = await _progressService.GetAllUsersProgressAsync();
            return View("~/Views/Manager/Progress.cshtml", progress);
        }

        // ── GET /Manager/Progress/User/{id}  (returns JSON for the modal) ─────
        [HttpGet("User/{id:int}")]
        public async Task<IActionResult> UserDetail(int id)
        {
            if (GetCurrentUserRole() != "Manager")
                return Forbid();

            var data = await _progressService.GetUserProgressAsync(id);
            if (data == null)
                return NotFound(new { message = $"User {id} not found." });

            return Ok(data);
        }
    }
}