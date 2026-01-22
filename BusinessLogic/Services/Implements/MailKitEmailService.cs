using BusinessLogic.DTOs.Settings;
using BusinessLogic.Services.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace BusinessLogic.Services.Implements;

public class MailKitEmailService : IEmailService
{
    private readonly SmtpOptions _options;

    public MailKitEmailService(IOptions<SmtpOptions> options)
    {
        _options = options.Value;
    }

    public async Task SendAsync(string toEmail, string subject, string htmlBody)
    {
        using var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_options.FromName, _options.FromEmail));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;

        var builder = new BodyBuilder
        {
            HtmlBody = htmlBody
        };

        message.Body = builder.ToMessageBody();

        using var client = new SmtpClient();

        var secureSocket =
            _options.Port == 465 ? SecureSocketOptions.SslOnConnect :
            _options.Port == 587 ? SecureSocketOptions.StartTls :
            (_options.EnableSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTlsWhenAvailable);


        await client.ConnectAsync(_options.Host, _options.Port, secureSocket);

        if (!string.IsNullOrWhiteSpace(_options.Username))
        {
            await client.AuthenticateAsync(_options.Username, _options.Password);
        }

        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
