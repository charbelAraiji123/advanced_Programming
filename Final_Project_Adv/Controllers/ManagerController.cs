using Final_Project_Adv.Models;
using Final_Project_Adv.Services;
using Microsoft.AspNetCore.Mvc;

public class ManagerController : Controller
{
    private readonly IManagerServices _managerServices;

    public ManagerController(IManagerServices managerServices)
    {
        _managerServices = managerServices;
    }

    public async Task<IActionResult> Dashboard()
    {
        var tasks = await _managerServices.GetDashboardTasksAsync();

        var vm = new TaskDashboardVm
        {
            Tasks = tasks.ToList()
        };

        return View(vm);
    }
}