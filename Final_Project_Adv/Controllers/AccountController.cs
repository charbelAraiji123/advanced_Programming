using Final_Project_Adv.Models;
using Final_Project_Adv.Services;
using Microsoft.AspNetCore.Mvc;

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

            // Store session values — required for audit logging and role checks
            HttpContext.Session.SetString("UserId", user.Id.ToString());
            HttpContext.Session.SetString("UserRole", user.Role);
            HttpContext.Session.SetString("UserName", user.Username);

            if (string.Equals(user.Role, UserRoles.Admin, StringComparison.OrdinalIgnoreCase))
                return RedirectToAction("AdminPanel", "AdminView");

            if (string.Equals(user.Role, UserRoles.Manager, StringComparison.OrdinalIgnoreCase))
                return Redirect("/Manager/Dashboard");

            if (string.Equals(user.Role, UserRoles.Employee, StringComparison.OrdinalIgnoreCase))
                return Redirect("/Employee/ViewTask");

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            Response.Cookies.Delete("AuthToken");
            return RedirectToAction("Login");
        }

        [HttpPost("api/Account/Login")]
        public async Task<IActionResult> ApiLogin([FromBody] LoginViewModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = await _managerServices.GetUserByEmailAsync(model.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
                return Unauthorized(new { message = "Invalid email or password." });

            var token = _jwtService.GenerateToken(user);
            return Ok(new
            {
                token = token,
                username = user.Username,
                role = user.Role,
                userId = user.Id
            });
        }
    }
}