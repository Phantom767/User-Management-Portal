using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserManagementPortal.Models;

namespace UserManagementPortal.Controllers;

[AllowAnonymous]
public class HomeController : Controller
{
    public IActionResult Index() =>
        User.Identity?.IsAuthenticated == true
            ? RedirectToAction("Index", "Admin")
            : RedirectToAction("Login", "Account");

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}