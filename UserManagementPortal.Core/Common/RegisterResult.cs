using UserManagementPortal.Core.Entities;
using UserManagementPortal.Core.Enums;

namespace UserManagementPortal.Core.Common;

public class RegisterResult
{
    public RegisterOutcome Outcome { get; init; }
    public string? ErrorMessage { get; init; }
    public User? User { get; init; }

    public static RegisterResult Success(User user) =>
        new() { Outcome = RegisterOutcome.Success, User = user };

    public static RegisterResult Fail(RegisterOutcome outcome, string message) =>
        new() { Outcome = outcome, ErrorMessage = message };
}