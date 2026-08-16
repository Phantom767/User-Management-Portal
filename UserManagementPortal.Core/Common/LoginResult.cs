using UserManagementPortal.Core.Entities;
using UserManagementPortal.Core.Enums;

namespace UserManagementPortal.Core.Common;

public class LoginResult
{
    public LoginOutcome Outcome { get; init; }
    public string? ErrorMessage { get; init; }
    public User? User { get; init; }

    public static LoginResult Success(User user) =>
        new() { Outcome = LoginOutcome.Success, User = user };

    public static LoginResult Fail(LoginOutcome outcome, string message) =>
        new() { Outcome = outcome, ErrorMessage = message };
}