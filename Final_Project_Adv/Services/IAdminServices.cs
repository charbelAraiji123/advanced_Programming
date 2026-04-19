using Final_Project_Adv.Domain.DTO;

namespace Final_Project_Adv.Services
{
    public interface IAdminServices
    {
        //User
        Task<UsersDto> CreateUserAsync(CreateUserDto createUser);
        Task DeleteUserAsync(int id);
        Task UpdateUserAsync(UsersDto usersDto);

        //Department
        Task<DepartmentDto> CreateDeptAsync(CreateDepartmentDto createDept);
        Task DeleteDeptAsync(int id);
        Task UpdateDptAsync(DepartmentDto deptDto);



    }
}
