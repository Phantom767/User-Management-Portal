using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using UserManagementPortal.Core.Enums;
using UserManagementPortal.Core.Interfaces;

namespace UserManagementPortal.Middleware;

public class UserStatusMiddleware
{
    private readonly RequestDelegate _next;

    public UserStatusMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IUserService userService)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var idClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (idClaim is not null && int.TryParse(idClaim, out var userId))
            {
                var user = await userService.GetByIdAsync(userId, context.RequestAborted);

                if (user is null || user.CurrentStatus == UserStatus.Blocked)
                {
                    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    context.Response.Redirect("/Account/Login");
                    return;
                }
            }
        }

        await _next(context);
    }
}