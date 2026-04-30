using Final_Project_Adv.Domain.DTO;

using Final_Project_Adv.Infrastructure.Data;
using Final_Project_Adv.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;


namespace Final_Project_Adv.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class ManagerController(IManagerServices ManagerServices) : ControllerBase
    {
        [HttpPost("Users/Create")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
        {
            var result = await ManagerServices.CreateUserAsync(dto);
            return CreatedAtAction(nameof(CreateUser), new { id = result.Id }, result);
        }
        [HttpPost("Task/Create")]
        public async Task<IActionResult> CreateTaskItem([FromBody] CreateTaskItemDto dto)
        {
            var result = await ManagerServices.CreateTaskAsync(dto);
            return CreatedAtAction(nameof(CreateTaskItem), new { id = result.Id }, result);
        }


        // Inside ManagerController.cs

        [HttpDelete("Task/Delete/{id}")]
        public async Task<IActionResult> DeleteTaskItem(int id, [FromQuery] int requestingUserId)
        {
            // Now the interface accepts 2 arguments, so this will compile!
            var deleted = await ManagerServices.DeleteTaskAsync(id, requestingUserId);

            if (!deleted)
                return NotFound(new { error = $"Task with ID {id} not found." });

            return NoContent();
        }


        [HttpPost("Subtask/Create")]
        public async Task<IActionResult> CreateSubtask([FromBody] CreateSubtaskDto dto)
        {

            try
            {
                var result = await ManagerServices.CreateSubtaskAsync(dto);
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

            var deleted = await ManagerServices.DeleteSubTaskAsync(id);
            if (!deleted)
                return NotFound(new { error = $"Task with ID {id} not found." });

            return NoContent();
        }
        [HttpPut("users/update/{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto dto)
        {
            try
            {
                var result = await ManagerServices.UpdateUserAsync(id, dto);
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
            var users = await ManagerServices.GetAllUsersAsync();

            return Ok(users);
        }
        [HttpGet("Tasks/GetAllTaks")]
        public async Task<IActionResult> GetAllTasks()
        {
            var tasks = await ManagerServices.GetAllTasksAsync();

            return Ok(tasks);
        }
        [HttpGet("Subtasks/GetAll")]

        public async Task<IActionResult> GetAllSubtasks()
        { 
            var subtasks = await ManagerServices.GetAllSubTasksAsync();
            return Ok(subtasks);    
            
        }
        [HttpPut("Task/Edit/{id}")]
        public async Task<IActionResult> UpdateTaskItem(int id , [FromBody] UpdateTaskDTO dto)
        {
            try
            {
                var result = await ManagerServices.UpdateTasksAsync(id,dto);
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
                var result = await ManagerServices.UpdateSubTasksAsync(id, dto);
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
                var result = await ManagerServices.TaskAssignAsync(UId, TId);
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
            var result = await ManagerServices.UnassignTaskAsync(userId, taskId);
            return Ok(result);
        }
        [HttpGet("ProgressBYUser/{userId}")]
        public async Task<ActionResult<UserTaskStatusDto>> GetUserTaskStatus(int userId)
        {
            try
            {
                var result = await ManagerServices.GetUserTaskStatus(userId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
        [HttpPost("TaskComment")]

        public async Task<ActionResult<UserTaskStatusDto>> TaskComment([FromBody] CreateTaskCommentDto dto)
        {
            var result = await ManagerServices.TaskCommentAsync(dto);
            return CreatedAtAction(nameof(CreateUser), new { id = result.Id }, result);
        }
        [HttpPost("SubTaskComment")]

        public async Task<ActionResult<UserTaskStatusDto>> SubTaskCommentAsync([FromBody]CreateSubtaskCommentDto dto)
        {
            var result = await ManagerServices.SubTaskCommentAsync(dto);
            return CreatedAtAction(nameof(CreateUser), new { id = result.Id }, result);
        }
        [HttpGet("old-tasks")]
        public async Task<IActionResult> GetOldTasks()
        {
            var result = await ManagerServices.GetOldTasksAsync();
            return Ok(result);
        }
    }
}
