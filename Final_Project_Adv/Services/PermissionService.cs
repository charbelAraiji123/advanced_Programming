using Final_Project_Adv.Domain.DTO;
using Final_Project_Adv.Domain.Enums;
using Final_Project_Adv.Infrastructure.Data;
using Final_Project_Adv.Models;
using Microsoft.EntityFrameworkCore;

public class PermissionService(AppDbContext context)
{
    public async Task<bool> HasPermissionAsync(int userId, PermissionType permission)
    {
        var user = await context.Users.FindAsync(userId);
        if (user?.Role == "Admin") return true;

        return await context.UserPermission
            .AnyAsync(p => p.UserId == userId && p.Permission == permission);
    }

    public async Task RequirePermissionAsync(int userId, PermissionType permission)
    {
        if (!await HasPermissionAsync(userId, permission))
            throw new UnauthorizedAccessException($"User {userId} does not have the '{permission}' permission.");
    }

    public async Task<UserPermissionDto> GrantPermissionAsync(GrantPermissionDto dto)
    {
        var exists = await context.UserPermission.AnyAsync(
            p => p.UserId == dto.UserId && p.Permission == dto.Permission);

        if (!exists)
        {
            context.UserPermission.Add(new UserPermission
            {
                UserId = dto.UserId,
                Permission = dto.Permission,
                GrantedById = dto.GrantedById,
                GrantedAt = DateTime.UtcNow
            });
            await context.SaveChangesAsync();
        }
        return await GetUserPermissionsAsync(dto.UserId);
    }

    public async Task<UserPermissionDto> RevokePermissionAsync(RevokePermissionDto dto)
    {
        await context.UserPermission
            .Where(p => p.UserId == dto.UserId && p.Permission == dto.Permission)
            .ExecuteDeleteAsync();

        return await GetUserPermissionsAsync(dto.UserId);
    }

    public async Task<UserPermissionDto> GetUserPermissionsAsync(int userId)
    {
        var user = await context.Users
            .Include(u => u.Permissions)
            .FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new KeyNotFoundException($"User {userId} not found.");

        return new UserPermissionDto
        {
            UserId = user.Id,
            Username = user.Username,
            Permissions = user.Permissions.Select(p => p.Permission.ToString()).ToList()
        };
    }
}