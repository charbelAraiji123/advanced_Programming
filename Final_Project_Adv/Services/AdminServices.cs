using Final_Project_Adv.Domain.DTO;
using Final_Project_Adv.Models;
using Final_Project_Adv.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Final_Project_Adv.Services
{
    public class AdminServices(AppDbContext context, PermissionService permissionService) : IAdminServices
    {
        // ─────────────────────────────────────────────────────────────────────
        // USER CRUD
        // ─────────────────────────────────────────────────────────────────────

        public async Task<UsersDto> CreateUserAsync(CreateUserDto dto)
        {
            // 1. Validate Department
            var departmentExists = await context.Department.AnyAsync(d => d.Id == dto.DepartmentId.Value);
            if (!departmentExists)
                throw new Exception($"Department ID {dto.DepartmentId} does not exist.");

            // 2. Validate Username Uniqueness
            var userExists = await context.Users.AnyAsync(u => u.Username == dto.Username);
            if (userExists)
                throw new Exception("Username is already taken.");

            // 3. Map DTO to Entity
            var user = new Users
            {
                Username = dto.Username,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Email = dto.Email,
                Role = dto.Role,
                DepartmentId = dto.DepartmentId.Value,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // 4. Persistence
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

        /// <summary>
        /// Updates an existing user's profile fields.
        /// Validates: user existence, username uniqueness (excluding self),
        /// department existence.
        /// </summary>
        public async Task UpdateUserAsync(UsersDto usersDto)
        {
            // 1. User must exist
            var user = await context.Users.FindAsync(usersDto.Id);
            if (user == null)
                throw new Exception($"User with ID {usersDto.Id} does not exist.");

            // 2. Username uniqueness – exclude the current user
            var usernameTaken = await context.Users
                .AnyAsync(u => u.Username == usersDto.Username && u.Id != usersDto.Id);
            if (usernameTaken)
                throw new Exception($"Username '{usersDto.Username}' is already taken by another user.");

            // 3. Department must exist
            var deptExists = await context.Department.AnyAsync(d => d.Id == usersDto.DepartmentId);
            if (!deptExists)
                throw new Exception($"Department with ID {usersDto.DepartmentId} does not exist.");

            // 4. Apply changes
            user.Username = usersDto.Username;
            user.Email = usersDto.Email;
            user.Role = usersDto.Role;
            user.DepartmentId = usersDto.DepartmentId;
            user.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();
        }

        // ─────────────────────────────────────────────────────────────────────
        // PERMISSION LOGIC
        // ─────────────────────────────────────────────────────────────────────

        public async Task<UserPermissionDto> GrantPermissionToUserAsync(GrantPermissionDto dto)
            => await permissionService.GrantPermissionAsync(dto);

        public async Task<UserPermissionDto> RevokePermissionFromUserAsync(RevokePermissionDto dto)
            => await permissionService.RevokePermissionAsync(dto);

        public async Task<UserPermissionDto> GetUserPermissionsAsync(int userId)
            => await permissionService.GetUserPermissionsAsync(userId);

        // ─────────────────────────────────────────────────────────────────────
        // DEPARTMENT LOGIC
        // ─────────────────────────────────────────────────────────────────────

        public async Task<DepartmentDto> CreateDeptAsync(CreateDepartmentDto dto)
        {
            var dept = new Department
            {
                Name = dto.Name,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
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