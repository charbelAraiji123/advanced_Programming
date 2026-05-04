using Final_Project_Adv.Domain.DTO;
using Final_Project_Adv.Domain.Enums;
using Final_Project_Adv.Models;
using Final_Project_Adv.Services;
using Final_Project_Adv.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Final_Project_Adv.Controllers
{
    [Route("Admin")]
    public class AdminViewController : Controller
    {
        private readonly IAdminServices _adminServices;
        private readonly AppDbContext _context;
        private readonly AuditService _auditService;

        public AdminViewController(IAdminServices adminServices, AppDbContext context, AuditService auditService)
        {
            _adminServices = adminServices;
            _context = context;
            _auditService = auditService;
        }

        private bool IsAdmin()
        {
            var role = HttpContext.Session.GetString("UserRole");
            return string.Equals(role, UserRoles.Admin, StringComparison.OrdinalIgnoreCase);
        }

        private int GetAdminId()
        {
            var idStr = HttpContext.Session.GetString("UserId");
            return int.TryParse(idStr, out int id) ? id : 0;
        }

        private async Task LoadDepartmentsAsync()
        {
            var departments = await _context.Department
                .Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.Name })
                .ToListAsync();
            ViewBag.Departments = departments;
        }

        // ─────────────────────────────────────────────────────────────────────
        // PANEL & USER MANAGEMENT
        // ─────────────────────────────────────────────────────────────────────

        [HttpGet("Panel")]
        public IActionResult AdminPanel()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            return View();
        }

        [HttpGet("UserManagement")]
        public IActionResult UserManagement()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            return View();
        }

        [HttpGet("GetUsersList")]
        public async Task<IActionResult> GetUsersList()
        {
            if (!IsAdmin()) return Unauthorized();
            var users = await _context.Users
                .Include(u => u.Department)           // ← add this
                .Select(u => new {
                    u.Id,
                    u.Username,
                    u.Email,
                    u.Role,
                    u.DepartmentId,
                    DepartmentName = u.Department.Name  // ← and this
                })
                .ToListAsync();
            return Ok(users);
        }

        // ─────────────────────────────────────────────────────────────────────
        // PERMISSIONS
        // ─────────────────────────────────────────────────────────────────────

        [HttpGet("Permissions")]
        public async Task<IActionResult> Permissions(string? search, string? filterRole)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            // Load all non-admin users with their current permissions
            var query = _context.Users
                .Include(u => u.Permissions)
                .Where(u => u.Role != UserRoles.Admin)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(u => u.Username.Contains(search) || u.Email.Contains(search));

            if (!string.IsNullOrWhiteSpace(filterRole))
                query = query.Where(u => u.Role == filterRole);

            var users = await query
                .OrderBy(u => u.Role)
                .ThenBy(u => u.Username)
                .Select(u => new UserPermissionsViewModel
                {
                    UserId = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    Role = u.Role,
                    Permissions = u.Permissions.Select(p => p.Permission).ToList()
                })
                .ToListAsync();

            ViewBag.AllPermissions = Enum.GetValues<PermissionType>().ToList();
            ViewBag.Search = search;
            ViewBag.FilterRole = filterRole;

            return View(users);
        }

        // Called via AJAX — grants a single permission
        [HttpPost("Permissions/Grant")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GrantPermission([FromBody] GrantPermissionDto dto)
        {
            if (!IsAdmin()) return Unauthorized();
            dto.GrantedById = GetAdminId();

            try
            {
                var result = await _adminServices.GrantPermissionToUserAsync(dto);
                await _auditService.LogAsync(
                    "GrantedPermission", "UserPermission", dto.UserId,
                    null, new { dto.UserId, Permission = dto.Permission.ToString() },
                    GetAdminId());
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // Called via AJAX — revokes a single permission
        [HttpPost("Permissions/Revoke")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RevokePermission([FromBody] RevokePermissionDto dto)
        {
            if (!IsAdmin()) return Unauthorized();

            try
            {
                var result = await _adminServices.RevokePermissionFromUserAsync(dto);
                await _auditService.LogAsync(
                    "RevokedPermission", "UserPermission", dto.UserId,
                    new { dto.UserId, Permission = dto.Permission.ToString() }, null,
                    GetAdminId());
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // Called via AJAX — bulk-sets all permissions for a user (replaces current set)
        [HttpPost("Permissions/SetAll")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetAllPermissions([FromBody] SetAllPermissionsDto dto)
        {
            if (!IsAdmin()) return Unauthorized();

            try
            {
                int adminId = GetAdminId();

                // Get current permissions
                var current = await _context.UserPermission
                    .Where(p => p.UserId == dto.UserId)
                    .Select(p => p.Permission)
                    .ToListAsync();

                var desired = dto.Permissions ?? new List<PermissionType>();

                // Grant new ones
                foreach (var perm in desired.Except(current))
                {
                    await _adminServices.GrantPermissionToUserAsync(new GrantPermissionDto
                    {
                        UserId = dto.UserId,
                        GrantedById = adminId,
                        Permission = perm
                    });
                }

                // Revoke removed ones
                foreach (var perm in current.Except(desired))
                {
                    await _adminServices.RevokePermissionFromUserAsync(new RevokePermissionDto
                    {
                        UserId = dto.UserId,
                        Permission = perm
                    });
                }

                await _auditService.LogAsync(
                    "BulkSetPermissions", "UserPermission", dto.UserId,
                    new { Removed = current.Except(desired).Select(p => p.ToString()) },
                    new { Added = desired.Except(current).Select(p => p.ToString()) },
                    adminId);

                var updated = await _adminServices.GetUserPermissionsAsync(dto.UserId);
                return Ok(updated);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // AUDIT LOGS
        // ─────────────────────────────────────────────────────────────────────

        [HttpGet("AuditLogs")]
        public async Task<IActionResult> AuditLogs(string? search, string? filterAction, string? filterEntity, int page = 1)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            const int pageSize = 20;
            var query = _context.AuditLog.Include(a => a.PerformedBy).AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(a =>
                    a.EntityType.Contains(search) ||
                    a.Action.Contains(search) ||
                    a.PerformedBy.Username.Contains(search));

            if (!string.IsNullOrWhiteSpace(filterAction))
                query = query.Where(a => a.Action == filterAction);

            if (!string.IsNullOrWhiteSpace(filterEntity))
                query = query.Where(a => a.EntityType == filterEntity);

            var totalCount = await query.CountAsync();

            var logs = await query
                .OrderByDescending(a => a.PerformedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new AuditLogDto
                {
                    Id = a.Id,
                    Action = a.Action,
                    EntityType = a.EntityType,
                    EntityId = a.EntityId,
                    OldValue = a.OldValue,
                    NewValue = a.NewValue,
                    PerformedById = a.PerformedById,
                    PerformedBy = a.PerformedBy.Username,
                    PerformedAt = a.PerformedAt
                })
                .ToListAsync();

            ViewBag.Actions = await _context.AuditLog.Select(a => a.Action).Distinct().ToListAsync();
            ViewBag.Entities = await _context.AuditLog.Select(a => a.EntityType).Distinct().ToListAsync();
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            ViewBag.TotalCount = totalCount;
            ViewBag.Search = search;
            ViewBag.FilterAction = filterAction;
            ViewBag.FilterEntity = filterEntity;

            return View(logs);
        }

        // ─────────────────────────────────────────────────────────────────────
        // CREATE USER
        // ─────────────────────────────────────────────────────────────────────

        [HttpGet("CreateUser")]
        public async Task<IActionResult> CreateUser()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            await LoadDepartmentsAsync();
            return View(new CreateUserDto());
        }

        [HttpPost("CreateUser")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUserPost()
        {
            var username = Request.Form["Username"].ToString();
            var email = Request.Form["Email"].ToString();
            var password = Request.Form["Password"].ToString();
            var role = Request.Form["Role"].ToString();
            var deptIdStr = Request.Form["DepartmentId"].ToString();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) ||
                string.IsNullOrEmpty(password) || string.IsNullOrEmpty(role) ||
                string.IsNullOrEmpty(deptIdStr))
            {
                ModelState.AddModelError("", "All fields are required.");
                await LoadDepartmentsAsync();
                return View("CreateUser", new CreateUserDto());
            }

            if (!int.TryParse(deptIdStr, out int deptId))
            {
                ModelState.AddModelError("", "Invalid department.");
                await LoadDepartmentsAsync();
                return View("CreateUser", new CreateUserDto());
            }

            try
            {
                var dto = new CreateUserDto
                {
                    Username = username,
                    Email = email,
                    Password = password,
                    Role = role,
                    DepartmentId = deptId
                };
                await _adminServices.CreateUserAsync(dto, GetAdminId());
                TempData["Success"] = $"User '{username}' created successfully.";
                return RedirectToAction("UserManagement");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error: {ex.Message}");
                if (ex.InnerException != null)
                    ModelState.AddModelError("", $"Detail: {ex.InnerException.Message}");
                await LoadDepartmentsAsync();
                return View("CreateUser", new CreateUserDto());
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // UPDATE USER
        // ─────────────────────────────────────────────────────────────────────

        [HttpGet("GetUser")]
        public async Task<IActionResult> GetUser(int id)
        {
            if (!IsAdmin()) return Unauthorized(new { message = "Access denied." });
            if (id <= 0) return BadRequest(new { message = "ID must be a positive integer." });

            var user = await _context.Users
                .Where(u => u.Id == id)
                .Select(u => new { u.Id, u.Username, u.Email, u.Role, u.DepartmentId })
                .FirstOrDefaultAsync();

            if (user == null) return NotFound(new { message = $"No user found with ID {id}." });
            return Ok(user);
        }

        [HttpGet("UpdateUser")]
        public async Task<IActionResult> UpdateUser()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            await LoadDepartmentsAsync();
            return View();
        }

        [HttpPost("UpdateUser")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUserPost()
        {
            await LoadDepartmentsAsync();

            var idStr = Request.Form["Id"].ToString();
            var username = Request.Form["Username"].ToString().Trim();
            var email = Request.Form["Email"].ToString().Trim();
            var role = Request.Form["Role"].ToString();
            var deptIdStr = Request.Form["DepartmentId"].ToString();

            IActionResult ReturnWithErrors()
            {
                ViewBag.HasFormErrors = true;
                ViewBag.PostedId = idStr;
                ViewBag.PostedUser = username;
                ViewBag.PostedEmail = email;
                ViewBag.PostedRole = role;
                ViewBag.PostedDept = deptIdStr;
                return View("UpdateUser");
            }

            if (!int.TryParse(idStr, out int userId) || userId <= 0)
            { ModelState.AddModelError("", "Invalid or missing user ID."); return ReturnWithErrors(); }

            if (string.IsNullOrWhiteSpace(username) || username.Length < 3)
            { ModelState.AddModelError("", "Username must be at least 3 characters."); return ReturnWithErrors(); }

            var emailValidator = new System.ComponentModel.DataAnnotations.EmailAddressAttribute();
            if (string.IsNullOrWhiteSpace(email) || !emailValidator.IsValid(email))
            { ModelState.AddModelError("", "A valid email is required."); return ReturnWithErrors(); }

            if (string.IsNullOrWhiteSpace(role))
            { ModelState.AddModelError("", "Role is required."); return ReturnWithErrors(); }

            if (!int.TryParse(deptIdStr, out int deptId) || deptId <= 0)
            { ModelState.AddModelError("", "Please select a valid department."); return ReturnWithErrors(); }

            try
            {
                if (!await _context.Users.AnyAsync(u => u.Id == userId))
                { ModelState.AddModelError("", $"User with ID {userId} does not exist."); return ReturnWithErrors(); }

                if (await _context.Users.AnyAsync(u => u.Username == username && u.Id != userId))
                { ModelState.AddModelError("", $"Username '{username}' is already taken."); return ReturnWithErrors(); }

                if (!await _context.Department.AnyAsync(d => d.Id == deptId))
                { ModelState.AddModelError("", $"Department with ID {deptId} does not exist."); return ReturnWithErrors(); }

                await _adminServices.UpdateUserAsync(new UsersDto
                {
                    Id = userId,
                    Username = username,
                    Email = email,
                    Role = role,
                    DepartmentId = deptId
                }, GetAdminId());

                TempData["Success"] = $"User '{username}' updated successfully.";
                return RedirectToAction("UserManagement");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Unexpected error: {ex.Message}");
                return ReturnWithErrors();
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // DELETE USER
        // ─────────────────────────────────────────────────────────────────────

        [HttpGet("DeleteManagement")]
        public IActionResult DeleteUser()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            return View();
        }

        [HttpPost("DeleteUser")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUserPost()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var idStr = Request.Form["Id"].ToString();
            if (!int.TryParse(idStr, out int userId) || userId <= 0)
            { TempData["Error"] = "Invalid user ID."; return RedirectToAction("DeleteUser"); }

            if (!await _context.Users.AnyAsync(u => u.Id == userId))
            { TempData["Error"] = $"User with ID {userId} does not exist."; return RedirectToAction("DeleteUser"); }

            await _adminServices.DeleteUserAsync(userId, GetAdminId());
            TempData["Success"] = $"User with ID {userId} deleted successfully.";
            return RedirectToAction("UserManagement");
        }

        // ─────────────────────────────────────────────────────────────────────
        // DEBUG HELPERS (remove before production)
        // ─────────────────────────────────────────────────────────────────────

        [HttpGet("TestSave")]
        public async Task<IActionResult> TestSave()
        {
            try
            {
                var user = new Users
                {
                    Username = $"TestUser_{DateTime.Now.Ticks}",
                    Password = BCrypt.Net.BCrypt.HashPassword("password123"),
                    Email = "test@test.com",
                    Role = "Employee",
                    DepartmentId = 1,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.Users.Add(user);
                int rows = await _context.SaveChangesAsync();
                return Ok($"Success! Rows: {rows}, New User ID: {user.Id}");
            }
            catch (Exception ex)
            {
                return Ok($"ERROR: {ex.Message} | INNER: {ex.InnerException?.Message}");
            }
        }

        [HttpPost("TestFormSave")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> TestFormSave()
        {
            var username = Request.Form["Username"].ToString();
            var email = Request.Form["Email"].ToString();
            var password = Request.Form["Password"].ToString();
            var role = Request.Form["Role"].ToString();
            var deptId = int.Parse(Request.Form["DepartmentId"].ToString());
            try
            {
                var user = new Users
                {
                    Username = username,
                    Password = BCrypt.Net.BCrypt.HashPassword(password),
                    Email = email,
                    Role = role,
                    DepartmentId = deptId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.Users.Add(user);
                int rows = await _context.SaveChangesAsync();
                return Ok($"Success! Rows: {rows}, ID: {user.Id}, Username: {user.Username}");
            }
            catch (Exception ex)
            {
                return Ok($"ERROR: {ex.Message} | INNER: {ex.InnerException?.Message}");
            }
        }
    }
}