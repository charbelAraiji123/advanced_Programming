using Final_Project_Adv.Domain.DTO;
using Final_Project_Adv.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Final_Project_Adv.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Employee")]
    public class EmployeeApiController : ControllerBase
    {
        private readonly IEmployeeServices _employeeServices;

        public EmployeeApiController(IEmployeeServices employeeServices)
        {
            _employeeServices = employeeServices;
        }

        private int GetCurrentUserId()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(claim, out int id) ? id : 0;
        }

        // ─────────────────────────────────────────────────────────────────────
        // READ
        // ─────────────────────────────────────────────────────────────────────

        [HttpGet("tasks/mine")]
        public async Task<IActionResult> GetMyTasks()
        {
            var tasks = await _employeeServices.GetMyTasksAsync(GetCurrentUserId());
            return Ok(tasks);
        }

        [HttpGet("subtasks/mine")]
        public async Task<IActionResult> GetMySubtasks()
        {
            var subtasks = await _employeeServices.GetMySubtasksAsync(GetCurrentUserId());
            return Ok(subtasks);
        }

        // ─────────────────────────────────────────────────────────────────────
        // ACCEPT / SUBMIT
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>Moves task from Pending → InProgress.</summary>
        [HttpPut("tasks/accept/{taskId}")]
        public async Task<IActionResult> AcceptTask(int taskId)
        {
            try
            {
                var result = await _employeeServices.AcceptTaskAsync(taskId);
                if (!result) return NotFound(new { message = "Task not found." });
                return Ok(new { message = "Task accepted and set to InProgress." });
            }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
        }

        /// <summary>Moves task from InProgress → Completed.</summary>
        [HttpPut("tasks/submit/{taskId}")]
        public async Task<IActionResult> SubmitTask(int taskId)
        {
            try
            {
                var result = await _employeeServices.SubmitTaskAsync(taskId);
                if (!result) return NotFound(new { message = "Task not found." });
                return Ok(new { message = "Task submitted successfully." });
            }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
        }

        /// <summary>Moves subtask → Completed.</summary>
        [HttpPut("subtasks/submit/{subtaskId}")]
        public async Task<IActionResult> SubmitSubtask(int subtaskId)
        {
            try
            {
                var result = await _employeeServices.SubmitSubtaskAsync(subtaskId);
                if (!result) return NotFound(new { message = "Subtask not found." });
                return Ok(new { message = "Subtask submitted successfully." });
            }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
        }

        // ─────────────────────────────────────────────────────────────────────
        // REQUEST ASSIGNMENT
        // ─────────────────────────────────────────────────────────────────────

        [HttpPost("tasks/request/{taskId}")]
        public async Task<IActionResult> RequestTask(int taskId)
        {
            try
            {
                var success = await _employeeServices.RequestTaskAssignmentAsync(GetCurrentUserId(), taskId);
                if (!success) return BadRequest(new { message = "Already assigned or request exists." });
                return Ok(new { message = "Assignment request submitted." });
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }

        // ─────────────────────────────────────────────────────────────────────
        // COMMENTS
        // ─────────────────────────────────────────────────────────────────────

        [HttpPost("comments/task")]
        public async Task<IActionResult> AddTaskComment([FromBody] CreateTaskCommentDto dto)
        {
            dto.AuthorId = GetCurrentUserId();
            try
            {
                var result = await _employeeServices.AddTaskCommentAsync(dto);
                return CreatedAtAction(nameof(AddTaskComment), new { id = result.Id }, result);
            }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }

        [HttpPost("comments/subtask")]
        public async Task<IActionResult> AddSubtaskComment([FromBody] CreateSubtaskCommentDto dto)
        {
            dto.AuthorId = GetCurrentUserId();
            try
            {
                var result = await _employeeServices.AddSubtaskCommentAsync(dto);
                return CreatedAtAction(nameof(AddSubtaskComment), new { id = result.Id }, result);
            }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }
    }
}