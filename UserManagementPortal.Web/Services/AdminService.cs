using Microsoft.EntityFrameworkCore;
using UserManagementPortal.Core.Common;
using UserManagementPortal.Core.Dtos;
using UserManagementPortal.Core.Enums;
using UserManagementPortal.Core.Interfaces;
using UserManagementPortal.Persistence;

namespace UserManagementPortal.Services;

public class AdminService : IAdminService
{
    private readonly ApplicationDbContext _db;

    public AdminService(ApplicationDbContext db) => _db = db;

    public Task<List<UserListItemDto>> GetUsersAsync(CancellationToken ct = default) =>
        _db.Users
            .AsNoTracking()
            .OrderByDescending(u => u.LastLoginAt ?? u.CreatedAt)
            .Select(u => new UserListItemDto(u.Id, u.Name, u.Email, u.CurrentStatus.ToString(), u.LastLoginAt))
            .ToListAsync(ct);

    public async Task<BulkActionResult> BlockUsersAsync(List<Guid> ids, Guid currentUserId, CancellationToken ct = default)
    {
        if (ids.Count == 0) return BulkActionResult.Ok(0);

        await _db.Users
            .Where(u => ids.Contains(u.Id))
            .ExecuteUpdateAsync(s => s.SetProperty(u => u.PreviousStatus, u => u.CurrentStatus), ct);
        
        var affected = await _db.Users
            .Where(u => ids.Contains(u.Id))
            .ExecuteUpdateAsync(s => s.SetProperty(u => u.CurrentStatus, UserStatus.Blocked), ct);

        return BulkActionResult.Ok(affected, ids.Contains(currentUserId));
    }

    public async Task<BulkActionResult> UnblockUsersAsync(List<Guid> ids, Guid currentUserId, CancellationToken ct = default)
    {
        if (ids.Count == 0) return BulkActionResult.Ok(0);

        var affected = await _db.Users
            .Where(u => ids.Contains(u.Id))
            .ExecuteUpdateAsync(s => s.SetProperty(u => u.CurrentStatus, u => u.PreviousStatus), ct);

        return BulkActionResult.Ok(affected);
    }

    public async Task<BulkActionResult> DeleteUsersAsync(List<Guid> ids, Guid currentUserId, CancellationToken ct = default)
    {
        if (ids.Count == 0) return BulkActionResult.Ok(0);

        var affected = await _db.Users
            .Where(u => ids.Contains(u.Id))
            .ExecuteDeleteAsync(ct);

        return BulkActionResult.Ok(affected, ids.Contains(currentUserId));
    }

    public async Task<BulkActionResult> DeleteUnverifiedUsersAsync(List<Guid> ids, Guid currentUserId, CancellationToken ct = default)
    {
        if (ids.Count == 0) return BulkActionResult.Ok(0);

        var toDelete = await _db.Users
            .Where(u => ids.Contains(u.Id) && u.CurrentStatus == UserStatus.Unverified)
            .Select(u => u.Id)
            .ToListAsync(ct);

        if (toDelete.Count == 0) return BulkActionResult.Ok(0);

        await _db.Users.Where(u => toDelete.Contains(u.Id)).ExecuteDeleteAsync(ct);

        return BulkActionResult.Ok(toDelete.Count, toDelete.Contains(currentUserId));
    }
}