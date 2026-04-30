using Final_Project_Adv.Domain.DTO;

namespace Final_Project_Adv.Services
{
    public interface IAdminServices
    {
        // User Management
        Task<UsersDto> CreateUserAsync(CreateUserDto createUser);
        Task DeleteUserAsync(int id);
        Task UpdateUserAsync(UsersDto usersDto);

        // Department Management
        Task<DepartmentDto> CreateDeptAsync(CreateDepartmentDto createDept);
        Task DeleteDeptAsync(int id);
        Task UpdateDptAsync(DepartmentDto deptDto);

        // Permission Management (Admin deciding Manager permissions)
        Task<UserPermissionDto> GrantPermissionToUserAsync(GrantPermissionDto dto);
        Task<UserPermissionDto> RevokePermissionFromUserAsync(RevokePermissionDto dto);
        Task<UserPermissionDto> GetUserPermissionsAsync(int userId);
    }
}