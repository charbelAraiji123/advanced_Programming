using Final_Project_Adv.Domain.DTO;
using Final_Project_Adv.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = $"{JwtBearerDefaults.AuthenticationScheme},CookieAuth", Roles = "Manager")]
public class ManagerApiController : ControllerBase
{
    private readonly IManagerServices _managerServices;

    public ManagerApiController(IManagerServices managerServices)
    {
        _managerServices = managerServices;
    }

    [HttpPost("Users/Create")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
    {
        var result = await _managerServices.CreateUserAsync(dto);
        return CreatedAtAction(nameof(CreateUser), new { id = result.Id }, result);
    }

    [HttpPost("Task/Create")]
    public async Task<IActionResult> CreateTaskItem([FromBody] CreateTaskItemDto dto)
    {
        var result = await _managerServices.CreateTaskAsync(dto);
        return CreatedAtAction(nameof(CreateTaskItem), new { id = result.Id }, result);
    }

    [HttpDelete("Task/Delete/{id}")]
    public async Task<IActionResult> DeleteTaskItem(int id)
    {
        // Getting the current logged-in user's ID from the Claims
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        int requestingUserId = int.Parse(userIdClaim ?? "0");

        // Ensure your service method accepts both arguments
        var deleted = await _managerServices.DeleteTaskAsync(id, requestingUserId);

        if (!deleted)
            return NotFound(new { error = $"Task with ID {id} not found." });

        return NoContent();
    }

    [HttpPost("Subtask/Create")]
    public async Task<IActionResult> CreateSubtask([FromBody] CreateSubtaskDto dto)
    {
        try
        {
            var result = await _managerServices.CreateSubtaskAsync(dto);
            return CreatedAtAction(nameof(CreateSubtask), new { id = result.Id }, result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpDelete("SubTask/Delete/{id}")]
    public async Task<IActionResult> DeleteSubTaskItem(int id)
    {
        // 1. Extract the current logged-in user's ID from the JWT token claims
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        // 2. Parse the ID and handle cases where the claim might be missing
        if (!int.TryParse(userIdClaim, out int requestingUserId))
        {
            return Unauthorized(new { error = "User identification missing from token." });
        }

        // 3. Pass BOTH arguments to the service
        var deleted = await _managerServices.DeleteSubTaskAsync(id, requestingUserId);

        if (!deleted)
            return NotFound(new { error = $"Subtask with ID {id} not found." });

        return NoContent();
    }

    [HttpPut("users/update/{id}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto dto)
    {
        try
        {
            var result = await _managerServices.UpdateUserAsync(id, dto);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpGet("users/GetAllUser")]
    public async Task<IActionResult> GetAllusers()
    {
        var users = await _managerServices.GetAllUsersAsync();
        return Ok(users);
    }

    [HttpGet("Tasks/GetAllTaks")]
    public async Task<IActionResult> GetAllTasks()
    {
        var tasks = await _managerServices.GetAllTasksAsync();
        return Ok(tasks);
    }

    [HttpGet("Subtasks/GetAll")]
    public async Task<IActionResult> GetAllSubtasks()
    {
        var subtasks = await _managerServices.GetAllSubTasksAsync();
        return Ok(subtasks);
    }

    [HttpPut("Task/Edit/{id}")]
    public async Task<IActionResult> UpdateTaskItem(int id, [FromBody] UpdateTaskDTO dto)
    {
        try
        {
            var result = await _managerServices.UpdateTasksAsync(id, dto);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpPut("Subtask/Edit/{id}")]
    public async Task<IActionResult> UpdateSubtask(int id, [FromBody] UpdateSubTaskDTO dto)
    {
        try
        {
            var result = await _managerServices.UpdateSubTasksAsync(id, dto);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpPost("TaskAssignment/{UId}/{TId}")]
    public async Task<IActionResult> TaskAssign(int UId, int TId)
    {
        try
        {
            var result = await _managerServices.TaskAssignAsync(UId, TId);
            return Ok(result);
        }
        catch (TaskLimitExceededException ex)
        {
            return BadRequest(new
            {
                message = ex.Message,
                availableUsers = ex.AvailableUsers
            });
        }
    }

    [HttpDelete("Unassigntask/{userId}/{taskId}")]
    public async Task<IActionResult> UnassignTask(int userId, int taskId)
    {
        var result = await _managerServices.UnassignTaskAsync(userId, taskId);
        return Ok(result);
    }

    [HttpGet("ProgressBYUser/{userId}")]
    public async Task<ActionResult<UserTaskStatusDto>> GetUserTaskStatus(int userId)
    {
        try
        {
            var result = await _managerServices.GetUserTaskStatus(userId);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost("TaskComment")]
    public async Task<IActionResult> TaskComment([FromBody] CreateTaskCommentDto dto)
    {
        var result = await _managerServices.TaskCommentAsync(dto);
        return CreatedAtAction(nameof(TaskComment), new { id = result.Id }, result);
    }

    [HttpPost("SubTaskComment")]
    public async Task<IActionResult> SubTaskCommentAsync([FromBody] CreateSubtaskCommentDto dto)
    {
        var result = await _managerServices.SubTaskCommentAsync(dto);
        return CreatedAtAction(nameof(SubTaskCommentAsync), new { id = result.Id }, result);
    }

    [HttpGet("old-tasks")]
    public async Task<IActionResult> GetOldTasks()
    {
        var result = await _managerServices.GetOldTasksAsync();
        return Ok(result);
    }
}