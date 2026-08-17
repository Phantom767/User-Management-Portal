namespace UserManagementPortal.Core.Dtos;

public record UserListItemDto(Guid Id, string Name, string Email, string Status, DateTime? LastSeenAt);