using UserManagementPortal.Core.Common;
using UserManagementPortal.Core.Dtos;

namespace UserManagementPortal.Core.Interfaces;

public interface IAdminService
{
    Task<List<UserListItemDto>> GetUsersAsync(CancellationToken ct = default);

    Task<BulkActionResult> BlockUsersAsync(List<Guid> ids, Guid currentUserId, CancellationToken ct = default);
    Task<BulkActionResult> UnblockUsersAsync(List<Guid> ids, Guid currentUserId, CancellationToken ct = default);
    Task<BulkActionResult> DeleteUsersAsync(List<Guid> ids, Guid currentUserId, CancellationToken ct = default);
    Task<BulkActionResult> DeleteUnverifiedUsersAsync(List<Guid> ids, Guid currentUserId, CancellationToken ct = default);
}