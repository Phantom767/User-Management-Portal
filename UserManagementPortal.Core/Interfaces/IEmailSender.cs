namespace UserManagementPortal.Core.Interfaces;

public interface IEmailSender
{
    Task SendEmailAsync(string email, string subject, string htmlMessage);
    Task SendConfirmationEmailAsync(string dtoEmail, string confirmLink);
}