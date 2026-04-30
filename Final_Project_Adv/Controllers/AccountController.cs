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


            // Session Storage
            HttpContext.Session.SetString("UserRole", user.Role);
            HttpContext.Session.SetString("UserName", user.Username);

            var token = _jwtService.GenerateToken(user);
            Response.Cookies.Append("AuthToken", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddMinutes(60)
            });

            // Role-Based Redirection
            if (string.Equals(user.Role, UserRoles.Admin, StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("AdminPanel", "Admin");
            }

            return RedirectToAction("Index", "Home");

            // ✅ CREATE CLAIMS (THIS IS THE CONNECTION POINT)
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, user.Username),
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Role, user.Role) // 🔥 CRITICAL
    };

            var identity = new ClaimsIdentity(claims, "CookieAuth");
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync("CookieAuth", principal);

            // ✅ ROLE-BASED REDIRECT
            if (user.Role == "Admin")
                return RedirectToAction("Index", "Admin");

            if (user.Role == "Manager")
                return RedirectToAction("Dashboard", "Manager");

            return RedirectToAction("Index", "Employee");

        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            Response.Cookies.Delete("AuthToken");
            return RedirectToAction("Login");
        }
    }
}