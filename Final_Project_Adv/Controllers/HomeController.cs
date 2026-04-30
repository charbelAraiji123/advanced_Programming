using Microsoft.AspNetCore.Mvc;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        // Retrieve the role from session
        ViewBag.Role = HttpContext.Session.GetString("UserRole") ?? "Guest";
        ViewBag.Username = HttpContext.Session.GetString("UserName") ?? "User";

        return View();
    }
}