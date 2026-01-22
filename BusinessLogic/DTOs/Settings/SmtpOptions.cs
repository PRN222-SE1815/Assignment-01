namespace BusinessLogic.DTOs.Settings;

public class SmtpOptions
{
    public string Host { get; set; } = null!;
    public int Port { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public bool EnableSsl { get; set; }
    public string FromEmail { get; set; } = null!;
    public string FromName { get; set; } = null!;
}
