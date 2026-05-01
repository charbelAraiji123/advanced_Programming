using Final_Project_Adv.Domain.DTO;
using Final_Project_Adv.Services;
using Microsoft.AspNetCore.Mvc;
using Final_Project_Adv.Models; // Added for UserRoles

namespace Final_Project_Adv.Controllers
{
    [Route("api/[controller]")]
    public class AdminController(IAdminServices adminServices) : Controller
    {
        [HttpGet("Panel")]
        //public IActionResult AdminPanel()
        //{
        //    var role = HttpContext.Session.GetString("UserRole");

        //    if (!string.Equals(role, UserRoles.Admin, StringComparison.OrdinalIgnoreCase))
        //    {
        //        return RedirectToAction("Login", "Account");
        //    }

        //    return View();
        //}
        // --- User Management ---

        [HttpPost("Users/Create")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
        {
            var result = await adminServices.CreateUserAsync(dto);
            return Ok(result);
        }

        [HttpDelete("Users/Delete/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            await adminServices.DeleteUserAsync(id);
            return NoContent();
        }

        [HttpPut("Users/Update")]
        public async Task<IActionResult> UpdateUser([FromBody] UsersDto dto)
        {
            await adminServices.UpdateUserAsync(dto);
            return Ok(new { message = "User updated successfully" });
        }

        // --- Department Management ---

        [HttpPost("Departments/Create")]
        public async Task<IActionResult> CreateDept([FromBody] CreateDepartmentDto dto)
        {
            var result = await adminServices.CreateDeptAsync(dto);
            return Ok(result);
        }

        [HttpDelete("Departments/Delete/{id}")]
        public async Task<IActionResult> DeleteDept(int id)
        {
            await adminServices.DeleteDeptAsync(id);
            return NoContent();
        }

        // --- Permission Management (The Admin's control over Managers) ---

        [HttpPost("Permissions/Grant")]
        public async Task<IActionResult> GrantPermission([FromBody] GrantPermissionDto dto)
        {
            var result = await adminServices.GrantPermissionToUserAsync(dto);
            return Ok(result);
        }

        [HttpPost("Permissions/Revoke")]
        public async Task<IActionResult> RevokePermission([FromBody] RevokePermissionDto dto)
        {
            var result = await adminServices.RevokePermissionFromUserAsync(dto);
            return Ok(result);
        }

        [HttpGet("Permissions/{userId}")]
        public async Task<IActionResult> GetPermissions(int userId)
        {
            var result = await adminServices.GetUserPermissionsAsync(userId);
            return Ok(result);
        }
    }
}