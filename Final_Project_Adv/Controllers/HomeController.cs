using Microsoft.AspNetCore.Mvc;

namespace Final_Project_Adv.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}