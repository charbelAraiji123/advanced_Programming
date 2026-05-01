using Final_Project_Adv.Models;
using Final_Project_Adv.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Final_Project_Adv.Controllers
{
    public class AccountController : Controller
    {
        private readonly IManagerServices _managerServices;
        private readonly JwtService _jwtService;

        public AccountController(IManagerServices managerServices, JwtService jwtService)
        {
            _managerServices = managerServices;
            _jwtService = jwtService;
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _managerServices.GetUserByEmailAsync(model.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                return View(model);
            }

            // Set Session
            HttpContext.Session.SetString("UserRole", user.Role);
            HttpContext.Session.SetString("UserName", user.Username);

            // Redirect based on role
            if (string.Equals(user.Role, UserRoles.Admin, StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("AdminPanel", "AdminView"); // Points to AdminViewController
            }

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            Response.Cookies.Delete("AuthToken");
            return RedirectToAction("Login");
        }
    }
}