using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserManagementPortal.Core.Dtos;
using UserManagementPortal.Core.Enums;
using UserManagementPortal.Core.Interfaces;

namespace UserManagementPortal.Controllers;

public class AccountController : Controller
{
    private readonly IUserService _userService;
    private readonly IEmailService _emailService;

    public AccountController(IUserService userService, IEmailService emailService)
    {
        _userService = userService;
        _emailService = emailService;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterUserDto dto)
    {
        if (!ModelState.IsValid)
            return View(dto);

        var result = await _userService.RegisterAsync(dto);

        if (result.Outcome == RegisterOutcome.Success)
        {
            var user = result.User!;
            
            var confirmationLink = Url.Action(
                "ConfirmEmail", 
                "Account", 
                new { userId = user.Id }, 
                protocol: Request.Scheme);

            _ = Task.Run(async () =>
            {
                try
                {
                    await _emailService.SendConfirmationEmailAsync(user.Email, user.Id.ToString(), confirmationLink!);
                }
                catch
                {
                    // Логирование ошибки отправки, если SMTP недоступен
                }
            });
            
            TempData["StatusMessage"] = "Регистрация прошла успешно. Проверьте почту для подтверждения.";
            return RedirectToAction("Login");
        }

        ModelState.AddModelError(
            result.Outcome == RegisterOutcome.EmailAlreadyExists ? nameof(dto.Email) : string.Empty,
            result.ErrorMessage!);

        return View(dto);
    }
    
    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginDto dto, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
            return View(dto);

        var result = await _userService.LoginAsync(dto);

        if (result.Outcome != LoginOutcome.Success)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage!);
            return View(dto);
        }

        var user = result.User!;
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Name),
            new(ClaimTypes.Email, user.Email)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties { IsPersistent = true });

        if (Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Index", "Users");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }
    
    [HttpGet]
    public async Task<IActionResult> ConfirmEmail(Guid userId)
    {
        await _userService.ConfirmEmailAsync(userId);

        TempData["Success"] = "Ваш e-mail успешно подтвержден!";
        return RedirectToAction("Login");
    }
}