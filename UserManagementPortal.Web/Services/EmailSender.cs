using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using UserManagementPortal.Core.Interfaces;
using UserManagementPortal.Core.Models;

namespace UserManagementPortal.Services;

public class EmailSender(IOptions<EmailSettings> emailSettings, IWebHostEnvironment environment, IConfiguration config, IHttpClientFactory httpClientFactory) : IEmailSender
{
    private readonly EmailSettings _emailSettings = emailSettings.Value;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        var client = _httpClientFactory.CreateClient();
        
        var apiKey = config[_emailSettings.ApiKey]; 
        
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://api.resend.com/emails");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        var payload = new
        {
            from = _emailSettings.SenderEmail,
            to = new[] { email },
            subject = subject,
            html = htmlMessage
        };

        requestMessage.Content = new StringContent(
            JsonSerializer.Serialize(payload), 
            Encoding.UTF8, 
            "application/json"
        );

        var response = await client.SendAsync(requestMessage);
        
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Ошибка отправки через Resend API: {error}");
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