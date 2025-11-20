using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims; // For authentication claims
using Prog6212.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Prog6212.Controllers
{
    public class AccountController : Controller
    {
        // Simulated in-memory user store
        private static readonly List<User> Users = new()
        {
            new User { Id = 1, FullName = "Dr. John Smith", Email = "lecturer@example.com", Password = "Test@123", Role = "Lecturer" },
            new User { Id = 2, FullName = "Ms. Sarah Wilson", Email = "coordinator@example.com", Password = "Test@123", Role = "Coordinator" },
            new User { Id = 3, FullName = "Mr. David Brown", Email = "manager@example.com", Password = "Test@123", Role = "Manager" }
        };

        // GET: Login
        [HttpGet]
        public IActionResult Login() => View();

        // POST: Login
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = Users.FirstOrDefault(u =>
                u.Email.Equals(email, System.StringComparison.OrdinalIgnoreCase)
                && u.Password == password);

            if (user == null)
            {
                ViewBag.Error = "Invalid email or password.";
                return View();
            }

            var claims = new List<System.Security.Claims.Claim>
            {
                new System.Security.Claims.Claim(ClaimTypes.Name, user.FullName),
                new System.Security.Claims.Claim(ClaimTypes.Email, user.Email),
                new System.Security.Claims.Claim(ClaimTypes.Role, user.Role),
                new System.Security.Claims.Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return RedirectToAction("Index", "Dashboard");
        }

        // GET: Register
        [HttpGet]
        public IActionResult Register() => View();

        // POST: Register
        [HttpPost]
        public IActionResult Register(string fullName, string email, string password, string role)
        {
            if (Users.Any(u => u.Email.Equals(email, System.StringComparison.OrdinalIgnoreCase)))
            {
                ViewBag.Error = "Email already exists. Try logging in.";
                return View();
            }

            var newUser = new User
            {
                Id = Users.Count + 1,
                FullName = fullName,
                Email = email,
                Password = password,
                Role = role
            };

            Users.Add(newUser);
            ViewBag.Success = "Registration successful! You can now log in.";
            return RedirectToAction("Login");
        }

        // GET: Logout
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied() => View();
    }
}
