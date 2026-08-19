using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using UserManagementPortal.Core.Interfaces;
using UserManagementPortal.Core.Models;

namespace UserManagementPortal.Services;

public class EmailSender(IOptions<EmailSettings> emailSettings, IWebHostEnvironment environment) : IEmailSender
{
    private readonly EmailSettings _emailSettings = emailSettings.Value;

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        if (string.IsNullOrWhiteSpace(_emailSettings.SmtpServer))
        {
            throw new InvalidOperationException("Настройки SMTP не заданы. Проверьте секцию 'EmailSettings' в appsettings.json.");
        }
        
        var emailMessage = new MimeMessage();

        emailMessage.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
        emailMessage.To.Add(new MailboxAddress("", email));
        emailMessage.Subject = subject;

        var bodyBuilder = new BodyBuilder { HtmlBody = htmlMessage };
        emailMessage.Body = bodyBuilder.ToMessageBody();

        using (var client = new SmtpClient())
        {
            try
            {
                await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.Port, SecureSocketOptions.Auto);
                    
                await client.AuthenticateAsync(_emailSettings.SenderEmail, _emailSettings.Password);
                    
                await client.SendAsync(emailMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка отправки письма: {ex.Message}");
                throw; 
            }
            finally
            {
                await client.DisconnectAsync(true);
            }
        }
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