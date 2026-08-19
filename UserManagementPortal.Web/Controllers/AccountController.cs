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
    private readonly IEmailSender _emailSender;

    public AccountController(IUserService userService, IEmailSender emailSender)
    {
        _userService = userService;
        _emailSender = emailSender;
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
            var confirmLink = Url.Action(
                "ConfirmEmail",
                "Account",
                new
                {
                    email = dto.Email,
                    token = result.ConfirmationToken
                },
                Request.Scheme)!;
            
            await _emailSender.SendConfirmationEmailAsync(dto.Email, confirmLink);
            
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

        return RedirectToAction("Index", "Admin");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }
    
    [HttpGet]
    public async Task<IActionResult> ConfirmEmail(string email, Guid token)
    {
        var confirmed = await _userService.ConfirmEmailAsync(email, token);
        TempData["StatusMessage"] = confirmed
            ? "Email подтверждён, теперь можно войти."
            : "Ссылка недействительна.";
        return RedirectToAction(nameof(Login));
    }
}