using System.Security.Claims;
using HireHub.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace HireHub.Controllers
{
    public class HomeController : Controller
    {
        // GET: /
        public IActionResult Index()
        {
            // If user is logged in, redirect based on role
            if (User.Identity?.IsAuthenticated == true)
            {
                var role = User.FindFirst(ClaimTypes.Role)?.Value;

                return role switch
                {
                    RoleConstants.Admin => RedirectToAction("Index", "Admin"),
                    RoleConstants.Recruiter => RedirectToAction("Index", "Recruiter"),
                    RoleConstants.JobSeeker => RedirectToAction("Index", "JobSeeker"),
                    _ => View()
                };
            }

            // Not logged in → show welcome page
            return View();
        }

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}