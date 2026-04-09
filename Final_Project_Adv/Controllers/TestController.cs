using Final_Project_Adv.Domain.DTO;
using Final_Project_Adv.Infrastructure.Data;
using Final_Project_Adv.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Final_Project_Adv.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController(ITest Test ):ControllerBase {
        [HttpPost]
        public async Task<ActionResult<DepartmentDto>> InsertDepartment([FromBody] CreateDepartmentDto dto)
        {
            var result = await Test.InsertDepartment(dto);
            return Ok(result);
        }

        [HttpPost("user")]
        public async Task<ActionResult<UsersDto>> InsertUser([FromBody] CreateUserDto dto)
        {
            var result = await Test.InsertUser(dto);
            return Ok(result);
        }
    }
}
