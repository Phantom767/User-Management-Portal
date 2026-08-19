using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserManagementPortal.Core.Common;
using UserManagementPortal.Core.Dtos;
using UserManagementPortal.Core.Interfaces;

namespace UserManagementPortal.Controllers;

[Authorize]
public class AdminController : Controller
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService) => _adminService = adminService;

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var users = await _adminService.GetUsersAsync(ct);
        return View(users);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> Block([FromBody] BulkActionDto dto, CancellationToken ct) =>
        RunBulkAction(dto, _adminService.BlockUsersAsync, ct);

    [HttpPost]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> Unblock([FromBody] BulkActionDto dto, CancellationToken ct) =>
        RunBulkAction(dto, _adminService.UnblockUsersAsync, ct);

    [HttpPost]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> Delete([FromBody] BulkActionDto dto, CancellationToken ct) =>
        RunBulkAction(dto, _adminService.DeleteUsersAsync, ct);

    [HttpPost]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> DeleteUnverified([FromBody] BulkActionDto dto, CancellationToken ct) =>
        RunBulkAction(dto, _adminService.DeleteUnverifiedUsersAsync, ct);

    private async Task<IActionResult> RunBulkAction(
        BulkActionDto dto,
        Func<List<Guid>, Guid, CancellationToken, Task<BulkActionResult>> action,
        CancellationToken ct)
    {
        if (dto?.Ids is null || dto.Ids.Count == 0)
            return BadRequest(new { success = false, message = "Не выбрано ни одного пользователя." });

        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(userIdStr, out var currentUserId))
        {
            return Unauthorized(new { success = false, message = "Сессия недействительна." });
        }

        var result = await action(dto.Ids, currentUserId, ct);

        if (result.AffectedCurrentUser)
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }

        return Json(new
        {
            success = result.Success,
            affectedCount = result.AffectedCount,
            redirectToLogin = result.AffectedCurrentUser
        });
    }
}