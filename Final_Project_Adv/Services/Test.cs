using Final_Project_Adv.Domain.DTO;
using Final_Project_Adv.Domain.Entities;
using Final_Project_Adv.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Final_Project_Adv.Services
{
    public class Test(AppDbContext context):ITest
    {
        public async Task<DepartmentDto> InsertDepartment(CreateDepartmentDto dto)
        {
            var department = new Department
            {
                Name = dto.Name,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            context.Department.Add(department);
            await context.SaveChangesAsync();

            return new DepartmentDto
            {
                Id = department.Id,
                Name = department.Name,
                CreatedAt = department.CreatedAt,
                UpdatedAt = department.UpdatedAt
            };
        }
        public async Task<UsersDto> InsertUser(CreateUserDto dto)
        {
            // 🔍 Optional: check if department exists
            var departmentExists = await context.Department
                .AnyAsync(d => d.Id == dto.DepartmentId);

            if (!departmentExists)
                throw new Exception("Department not found");

            var user = new Users
            {
                Username = dto.Username,
                Password = dto.Password,
                Email = dto.Email,
                Role = dto.Role,
                DepartmentId = dto.DepartmentId,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            return new UsersDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                DepartmentId = user.DepartmentId
            };
        }
    }
    }

