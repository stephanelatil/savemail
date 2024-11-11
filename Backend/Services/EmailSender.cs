namespace Backend.Services;

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

    public async Task Execute(string subject, string message, string toEmail)
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

    //TODO add method to customize Email with an icon or some styling
}