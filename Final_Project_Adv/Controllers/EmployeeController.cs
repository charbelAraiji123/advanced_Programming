using Final_Project_Adv.Domain.DTO;

using Final_Project_Adv.Services.Final_Project_Adv.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Final_Project_Adv.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Employee")]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeServices _employeeServices;

        public EmployeeController(IEmployeeServices employeeServices)
        {
            _employeeServices = employeeServices;
        }

        // ================= TASKS =================

        [HttpGet("MyTasks/{userId}")]
        public async Task<IActionResult> GetMyTasks(int userId)
        {
            var tasks = await _employeeServices.GetMyTasksAsync(userId);
            return Ok(tasks);
        }

        [HttpGet("MySubtasks/{userId}")]
        public async Task<IActionResult> GetMySubtasks(int userId)
        {
            var subtasks = await _employeeServices.GetMySubtasksAsync(userId);
            return Ok(subtasks);
        }

        // ================= SUBMIT =================

        [HttpPut("SubmitTask/{taskId}")]
        public async Task<IActionResult> SubmitTask(int taskId)
        {
            var result = await _employeeServices.SubmitTaskAsync(taskId);

            if (!result)
                return NotFound(new { message = "Task not found" });

            return Ok(new { message = "Task submitted successfully" });
        }

        [HttpPut("SubmitSubtask/{subtaskId}")]
        public async Task<IActionResult> SubmitSubtask(int subtaskId)
        {
            var result = await _employeeServices.SubmitSubtaskAsync(subtaskId);

            if (!result)
                return NotFound(new { message = "Subtask not found" });

            return Ok(new { message = "Subtask submitted successfully" });
        }

        // ================= REQUEST TASK =================

        [HttpPost("RequestTask/{userId}/{taskId}")]
        public async Task<IActionResult> RequestTask(int userId, int taskId)
        {
            var success = await _employeeServices.RequestTaskAssignmentAsync(userId, taskId);

            if (!success)
                return BadRequest(new { message = "Already assigned or request exists" });

            return Ok(new { message = "Request submitted successfully" });
        }

        // ================= COMMENTS =================

        [HttpPost("TaskComment")]
        public async Task<IActionResult> AddTaskComment([FromBody] CreateTaskCommentDto dto)
        {
            var result = await _employeeServices.AddTaskCommentAsync(dto);
            return CreatedAtAction(nameof(AddTaskComment), new { id = result.Id }, result);
        }

        [HttpPost("SubtaskComment")]
        public async Task<IActionResult> AddSubtaskComment([FromBody] CreateSubtaskCommentDto dto)
        {
            var result = await _employeeServices.AddSubtaskCommentAsync(dto);
            return CreatedAtAction(nameof(AddSubtaskComment), new { id = result.Id }, result);
        }
    }
}