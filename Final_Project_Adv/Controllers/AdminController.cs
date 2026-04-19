using Microsoft.AspNetCore.Mvc;

namespace Final_Project_Adv.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
