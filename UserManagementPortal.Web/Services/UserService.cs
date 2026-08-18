using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using UserManagementPortal.Core.Common;
using UserManagementPortal.Core.Dtos;
using UserManagementPortal.Core.Entities;
using UserManagementPortal.Core.Enums;
using UserManagementPortal.Core.Interfaces;
using UserManagementPortal.Persistence;

namespace UserManagementPortal.Services;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _db;

    public UserService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<RegisterResult> RegisterAsync(RegisterUserDto dto, CancellationToken ct = default)
    {
        var user = new User
        {
            Name = dto.Name,
            Email = dto.Email.Trim().ToLowerInvariant(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Status = UserStatus.Unverified,
            CreatedAt = DateTime.UtcNow
        };

        user.EmailConfirmationToken = Guid.NewGuid();
        _db.Users.Add(user);

        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (IsUniqueEmailViolation(ex))
        {
            return RegisterResult.Fail(
                RegisterOutcome.EmailAlreadyExists,
                "Пользователь с таким email уже зарегистрирован.");
        }
        catch (DbUpdateException)
        {
            return RegisterResult.Fail(
                RegisterOutcome.UnknownError,
                "Не удалось выполнить регистрацию. Попробуйте позже.");
        }
        
        return RegisterResult.Success(user);
    }

    private static bool IsUniqueEmailViolation(DbUpdateException ex)
    {
        return ex.InnerException is PostgresException pgEx
            && pgEx.SqlState == PostgresErrorCodes.UniqueViolation
            && pgEx.ConstraintName == "UX_Users_Email";
    }
    
    public async Task<LoginResult> LoginAsync(LoginDto dto, CancellationToken ct = default)
    {
        var email = dto.Email.Trim().ToLowerInvariant();
        var user = await _db.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email, ct);

        if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return LoginResult.Fail(LoginOutcome.InvalidCredentials, "Неверный email или пароль.");

        if (user.Status == UserStatus.Blocked)
            return LoginResult.Fail(LoginOutcome.Blocked, "Аккаунт заблокирован.");

        return LoginResult.Success(user);
    }

    public async Task<bool> ConfirmEmailAsync(string email, Guid token, CancellationToken ct = default)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant(), ct);

        if (user is null || user.EmailConfirmationToken != token || user.Status == UserStatus.Blocked)
            return false;

        if (user.Status == UserStatus.Unverified)
            user.Status = UserStatus.Active;

        await _db.SaveChangesAsync(ct);
        return true;
    }

    public Task<User?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _db.Users.AsNoTracking().FirstOrDefaultAsync(u => Equals(u.Id, id), ct);
}