using Final_Project_Adv.Domain.DTO;
using Final_Project_Adv.Services;
using Microsoft.AspNetCore.Mvc;
using Final_Project_Adv.Models;
using System.Security.Claims;

namespace Final_Project_Adv.Controllers
{
    [Route("api/[controller]")]
    public class AdminController : Controller
    {
        private readonly IAdminServices _adminServices;

        public AdminController(IAdminServices adminServices)
        {
            _adminServices = adminServices;
        }

        /// <summary>
        /// Helper to extract the ID of the Admin performing the action.
        /// Priority: 1. Claims (Auth) -> 2. Session -> 3. Default 0
        /// </summary>
        private int GetCurrentUserId()
        {
            // 1. Try to get ID from Identity Claims (Recommended)
            var claimId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(claimId, out int idFromClaim)) return idFromClaim;

            // 2. Fallback to Session if Claims aren't populated
            var sessionId = HttpContext.Session.GetString("UserId");
            if (int.TryParse(sessionId, out int idFromSession)) return idFromSession;

            return 0; // Action performed by unknown/system
        }

        // --- User Management ---

        [HttpPost("Users/Create")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _adminServices.CreateUserAsync(dto, GetCurrentUserId());
            return Ok(result);
        }

        [HttpDelete("Users/Delete/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            await _adminServices.DeleteUserAsync(id, GetCurrentUserId());
            return NoContent();
        }

        [HttpPut("Users/Update")]
        public async Task<IActionResult> UpdateUser([FromBody] UsersDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            await _adminServices.UpdateUserAsync(dto, GetCurrentUserId());
            return Ok(new { message = "User updated successfully" });
        }

        // --- Department Management ---

        [HttpPost("Departments/Create")]
        public async Task<IActionResult> CreateDept([FromBody] CreateDepartmentDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _adminServices.CreateDeptAsync(dto, GetCurrentUserId());
            return Ok(result);
        }

        [HttpDelete("Departments/Delete/{id}")]
        public async Task<IActionResult> DeleteDept(int id)
        {
            await _adminServices.DeleteDeptAsync(id, GetCurrentUserId());
            return NoContent();
        }

        // --- Permission Management ---

        [HttpPost("Permissions/Grant")]
        public async Task<IActionResult> GrantPermission([FromBody] GrantPermissionDto dto)
        {
            var result = await _adminServices.GrantPermissionToUserAsync(dto);
            return Ok(result);
        }

        [HttpPost("Permissions/Revoke")]
        public async Task<IActionResult> RevokePermission([FromBody] RevokePermissionDto dto)
        {
            var result = await _adminServices.RevokePermissionFromUserAsync(dto);
            return Ok(result);
        }

        [HttpGet("Permissions/{userId}")]
        public async Task<IActionResult> GetPermissions(int userId)
        {
            var result = await _adminServices.GetUserPermissionsAsync(userId);
            return Ok(result);
        }

        [HttpGet("tasks")]
        public async Task<ActionResult<IEnumerable<TaskItemDto>>> GetAllTasks()
        {
            var tasks = await _adminServices.ViewAllTasksAsync();
            return Ok(tasks);
        }

        [HttpGet("tasks/department/{deptId}")]
        public async Task<ActionResult<IEnumerable<TaskItemDto>>> GetTasksByDepartment(int deptId)
        {
            var tasks = await _adminServices.ViewAllTasksPerDeptAsync(deptId);
            return Ok(tasks);
        }


    }
}