using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using TheatreCenter.Services.Interfaces.Services;
using TheatreCenter.Services.Options;

namespace TheatreCenter.Services.Services;

public class SmtpEmailSender : IEmailSender
{
    private readonly EmailOptions _options;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IOptions<EmailOptions> options, ILogger<SmtpEmailSender> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public Task SendTwoFactorCodeAsync(string recipientEmail, string code, DateTime expiresAt)
    {
        var subject = "TheatreCenter verification code";
        var body = $"Your TheatreCenter verification code is: {code}. It is valid until {expiresAt:O}.";
        return SendEmailAsync(recipientEmail, subject, body);
    }

    public Task SendUnlockCodeAsync(string recipientEmail, string code, DateTime expiresAt)
    {
        var subject = "TheatreCenter unlock code";
        var body = $"Your unlock code is: {code}. The account will remain locked until you submit this code (expires at {expiresAt:O}).";
        return SendEmailAsync(recipientEmail, subject, body);
    }

    public Task SendLockoutNotificationAsync(string recipientEmail, DateTime lockedUntil)
    {
        var subject = "TheatreCenter account lockout";
        var body = $"Your account is locked until {lockedUntil:O} because of repeated failed attempts.";
        return SendEmailAsync(recipientEmail, subject, body);
    }

    private async Task SendEmailAsync(string recipientEmail, string subject, string body)
    {
        if (_options.DisableDelivery)
        {
            _logger.LogInformation("Email delivery disabled. Skipping email to {Recipient}: {Subject}", recipientEmail, subject);
            return;
        }

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_options.FromName, _options.From));
        message.To.Add(MailboxAddress.Parse(recipientEmail));
        message.Subject = subject;
        message.Body = new TextPart("plain") { Text = body };

        using var client = new SmtpClient();
        await client.ConnectAsync(_options.SmtpHost, _options.SmtpPort, _options.UseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None);

        var password = ResolvePassword();
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException("Email password is not configured. Set Email:Password or E2E_MAIL_PASSWORD environment variable.");
        }

        await client.AuthenticateAsync(_options.Username, password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);

        _logger.LogInformation("Email sent to {Recipient} with subject {Subject}", recipientEmail, subject);
    }

    private string ResolvePassword()
    {
        if (!string.IsNullOrWhiteSpace(_options.Password))
        {
            return _options.Password;
        }

        var envPassword = Environment.GetEnvironmentVariable("E2E_MAIL_PASSWORD");
        return envPassword ?? string.Empty;
    }
}
