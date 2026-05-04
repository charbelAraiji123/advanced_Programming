using Final_Project_Adv.Domain.DTO;
using Final_Project_Adv.Models;
using Final_Project_Adv.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Final_Project_Adv.Services
{
    public class AdminServices(AppDbContext context, PermissionService permissionService, AuditService auditService) : IAdminServices
    {
        // ─────────────────────────────────────────────────────────────────────
        // USER CRUD
        // ─────────────────────────────────────────────────────────────────────

        public async Task<UsersDto> CreateUserAsync(CreateUserDto dto, int performedById)
        {
            var departmentExists = await context.Department.AnyAsync(d => d.Id == dto.DepartmentId.Value);
            if (!departmentExists)
                throw new Exception($"Department ID {dto.DepartmentId} does not exist.");

            var userExists = await context.Users.AnyAsync(u => u.Username == dto.Username);
            if (userExists)
                throw new Exception("Username is already taken.");

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

            context.Users.Add(user);
            await context.SaveChangesAsync();

            // ✅ Audit AFTER save so user.Id is populated
            await auditService.LogAsync("Created", "User", user.Id, null, user, performedById);

            return new UsersDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                DepartmentId = user.DepartmentId
            };
        }

        public async Task DeleteUserAsync(int id, int performedById)
        {
            var user = await context.Users.FindAsync(id);
            if (user == null) return;

            // ✅ Audit BEFORE remove so the entity still exists in DB
            await auditService.LogAsync("Deleted", "User", id, user, null, performedById);

            context.Users.Remove(user);
            await context.SaveChangesAsync();
        }

        public async Task UpdateUserAsync(UsersDto usersDto, int performedById)
        {
            var user = await context.Users.FindAsync(usersDto.Id);
            if (user == null)
                throw new Exception($"User with ID {usersDto.Id} does not exist.");

            var usernameTaken = await context.Users
                .AnyAsync(u => u.Username == usersDto.Username && u.Id != usersDto.Id);
            if (usernameTaken)
                throw new Exception($"Username '{usersDto.Username}' is already taken by another user.");

            var deptExists = await context.Department.AnyAsync(d => d.Id == usersDto.DepartmentId);
            if (!deptExists)
                throw new Exception($"Department with ID {usersDto.DepartmentId} does not exist.");

            var oldUser = new { user.Username, user.Email, user.Role, user.DepartmentId };

            user.Username = usersDto.Username;
            user.Email = usersDto.Email;
            user.Role = usersDto.Role;
            user.DepartmentId = usersDto.DepartmentId;
            user.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();

            await auditService.LogAsync("Updated", "User", user.Id, oldUser, user, performedById);
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

        public async Task<DepartmentDto> CreateDeptAsync(CreateDepartmentDto dto, int performedById)
        {
            var dept = new Department
            {
                Name = dto.Name,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            context.Department.Add(dept);
            await context.SaveChangesAsync();

            await auditService.LogAsync("Created", "Department", dept.Id, null, dept, performedById);

            return new DepartmentDto
            {
                Id = dept.Id,
                Name = dept.Name,
                CreatedAt = dept.CreatedAt,
                UpdatedAt = dept.UpdatedAt
            };
        }

        public async Task DeleteDeptAsync(int id, int performedById)
        {
            var dept = await context.Department.FindAsync(id);
            if (dept == null) return;

            // ✅ Audit BEFORE remove
            await auditService.LogAsync("Deleted", "Department", id, dept, null, performedById);

            context.Department.Remove(dept);
            await context.SaveChangesAsync();
        }

        public async Task UpdateDptAsync(DepartmentDto deptDto, int performedById)
        {
            var dept = await context.Department.FindAsync(deptDto.Id);
            if (dept == null)
                throw new Exception($"Department with ID {deptDto.Id} does not exist."); // ✅ throw, not silent return

            var oldDept = new { dept.Name };

            dept.Name = deptDto.Name;
            dept.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();

            await auditService.LogAsync("Updated", "Department", dept.Id, oldDept, dept, performedById);
        }

        public async Task<IEnumerable<TaskItemDto>> ViewAllTasksAsync()
        {
            return await context.TaskItem
                .AsNoTracking() // Performance optimization for read-only
                .Select(t => new TaskItemDto(
                    t.Id,
                    t.Title,
                    t.Description,
                    t.Status,
                    t.CreatedById,
                    t.DepartmentId,
                    t.CreatedAt,
                    t.UpdatedAt
                ))
                .ToListAsync();
        }

        public async Task<IEnumerable<TaskItemDto>> ViewAllTasksPerDeptAsync(int departmentId)
        {
            return await context.TaskItem
                .AsNoTracking()
                .Where(t => t.DepartmentId == departmentId)
                .Select(t => new TaskItemDto(
                    t.Id,
                    t.Title,
                    t.Description,
                    t.Status,
                    t.CreatedById,
                    t.DepartmentId,
                    t.CreatedAt,
                    t.UpdatedAt
                ))
                .ToListAsync();
        }
    }
}