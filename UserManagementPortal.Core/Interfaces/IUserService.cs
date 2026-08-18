using Microsoft.AspNetCore.Mvc;
using UserManagementPortal.Core.Common;
using UserManagementPortal.Core.Dtos;
using UserManagementPortal.Core.Entities;

namespace UserManagementPortal.Core.Interfaces;

public interface IUserService
{
    Task<RegisterResult> RegisterAsync(RegisterUserDto dto, CancellationToken ct = default);
    Task<LoginResult> LoginAsync(LoginDto dto, CancellationToken ct = default);
    Task<User?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<bool> ConfirmEmailAsync(string email, Guid token, CancellationToken ct = default);
}