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
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Manager")]
    public class ManagerApiController : ControllerBase
    {
        private readonly IManagerServices _managerServices;

        public ManagerApiController(IManagerServices managerServices)
        {
            _managerServices = managerServices;
        }

        private int GetCurrentUserId()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(claim, out int id) ? id : 0;
        }

        // ─────────────────────────────────────────────────────────────────────
        // USER
        // ─────────────────────────────────────────────────────────────────────

        [HttpPost("users/create")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var result = await _managerServices.CreateUserAsync(dto);
                return CreatedAtAction(nameof(GetAllUsers), new { id = result.Id }, result);
            }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        [HttpPut("users/update/{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto dto)
        {
            try
            {
                var result = await _managerServices.UpdateUserAsync(id, dto);
                return Ok(result);
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }

        [HttpGet("users/all")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _managerServices.GetAllUsersAsync();
            return Ok(users);
        }

        // ─────────────────────────────────────────────────────────────────────
        // TASK
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Creates a task. The CreatedById in the DTO is overridden with the
        /// authenticated manager's ID extracted from the JWT claim.
        /// </summary>
        [HttpPost("tasks/create")]
        public async Task<IActionResult> CreateTask([FromBody] CreateTaskItemDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            dto.CreatedById = GetCurrentUserId(); // always use JWT identity
            try
            {
                var result = await _managerServices.CreateTaskAsync(dto);
                return CreatedAtAction(nameof(GetAllTasks), new { id = result.Id }, result);
            }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        [HttpDelete("tasks/delete/{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            try
            {
                var deleted = await _managerServices.DeleteTaskAsync(id, GetCurrentUserId());
                if (!deleted) return NotFound(new { message = $"Task {id} not found." });
                return NoContent();
            }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
        }

        [HttpPut("tasks/update/{id}")]
        public async Task<IActionResult> UpdateTask(int id, [FromBody] UpdateTaskDTO dto)
        {
            try
            {
                var result = await _managerServices.UpdateTasksAsync(id, dto);
                return Ok(result);
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }

        [HttpGet("tasks/all")]
        public async Task<IActionResult> GetAllTasks()
        {
            var tasks = await _managerServices.GetAllTasksAsync();
            return Ok(tasks);
        }

        [HttpGet("tasks/old")]
        public async Task<IActionResult> GetOldTasks()
        {
            var result = await _managerServices.GetOldTasksAsync();
            return Ok(result);
        }

        // ─────────────────────────────────────────────────────────────────────
        // SUBTASK
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Creates a subtask linked to a specific task and assigned to an employee.
        /// Body must include TaskItemId and AssignedToId.
        /// </summary>
        [HttpPost("subtasks/create")]
        public async Task<IActionResult> CreateSubtask([FromBody] CreateSubtaskDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            // Override creator with JWT identity
            dto = dto with { CreatedById = GetCurrentUserId() };
            try
            {
                var result = await _managerServices.CreateSubtaskAsync(dto);
                return CreatedAtAction(nameof(GetAllSubtasks), new { id = result.Id }, result);
            }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        [HttpDelete("subtasks/delete/{id}")]
        public async Task<IActionResult> DeleteSubtask(int id)
        {
            try
            {
                var deleted = await _managerServices.DeleteSubTaskAsync(id, GetCurrentUserId());
                if (!deleted) return NotFound(new { message = $"Subtask {id} not found." });
                return NoContent();
            }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
        }

        [HttpPut("subtasks/update/{id}")]
        public async Task<IActionResult> UpdateSubtask(int id, [FromBody] UpdateSubTaskDTO dto)
        {
            try
            {
                var result = await _managerServices.UpdateSubTasksAsync(id, dto);
                return Ok(result);
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }

        [HttpGet("subtasks/all")]
        public async Task<IActionResult> GetAllSubtasks()
        {
            var subtasks = await _managerServices.GetAllSubTasksAsync();
            return Ok(subtasks);
        }

        // ─────────────────────────────────────────────────────────────────────
        // ASSIGNMENT
        // ─────────────────────────────────────────────────────────────────────

        [HttpPost("assignments/assign/{userId}/{taskId}")]
        public async Task<IActionResult> AssignTask(int userId, int taskId)
        {
            try
            {
                var result = await _managerServices.TaskAssignAsync(userId, taskId);
                return Ok(result);
            }
            catch (TaskLimitExceededException ex)
            {
                return BadRequest(new { message = ex.Message, availableUsers = ex.AvailableUsers });
            }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        [HttpDelete("assignments/unassign/{userId}/{taskId}")]
        public async Task<IActionResult> UnassignTask(int userId, int taskId)
        {
            try
            {
                var result = await _managerServices.UnassignTaskAsync(userId, taskId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }

        // ─────────────────────────────────────────────────────────────────────
        // PROGRESS
        // ─────────────────────────────────────────────────────────────────────

        [HttpGet("progress/{userId}")]
        public async Task<IActionResult> GetUserTaskStatus(int userId)
        {
            try
            {
                var result = await _managerServices.GetUserTaskStatus(userId);
                return Ok(result);
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
                var result = await _managerServices.TaskCommentAsync(dto);
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
                var result = await _managerServices.SubTaskCommentAsync(dto);
                return CreatedAtAction(nameof(AddSubtaskComment), new { id = result.Id }, result);
            }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }
    }
}