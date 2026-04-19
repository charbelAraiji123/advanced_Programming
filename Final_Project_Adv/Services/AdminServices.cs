using Final_Project_Adv.Domain.DTO;
using Final_Project_Adv.Domain.Entities;
using Final_Project_Adv.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Final_Project_Adv.Services
{
    public class AdminServices(AppDbContext context):IAdminServices
    {

        public async Task<UsersDto> CreateUserAsync(CreateUserDto dto)
        {

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

        public async Task DeleteUserAsync(int id)
        {
            var user = await context.Users.FindAsync(id);
            if (user == null) return;

            context.Users.Remove(user);
            await context.SaveChangesAsync();
        }

        public async Task UpdateUserAsync(UsersDto usersDto)
        {
            var user = await context.Users.FindAsync(usersDto.Id);
            if (user == null) return;

            user.Username = usersDto.Username;
            user.Email = usersDto.Email;
            user.Role = usersDto.Role;
            user.DepartmentId = usersDto.DepartmentId;
            user.UpdatedAt = DateTime.Now;

            await context.SaveChangesAsync();
        }


        public async Task<DepartmentDto> CreateDeptAsync(CreateDepartmentDto dto) {
            var dept = new Department
            {
                Name = dto.Name,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            context.Department.Add(dept);
            await context.SaveChangesAsync();

            return new DepartmentDto
            {
                Id = dept.Id,
                Name = dept.Name,
                CreatedAt = dept.CreatedAt,
                UpdatedAt = dept.UpdatedAt
            };
        }
        public async Task DeleteDeptAsync(int id) {
            var dept = await context.Department.FindAsync(id);
            if (dept == null) return;

            context.Department.Remove(dept);
            await context.SaveChangesAsync();
        }

        public async Task UpdateDptAsync(DepartmentDto deptDto) {
            var dept = await context.Department.FindAsync(deptDto.Id);
            if (dept == null) return;

            dept.Name = deptDto.Name;
            dept.Id = deptDto.Id;
            dept.UpdatedAt = DateTime.Now;

            await context.SaveChangesAsync();
        }


    }
}


