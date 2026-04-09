
using Final_Project_Adv.Domain.DTO;
using System.Collections.Generic;


namespace Final_Project_Adv.Services;

public interface ITest 
{
    Task<DepartmentDto> InsertDepartment(CreateDepartmentDto dto);
    Task<UsersDto> InsertUser(CreateUserDto dto);
}
