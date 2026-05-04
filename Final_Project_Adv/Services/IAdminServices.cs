using Final_Project_Adv.Domain.DTO;

namespace Final_Project_Adv.Services
{
    public interface IAdminServices
    {
        // User
        Task<UsersDto> CreateUserAsync(CreateUserDto dto, int performedById);
        Task DeleteUserAsync(int id, int performedById);
        Task UpdateUserAsync(UsersDto usersDto, int performedById);

        // Permissions
        Task<UserPermissionDto> GrantPermissionToUserAsync(GrantPermissionDto dto);
        Task<UserPermissionDto> RevokePermissionFromUserAsync(RevokePermissionDto dto);
        Task<UserPermissionDto> GetUserPermissionsAsync(int userId);

        // Department
        Task<DepartmentDto> CreateDeptAsync(CreateDepartmentDto dto, int performedById);
        Task DeleteDeptAsync(int id, int performedById);
        Task UpdateDptAsync(DepartmentDto deptDto, int performedById);

        Task<IEnumerable<TaskItemDto>> ViewAllTasksAsync();
        Task<IEnumerable<TaskItemDto>> ViewAllTasksPerDeptAsync(int departmentId);


    }
}