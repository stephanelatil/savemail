namespace Backend.Services;

using System.Text;
using Microsoft.AspNetCore.Identity.UI.Services;
using SendGrid;
using SendGrid.Helpers.Mail;

public class SendGridEmailSender : IEmailSender
{
    private readonly string? sendGridKey = null;
    private readonly string? fromEmail = null;
    private readonly string? fromName = null;
    private readonly ILogger _logger;

    public SendGridEmailSender(IConfiguration configuration,
                       ILogger<SendGridEmailSender> logger)
    {
        this._logger = logger;
        this.sendGridKey = configuration.GetValue<string>("SendGrid:Key");
        this.fromEmail = configuration.GetValue<string>("SendGrid:FromEmail");
        this.fromName = configuration.GetValue<string>("SendGrid:FromName");
    }

    public async Task SendEmailAsync(string toEmail, string subject, string message)
    {
        if (string.IsNullOrEmpty(this.sendGridKey))
        {
            this._logger.LogWarning("Cannot send email. SendGrid key is not set");
            throw new KeyNotFoundException("Null SendGridKey");
        }
        await this.Execute(subject, message, toEmail);
    }

    private async Task Execute(string subject, string message, string toEmail)
    {
        if (this.fromEmail is null)
            throw new MissingFieldException("SendGrid FromEmail should be set in the config");
        var client = new SendGridClient(this.sendGridKey);
        var msg = new SendGridMessage()
        {
            From = new EmailAddress(this.fromEmail, this.fromName),
            Subject = subject,
            PlainTextContent = message,
            HtmlContent = message
        };
        msg.AddTo(new EmailAddress(toEmail));

        // Disable click tracking.
        // See https://sendgrid.com/docs/User_Guide/Settings/tracking.html
        msg.SetClickTracking(false, false);
        msg.SetOpenTracking(false);
        var response = await client.SendEmailAsync(msg);
        if (response.IsSuccessStatusCode)
            this._logger.LogDebug("Email to {} queued successfully!", toEmail);
        else
            this._logger.LogWarning("Failure Email to {} due to {} :\n{}", toEmail, response.StatusCode, response.Body);
    }
}

//TODO add MailGun

public class BrevoEmailSender : IEmailSender
{
    private readonly string? key = null;
    private readonly string? senderId = null;
    private readonly ILogger _logger;
    private readonly HttpClient _httpClient;

    public BrevoEmailSender(IConfiguration configuration,
                       ILogger<SendGridEmailSender> logger,
                       HttpClient httpClient)
    {
        this._logger = logger;
        this.key = configuration.GetValue<string>("Brevo:Key");
        this.senderId = configuration.GetValue<string>("Brevo:SenderId");
        this._httpClient = httpClient;
        this._httpClient.DefaultRequestHeaders.Add("accept", "application/json");
        this._httpClient.DefaultRequestHeaders.Add("content-type", "application/json");
        this._httpClient.DefaultRequestHeaders.Add("api-key", this.key);
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        if (this.key is null || this.senderId is null)
        {
            this._logger.LogWarning("Cannot send email. Brevo key or SenderId is not set");
            throw new KeyNotFoundException("Null Brevo Key or SenderId ");
        }
        await this.Execute(subject, htmlMessage, email);
    }

    private async Task Execute(string subject, string message, string toEmail)
    {
        string data = $"{{\"sender\":{{\"id\":{this.senderId} }},\"to\":[{{\"email\":\"{toEmail}\"}}],"+
                      $"\"subject\":\"{subject}\", \"htmlContent\":\"{message}\"}}";
        HttpContent content = new StringContent(data, Encoding.UTF8, "application/json");
        var response = await this._httpClient.PostAsync("https://api.brevo.com/v3/smtp/email",
                                         content);
        if (response.IsSuccessStatusCode)
            this._logger.LogDebug("Email to {} queued successfully!", toEmail);
        else
            this._logger.LogWarning("Failure Email to {} due to {} :\n{}", toEmail, response.StatusCode, response.Content.ToString());
    }
}