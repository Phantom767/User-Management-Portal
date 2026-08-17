namespace UserManagementPortal.Core.Common;

public class BulkActionResult
{
    public bool Success { get; init; }
    public int AffectedCount { get; init; }
    public bool AffectedCurrentUser { get; init; }

    public static BulkActionResult Ok(int count, bool affectedCurrentUser = false) =>
        new() { Success = true, AffectedCount = count, AffectedCurrentUser = affectedCurrentUser };
}