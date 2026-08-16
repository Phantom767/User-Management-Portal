using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using UserManagementPortal.Core.Interfaces;
using UserManagementPortal.Core.Models;

namespace UserManagementPortal.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;

    public EmailService(IOptions<EmailSettings> settings)
    {
        _settings = settings.Value;
    }

    public string getUniqIdValue()
    {
        return Guid.NewGuid().ToString("N");
    }

    public async Task SendConfirmationEmailAsync(string toEmail, string userId, string confirmationLink)
    {
        var email = new MimeMessage();
        email.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
        email.To.Add(MailboxAddress.Parse(toEmail));
        email.Subject = "Подтверждение регистрации";

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = $@"
                <h3>Добро пожаловать!</h3>
                <p>Для подтверждения вашего аккаунта перейдите по ссылке ниже:</p>
                <p><a href='{confirmationLink}'>Подтвердить Email</a></p>
                <br/>
                <small>Request Token: {getUniqIdValue()}</small>"
        };

        email.Body = bodyBuilder.ToMessageBody();

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(_settings.SmtpServer, _settings.SmtpPort, SecureSocketOptions.StartTls);
        await smtp.AuthenticateAsync(_settings.Username, _settings.Password);
        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);
    }
    
    public async Task SendPasswordResetEmailAsync(string toEmail, string resetLink)
    {
        var email = new MimeMessage();
        email.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
        email.To.Add(MailboxAddress.Parse(toEmail));
        email.Subject = "Сброс пароля";

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = $@"
                <h3>Сброс пароля</h3>
                <p>Для сброса вашего пароля перейдите по ссылке ниже:</p>
                <p><a href='{resetLink}'>Сбросить пароль</a></p>"
        };

        email.Body = bodyBuilder.ToMessageBody();

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(_settings.SmtpServer, _settings.SmtpPort, SecureSocketOptions.StartTls);
        await smtp.AuthenticateAsync(_settings.Username, _settings.Password);
        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);
    }
    
    public async Task SendEmailAsync(string toEmail, string subject, string htmlContent)
    {
        var email = new MimeMessage();
        email.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
        email.To.Add(MailboxAddress.Parse(toEmail));
        email.Subject = subject;

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = htmlContent
        };

        email.Body = bodyBuilder.ToMessageBody();

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(_settings.SmtpServer, _settings.SmtpPort, SecureSocketOptions.StartTls);
        await smtp.AuthenticateAsync(_settings.Username, _settings.Password);
        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);
    }
}