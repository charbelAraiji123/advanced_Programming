using Final_Project_Adv.Domain.DTO;
using Final_Project_Adv.Models;
using Final_Project_Adv.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Final_Project_Adv.Services
{
    public class AdminServices(AppDbContext context, PermissionService permissionService) : IAdminServices
    {
        public async Task<UsersDto> CreateUserAsync(CreateUserDto dto)
        {
            var departmentExists = await context.Department.AnyAsync(d => d.Id == dto.DepartmentId);
            if (!departmentExists)
                throw new Exception("Department not found");

            var user = new Users
            {
                Username = dto.Username,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password), // Added Hashing
                Email = dto.Email,
                Role = dto.Role,
                DepartmentId = dto.DepartmentId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
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
            user.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();
        }

        // --- Permission Logic ---
        public async Task<UserPermissionDto> GrantPermissionToUserAsync(GrantPermissionDto dto)
            => await permissionService.GrantPermissionAsync(dto);

        public async Task<UserPermissionDto> RevokePermissionFromUserAsync(RevokePermissionDto dto)
            => await permissionService.RevokePermissionAsync(dto);

        public async Task<UserPermissionDto> GetUserPermissionsAsync(int userId)
            => await permissionService.GetUserPermissionsAsync(userId);

        // --- Department Logic ---
        public async Task<DepartmentDto> CreateDeptAsync(CreateDepartmentDto dto)
        {
            var dept = new Department { Name = dto.Name, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            context.Department.Add(dept);
            await context.SaveChangesAsync();
            return new DepartmentDto { Id = dept.Id, Name = dept.Name, CreatedAt = dept.CreatedAt, UpdatedAt = dept.UpdatedAt };
        }

        public async Task DeleteDeptAsync(int id)
        {
            var dept = await context.Department.FindAsync(id);
            if (dept != null)
            {
                context.Department.Remove(dept);
                await context.SaveChangesAsync();
            }
        }

        public async Task UpdateDptAsync(DepartmentDto deptDto)
        {
            var dept = await context.Department.FindAsync(deptDto.Id);
            if (dept == null) return;
            dept.Name = deptDto.Name;
            dept.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync();
        }
    }
}