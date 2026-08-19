using UserManagementPortal.Core.Entities;
using UserManagementPortal.Core.Enums;

namespace UserManagementPortal.Core.Common;

public class RegisterResult
{
    public RegisterOutcome Outcome { get; init; }
    public string? ErrorMessage { get; init; }
    public Guid ConfirmationToken { get; set; }

    public static RegisterResult Success(User user) =>
        new() { Outcome = RegisterOutcome.Success, ConfirmationToken = user.EmailConfirmationToken };

    public static RegisterResult Fail(RegisterOutcome outcome, string message) =>
        new() { Outcome = outcome, ErrorMessage = message };
}