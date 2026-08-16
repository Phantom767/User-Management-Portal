namespace UserManagementPortal.Core.Interfaces
{
    public interface IEmailService
    {
        Task SendConfirmationEmailAsync(string toEmail, string userId, string confirmationLink);
    }
}