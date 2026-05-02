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
    [Route("Manager")]
    public class ManagerController : Controller
    {
        private readonly IManagerServices _managerServices;
        private readonly AppDbContext _context;
        private readonly AuditService _auditService;
        private readonly PermissionService _permissionService;

        public ManagerController(
            IManagerServices managerServices,
            AppDbContext context,
            AuditService auditService,
            PermissionService permissionService)
        {
            _managerServices = managerServices;
            _context = context;
            _auditService = auditService;
            _permissionService = permissionService;
        }

        private int GetCurrentUserId()
        {
            var idStr = HttpContext.Session.GetString("UserId");
            return int.TryParse(idStr, out int id) ? id : 0;
        }

        private async Task LoadDepartmentsAsync()
        {
            ViewBag.Departments = await _context.Department
                .Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.Name })
                .ToListAsync();
        }

        // ─────────────────────────────────────────────────────────────────────
        // DASHBOARD
        // ─────────────────────────────────────────────────────────────────────

        [HttpGet("Dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            var tasks = await _managerServices.GetDashboardTasksAsync();
            var vm = new TaskDashboardVm { Tasks = tasks.ToList() };
            return View(vm);
        }

        // ─────────────────────────────────────────────────────────────────────
        // GET SUBTASKS FOR A TASK  (JSON — used by the drawer)
        // ─────────────────────────────────────────────────────────────────────

        [HttpGet("GetSubtasks")]
        public async Task<IActionResult> GetSubtasks(int taskId)
        {
            if (taskId <= 0)
                return BadRequest(new { message = "taskId must be a positive integer." });

            var taskExists = await _context.TaskItem.AnyAsync(t => t.Id == taskId);
            if (!taskExists)
                return NotFound(new { message = $"No task found with ID {taskId}." });

            var subtasks = await _context.Subtask
                .Where(s => s.TaskItemId == taskId)
                .OrderBy(s => s.CreatedAt)
                .Select(s => new
                {
                    s.Id,
                    s.Title,
                    s.Description,
                    Status = s.Status.ToString(),
                    s.TaskItemId,
                    s.AssignedToId,
                    AssignedToName = s.AssignedTo != null ? s.AssignedTo.Username : null,
                    s.CreatedById,
                    s.CreatedAt,
                    s.UpdatedAt
                })
                .ToListAsync();

            return Ok(subtasks);
        }

        // ─────────────────────────────────────────────────────────────────────
        // CREATE TASK
        // ─────────────────────────────────────────────────────────────────────

        [HttpGet("CreateTask")]
        public async Task<IActionResult> CreateTask()
        {
            await LoadDepartmentsAsync();
            return View();
        }

        [HttpGet("GetEmployeesByDept")]
        public async Task<IActionResult> GetEmployeesByDept(int deptId)
        {
            var employees = await _context.Users
                .Where(u => u.DepartmentId == deptId &&
                            (u.Role == "Employee" || u.Role == "Manager"))
                .Select(u => new
                {
                    u.Id,
                    u.Username,
                    u.Role,
                    taskCount = _context.TaskAssignment.Count(a => a.UserId == u.Id)
                })
                .ToListAsync();

            return Ok(employees);
        }

        [HttpPost("CreateTask")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTask(
            string title,
            string description,
            int departmentId,
            int assignedUserId)
        {
            int managerId = GetCurrentUserId();

            bool canCreate = await _permissionService.HasPermissionAsync(managerId, PermissionType.CreateTask);
            bool canAssign = await _permissionService.HasPermissionAsync(managerId, PermissionType.AssignTask);

            if (!canCreate || !canAssign)
            {
                ModelState.AddModelError("", "You do not have permission to create or assign tasks.");
                await LoadDepartmentsAsync();
                return View();
            }

            if (string.IsNullOrWhiteSpace(title))
            {
                ModelState.AddModelError("", "Task title is required.");
                await LoadDepartmentsAsync();
                return View();
            }

            if (departmentId <= 0)
            {
                ModelState.AddModelError("", "Please select a department.");
                await LoadDepartmentsAsync();
                return View();
            }

            if (assignedUserId <= 0)
            {
                ModelState.AddModelError("", "Please select an employee to assign.");
                await LoadDepartmentsAsync();
                return View();
            }

            try
            {
                var taskDto = await _managerServices.CreateTaskAsync(new CreateTaskItemDto
                {
                    Title = title.Trim(),
                    Description = description?.Trim() ?? string.Empty,
                    DepartmentId = departmentId,
                    CreatedById = managerId
                });

                await _managerServices.TaskAssignAsync(assignedUserId, taskDto.Id);

                await _auditService.LogAsync(
                    "CreatedAndAssigned", "TaskItem", taskDto.Id,
                    null,
                    new { taskDto.Id, taskDto.Title, AssignedTo = assignedUserId, DepartmentId = departmentId },
                    managerId);

                TempData["Success"] = $"Task '{taskDto.Title}' created and assigned successfully.";
                return RedirectToAction("Dashboard");
            }
            catch (TaskLimitExceededException ex)
            {
                ModelState.AddModelError("", ex.Message);
                ModelState.AddModelError("", "Available users with no tasks: " +
                    string.Join(", ", ex.AvailableUsers.Select(u => u.Username)));
                await LoadDepartmentsAsync();
                return View();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error: {ex.Message}");
                await LoadDepartmentsAsync();
                return View();
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // UPDATE TASK
        // ─────────────────────────────────────────────────────────────────────

        [HttpGet("GetTask")]
        public async Task<IActionResult> GetTask(int id)
        {
            if (id <= 0)
                return BadRequest(new { message = "ID must be a positive integer." });

            var task = await _context.TaskItem
                .Where(t => t.Id == id)
                .Select(t => new
                {
                    t.Id,
                    t.Title,
                    t.Description,
                    Status = t.Status.ToString(),
                    t.DepartmentId,
                    AssignedUserId = (int?)t.TaskAssignments.FirstOrDefault()!.UserId
                })
                .FirstOrDefaultAsync();

            if (task == null)
                return NotFound(new { message = $"No task found with ID {id}." });

            return Ok(task);
        }

        [HttpGet("UpdateTask")]
        public async Task<IActionResult> UpdateTask()
        {
            await LoadDepartmentsAsync();
            return View();
        }

        [HttpPost("UpdateTaskPost")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateTaskPost()
        {
            await LoadDepartmentsAsync();

            var idStr = Request.Form["Id"].ToString();
            var title = Request.Form["Title"].ToString().Trim();
            var description = Request.Form["Description"].ToString().Trim();
            var status = Request.Form["Status"].ToString();
            var deptIdStr = Request.Form["DepartmentId"].ToString();
            var assigneeStr = Request.Form["AssignedUserId"].ToString();

            int managerId = GetCurrentUserId();

            IActionResult ReturnWithErrors()
            {
                ViewBag.HasFormErrors = true;
                ViewBag.PostedId = idStr;
                ViewBag.PostedTitle = title;
                ViewBag.PostedDesc = description;
                ViewBag.PostedStatus = status;
                ViewBag.PostedDept = deptIdStr;
                ViewBag.PostedAssignee = assigneeStr;
                return View("UpdateTask");
            }

            // ── Permission ────────────────────────────────────────────────────
            bool canUpdate = await _permissionService.HasPermissionAsync(managerId, PermissionType.UpdateTask);
            if (!canUpdate)
            {
                ModelState.AddModelError("", "You do not have permission to update tasks.");
                return ReturnWithErrors();
            }

            // ── Validation ────────────────────────────────────────────────────
            if (!int.TryParse(idStr, out int taskId) || taskId <= 0)
            {
                ModelState.AddModelError("", "Invalid or missing task ID. Please search for a task first.");
                return ReturnWithErrors();
            }

            if (string.IsNullOrWhiteSpace(title))
            {
                ModelState.AddModelError("", "Title is required.");
                return ReturnWithErrors();
            }

            if (string.IsNullOrWhiteSpace(status))
            {
                ModelState.AddModelError("", "Status is required.");
                return ReturnWithErrors();
            }

            if (!int.TryParse(deptIdStr, out int deptId) || deptId <= 0)
            {
                ModelState.AddModelError("", "Please select a valid department.");
                return ReturnWithErrors();
            }

            if (!Enum.TryParse<Domain.Enums.TaskStatus>(status, out _))
            {
                ModelState.AddModelError("", $"'{status}' is not a valid status.");
                return ReturnWithErrors();
            }

            var taskExists = await _context.TaskItem.AnyAsync(t => t.Id == taskId);
            if (!taskExists)
            {
                ModelState.AddModelError("", $"Task with ID {taskId} does not exist.");
                return ReturnWithErrors();
            }

            var deptExists = await _context.Department.AnyAsync(d => d.Id == deptId);
            if (!deptExists)
            {
                ModelState.AddModelError("", $"Department with ID {deptId} does not exist.");
                return ReturnWithErrors();
            }

            int? newAssigneeId = int.TryParse(assigneeStr, out int parsedAssignee) && parsedAssignee > 0
                ? parsedAssignee
                : null;

            if (newAssigneeId.HasValue)
            {
                var assigneeExists = await _context.Users.AnyAsync(u => u.Id == newAssigneeId.Value);
                if (!assigneeExists)
                {
                    ModelState.AddModelError("", "Selected user does not exist.");
                    return ReturnWithErrors();
                }
            }

            try
            {
                // ── 1. Update task fields ─────────────────────────────────────
                await _managerServices.UpdateTasksAsync(taskId, new UpdateTaskDTO(
                    title,
                    description,
                    status,
                    deptId,
                    DateTime.UtcNow
                ));

                // ── 2. Handle reassignment ────────────────────────────────────
                var existingAssignment = await _context.TaskAssignment
                    .FirstOrDefaultAsync(a => a.TaskItemId == taskId);

                int? oldAssigneeId = existingAssignment?.UserId;
                bool assignmentChanged = newAssigneeId != oldAssigneeId;

                if (assignmentChanged)
                {
                    if (existingAssignment != null)
                    {
                        _context.TaskAssignment.Remove(existingAssignment);
                        await _context.SaveChangesAsync();

                        await _auditService.LogAsync(
                            "Unassigned", "TaskAssignment", taskId,
                            new { UserId = oldAssigneeId, TaskId = taskId },
                            null,
                            managerId);
                    }

                    if (newAssigneeId.HasValue)
                    {
                        _context.TaskAssignment.Add(new TaskAssignment
                        {
                            TaskItemId = taskId,
                            UserId = newAssigneeId.Value,
                            AssignedAt = DateTime.UtcNow
                        });
                        await _context.SaveChangesAsync();

                        await _auditService.LogAsync(
                            "Assigned", "TaskAssignment", taskId,
                            null,
                            new { UserId = newAssigneeId.Value, TaskId = taskId },
                            managerId);
                    }
                }

                TempData["Success"] = $"Task '{title}' updated successfully.";
                return RedirectToAction("Dashboard");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Unexpected error: {ex.Message}");
                return ReturnWithErrors();
            }
        }
    }
}