using Microsoft.Extensions.Options;
using Resend;
using UserManagementPortal.Core.Interfaces;
using UserManagementPortal.Core.Models;

namespace UserManagementPortal.Services;

public class EmailSender(IOptions<EmailSettings> emailSettings, IWebHostEnvironment environment, IResend resend) : IEmailSender
{
    private readonly EmailSettings _emailSettings = emailSettings.Value;

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        var message = new EmailMessage();
        message.From = _emailSettings.SenderEmail;
        message.To.Add(email);
        message.Subject = subject;
        message.HtmlBody = htmlMessage;
        
        await resend.EmailSendAsync(message);
    }
        
    public async Task SendConfirmationEmailAsync(
        string email,
        string confirmLink)
    {
        var templatePath = Path.Combine(
            environment.ContentRootPath,
            "EmailTemplates",
            "ConfirmEmail.html");

        var html = await File.ReadAllTextAsync(templatePath);

        html = html
            .Replace("{{EMAIL}}", email)
            .Replace("{{CONFIRM_LINK}}", confirmLink);

        await SendEmailAsync(
            email,
            "Подтверждение аккаунта — UserManagementPortal",
            html);
    }
}